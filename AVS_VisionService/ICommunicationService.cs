using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AVS_Service
{
    public interface ICommunicationService
    {
        void Start(string connectionId, CommProtocol protocol, CommRole role, string ip, int port);
        void Stop(string connectionId);
        Task SendAsync(string connectionId, string message);
        bool IsActive(string connectionId);
        event Action<string, string> MessageReceived;       // (connectionId, message)
        event Action<string> LogMessage;
        event Action<string, bool> ConnectionStatusChanged; // (connectionId, isConnected)

    }

    public class CommunicationService : ICommunicationService, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConnectionContext> _connections = new();
        private readonly ILogger _logger;

        public event Action<string, string> MessageReceived;
        public event Action<string> LogMessage;
        public event Action<string, bool> ConnectionStatusChanged;

        public CommunicationService(ILogger logger)
        {
            _logger = logger;
        }

        public void Start(string connectionId, CommProtocol protocol, CommRole role, string ip, int port)
        {
            if (_connections.TryGetValue(connectionId, out var existing))
            {
                _logger.Warning("[{Id}] 连接已存在，先停止", connectionId);
                Stop(connectionId);
            }

            var ctx = new ConnectionContext
            {
                ConnectionId = connectionId,
                Protocol = protocol,
                Role = role,
                IP = ip,
                Port = port,
                Cts = new CancellationTokenSource()
            };

            if (protocol == CommProtocol.TCP)
            {
                if (role == CommRole.Server)
                    StartTcpServer(ctx);
                else
                    StartTcpClient(ctx);
            }
            else if (protocol == CommProtocol.UDP)
            {
                StartUdp(ctx);
            }

            _connections[connectionId] = ctx;
        }

        private void StartUdp(ConnectionContext ctx)
        {
            try
            {
                if (ctx.Role == CommRole.Server)
                {
                    ctx.UdpClient = new UdpClient(ctx.Port);
                    _logger.Information("[{Id}] UDP Server 监听端口 {Port}", ctx.ConnectionId, ctx.Port);
                }
                else // Client
                {
                    ctx.UdpClient = new UdpClient();
                    ctx.TargetEndPoint = new IPEndPoint(IPAddress.Parse(ctx.IP), ctx.Port);
                    _logger.Information("[{Id}] UDP Client 目标 {IP}:{Port}", ctx.ConnectionId, ctx.IP, ctx.Port);
                }

                ctx.IsActive = true;
                ConnectionStatusChanged?.Invoke(ctx.ConnectionId, true);

                // 启动接收循环
                _ = Task.Run(() => HandleUdpReceive(ctx), ctx.Cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{Id}] 启动 UDP 失败", ctx.ConnectionId);
                ctx.IsActive = false;
                ConnectionStatusChanged?.Invoke(ctx.ConnectionId, false);
            }
        }

        public void Stop(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var ctx))
            {
                ctx.Cts?.Cancel();
                ctx.TcpListener?.Stop();
                ctx.TcpClient?.Close();
                ctx.TcpServerClient?.Close();
                ctx.UdpClient?.Close();
                ctx.IsActive = false;
                _logger.Information("[{Id}] 连接已停止", connectionId);
                ConnectionStatusChanged?.Invoke(connectionId, false);
            }
        }

        public bool IsActive(string connectionId) =>
            _connections.TryGetValue(connectionId, out var ctx) && ctx.IsActive;

        public async Task SendAsync(string connectionId, string message)
        {
            if (!_connections.TryGetValue(connectionId, out var ctx) || !ctx.IsActive)
            {
                _logger.Warning("[{Id}] 连接不可用，无法发送", connectionId);
                return;
            }

            byte[] data = Encoding.UTF8.GetBytes(message);
            try
            {
                if (ctx.Protocol == CommProtocol.TCP)
                {
                    NetworkStream stream = null;
                    if (ctx.Role == CommRole.Server)
                        stream = ctx.TcpServerClient?.GetStream();
                    else
                        stream = ctx.TcpClient?.GetStream();

                    if (stream != null && stream.CanWrite)
                    {
                        await stream.WriteAsync(data, 0, data.Length, ctx.Cts.Token);
                        _logger.Information("[{Id}] 发送消息: {Msg}", connectionId, message);
                    }
                }
                else if (ctx.Protocol == CommProtocol.UDP)
                {
                    if (ctx.UdpClient != null)
                    {
                        // Server 模式：向记录的远程端点发送
                        if (ctx.Role == CommRole.Server && ctx.RemoteEndPoint != null)
                            await ctx.UdpClient.SendAsync(data, data.Length, ctx.RemoteEndPoint);
                        // Client 模式：向配置的目标端点发送
                        else if (ctx.Role == CommRole.Client)
                            await ctx.UdpClient.SendAsync(data, data.Length, ctx.TargetEndPoint);
                        else
                            _logger.Warning("[{Id}] UDP 没有可用的发送目标", connectionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{Id}] 发送失败", connectionId);
            }
        }

        private void StartTcpServer(ConnectionContext ctx)
        {
            ctx.TcpListener = new TcpListener(IPAddress.Any, ctx.Port);
            ctx.TcpListener.Start();
            _logger.Information("[{Id}] TCP Server 监听端口 {Port}", ctx.ConnectionId, ctx.Port);

            Task.Run(async () =>
            {
                while (!ctx.Cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var client = await ctx.TcpListener.AcceptTcpClientAsync(ctx.Cts.Token);
                        string remote = client.Client.RemoteEndPoint.ToString();
                        _logger.Information("[{Id}] 客户端连接: {Remote}", ctx.ConnectionId, remote);
                        ctx.TcpServerClient = client;
                        ctx.IsActive = true;
                        ConnectionStatusChanged?.Invoke(ctx.ConnectionId, true);

                        // 处理该客户端消息
                        _ = Task.Run(() => HandleTcpSession(ctx.ConnectionId, client, ctx.Cts.Token));
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "[{Id}] 接受客户端异常", ctx.ConnectionId);
                        break;
                    }
                }
            }, ctx.Cts.Token);
        }

        //private void StartTcpClient(ConnectionContext ctx)
        //{
        //    Task.Run(async () =>
        //    {
        //        while (!ctx.Cts.Token.IsCancellationRequested)
        //        {
        //            try
        //            {
        //                var client = new TcpClient();
        //                await client.ConnectAsync(ctx.IP, ctx.Port);
        //                ctx.TcpClient = client;
        //                ctx.IsActive = true;
        //                ConnectionStatusChanged?.Invoke(ctx.ConnectionId, true);
        //                _logger.Information("[{Id}] TCP Client 已连接 {IP}:{Port}", ctx.ConnectionId, ctx.IP, ctx.Port);

        //                await HandleTcpSession(ctx.ConnectionId, client, ctx.Cts.Token);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error(ex, "[{Id}] 连接失败，5秒后重试", ctx.ConnectionId);
        //                ctx.IsActive = false;
        //                ConnectionStatusChanged?.Invoke(ctx.ConnectionId, false);
        //                await Task.Delay(5000, ctx.Cts.Token);
        //            }
        //        }
        //    }, ctx.Cts.Token);
        //}

        private void StartTcpClient(ConnectionContext ctx)
        {
            Task.Run(async () =>
            {
                int consecutiveFailCount = 0; //连续失败计数器
                const int logInterval = 3;   // 每失败3次输出一次错误日志

                while (!ctx.Cts.Token.IsCancellationRequested)
                {
                    TcpClient client = null;
                    try
                    {
                        client = new TcpClient();
                        await client.ConnectAsync(ctx.IP, ctx.Port);
                        ctx.TcpClient = client;
                        ctx.IsActive = true;
                        consecutiveFailCount = 0; //连接成功，重置失败计数

                        ConnectionStatusChanged?.Invoke(ctx.ConnectionId, true);
                        _logger.Information("[{Id}] TCP Client 已连接 {IP}:{Port}", ctx.ConnectionId, ctx.IP, ctx.Port);

                        await HandleTcpSession(ctx.ConnectionId, client, ctx.Cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        //主动取消，正常退出，不打错误日志
                        break;
                    }
                    catch (Exception ex)
                    {
                        //失败后清理残留socket
                        if (client != null)
                        {
                            client.Dispose();
                        }
                        ctx.TcpClient = null;
                        ctx.IsActive = false;
                        ConnectionStatusChanged?.Invoke(ctx.ConnectionId, false);

                        consecutiveFailCount++;

                        // 首次失败 || 达到日志间隔，输出错误；其余静默
                        if (consecutiveFailCount == 1 || consecutiveFailCount % logInterval == 0)
                        {
                            _logger.Error(ex, "[{Id}] TCP连接失败，已连续失败{FailCnt}次，{Delay}秒后重试 {IP}:{Port}",
                                ctx.ConnectionId, consecutiveFailCount, 5, ctx.IP, ctx.Port);
                        }
                        else
                        {
                            // 中间静默，可选择Trace/Debug级别，生产环境不显示
                            _logger.Debug("[{Id}] TCP持续连接失败，连续失败{FailCnt}次，静默不输出错误", ctx.ConnectionId, consecutiveFailCount);
                        }
                    }

                    try
                    {
                        await Task.Delay(5000, ctx.Cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                _logger.Debug("[{Id}] Tcp重连循环已退出", ctx.ConnectionId);
            }, ctx.Cts.Token);
        }


        private async Task HandleTcpSession(string connectionId, TcpClient client, CancellationToken token)
        {
            try
            {
                using (var stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    while (!token.IsCancellationRequested)
                    {
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (read == 0) break;
                        string msg = Encoding.UTF8.GetString(buffer, 0, read);
                        MessageReceived?.Invoke(connectionId, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{Id}] 读取异常", connectionId);
            }
            finally
            {
                _logger.Information("[{Id}] 连接断开", connectionId);
                if (client.Connected) client.Close();
                // 更新状态
                if (_connections.TryGetValue(connectionId, out var ctx))
                {
                    ctx.IsActive = false;
                }
                ConnectionStatusChanged?.Invoke(connectionId, false);
            }
        }

        private async Task HandleUdpReceive(ConnectionContext ctx)
        {
            try
            {
                while (!ctx.Cts.Token.IsCancellationRequested)
                {
                    var result = await ctx.UdpClient.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    string remote = result.RemoteEndPoint.ToString();

                    // Server 模式：记录最后一次通信的远程端点用于回复
                    if (ctx.Role == CommRole.Server)
                        ctx.RemoteEndPoint = result.RemoteEndPoint;

                    MessageReceived?.Invoke(ctx.ConnectionId, message);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, "[{Id}] UDP 接收异常", ctx.ConnectionId);
            }
            finally
            {
                _logger.Information("[{Id}] UDP 接收停止", ctx.ConnectionId);
                if (_connections.TryGetValue(ctx.ConnectionId, out var state))
                {
                    state.IsActive = false;
                }
                ConnectionStatusChanged?.Invoke(ctx.ConnectionId, false);
            }
        }
        public void Dispose()
        {
            foreach (var id in _connections.Keys.ToArray())
                Stop(id);
        }

        private class ConnectionContext
        {
            public string ConnectionId { get; set; }
            public CommProtocol Protocol { get; set; }
            public CommRole Role { get; set; }
            public string IP { get; set; }
            public int Port { get; set; }
            public TcpListener TcpListener { get; set; }
            public TcpClient TcpClient { get; set; }
            public TcpClient TcpServerClient { get; set; }
            public UdpClient UdpClient { get; set; }
            public IPEndPoint RemoteEndPoint { get; set; }   // UDP Server 记录的客户端地址
            public IPEndPoint TargetEndPoint { get; set; }   // UDP Client 的目标地址（在 StartUdp时赋值）
            public CancellationTokenSource Cts { get; set; }
            public bool IsActive { get; set; }
        }
    }

}
