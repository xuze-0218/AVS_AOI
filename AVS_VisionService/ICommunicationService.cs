using AVS_Service.Models;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AVS_Service
{
    public interface ICommunicationService
    {
        void Start(
            string connectionId,
            CommProtocol protocol,
            CommRole role,
            string ip,
            int port);

        void Stop(string connectionId);

        Task SendAsync(
            string connectionId,
            string message);

        bool IsActive(string connectionId);

        event Action<string, string> MessageReceived;

        event Action<string> LogMessage;

        event Action<string, bool> ConnectionStatusChanged;
    }

    public class CommunicationService : ICommunicationService, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConnectionContext> _connections;
        private readonly ILogger _logger;

        public event Action<string, string> MessageReceived;

        public event Action<string> LogMessage;

        public event Action<string, bool> ConnectionStatusChanged;

        public CommunicationService(ILogger logger)
        {
            _logger = logger;

            _connections =
                new ConcurrentDictionary<string, ConnectionContext>();
        }

        #region Start / Stop

        public void Start(
            string connectionId,
            CommProtocol protocol,
            CommRole role,
            string ip,
            int port)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException(
                    "connectionId不能为空",
                    nameof(connectionId));

            // 如果已经存在，先停止
            ConnectionContext existing;

            if (_connections.TryGetValue(
                connectionId,
                out existing))
            {
                _logger.Warning(
                    "[{Id}] 连接已存在，先停止",
                    connectionId);

                Stop(connectionId);
            }

            var ctx = new ConnectionContext
            {
                ConnectionId = connectionId,
                Protocol = protocol,
                Role = role,
                IP = ip,
                Port = port,
                Cts = new CancellationTokenSource(),
                IsActive = false
            };

            // 先加入字典，再启动后台任务
            _connections[connectionId] = ctx;

            try
            {
                if (protocol == CommProtocol.TCP)
                {
                    if (role == CommRole.Server)
                    {
                        StartTcpServer(ctx);
                    }
                    else
                    {
                        StartTcpClient(ctx);
                    }
                }
                else if (protocol == CommProtocol.UDP)
                {
                    StartUdp(ctx);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "[{Id}] 启动通信失败",
                    connectionId);

                Stop(connectionId);
            }
        }

        public void Stop(string connectionId)
        {
            ConnectionContext ctx;

            if (!_connections.TryRemove(
                connectionId,
                out ctx))
            {
                return;
            }

            try
            {
                // 先取消
                if (ctx.Cts != null)
                {
                    ctx.Cts.Cancel();
                }

                // TCP Server
                if (ctx.TcpListener != null)
                {
                    try
                    {
                        ctx.TcpListener.Stop();
                    }
                    catch
                    {
                    }
                }

                // TCP Client
                if (ctx.TcpClient != null)
                {
                    try
                    {
                        ctx.TcpClient.Close();
                    }
                    catch
                    {
                    }
                }

                // TCP Server 当前客户端
                if (ctx.TcpServerClient != null)
                {
                    try
                    {
                        ctx.TcpServerClient.Close();
                    }
                    catch
                    {
                    }
                }

                // UDP
                if (ctx.UdpClient != null)
                {
                    try
                    {
                        ctx.UdpClient.Close();
                    }
                    catch
                    {
                    }
                }

                ctx.IsActive = false;

                _logger.Information(
                    "[{Id}] 连接已停止",
                    connectionId);

                RaiseConnectionStatusChanged(
                    connectionId,
                    false);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "[{Id}] 停止连接异常",
                    connectionId);
            }
            finally
            {
                if (ctx.Cts != null)
                {
                    ctx.Cts.Cancel();
                    //ctx.Cts.Dispose();
                }
            }
        }

        public bool IsActive(string connectionId)
        {
            ConnectionContext ctx;

            return _connections.TryGetValue(
                       connectionId,
                       out ctx)
                   && ctx.IsActive;
        }

        #endregion

        #region UDP
        private void StartUdp(ConnectionContext ctx)
        {
            if (ctx.Role == CommRole.Server)
            {
                ctx.UdpClient = new UdpClient(ctx.Port);
                _logger.Information("[{Id}] UDP Server 监听端口 {Port}", ctx.ConnectionId, ctx.Port);
            }
            else
            {
                ctx.UdpClient = new UdpClient();
                ctx.TargetEndPoint = new IPEndPoint(IPAddress.Parse(ctx.IP), ctx.Port);
                _logger.Information("[{Id}] UDP Client 目标 {IP}:{Port}", ctx.ConnectionId, ctx.IP, ctx.Port);
            }

            ctx.IsActive = true;
            RaiseConnectionStatusChanged(ctx.ConnectionId, true);

            Task.Run(() => HandleUdpReceive(ctx), ctx.Cts.Token);
        }
        private async Task HandleUdpReceive(
            ConnectionContext ctx)
        {
            try
            {
                while (!ctx.Cts.IsCancellationRequested)
                {
                    UdpReceiveResult result =
                        await ctx.UdpClient.ReceiveAsync();

                    if (ctx.Cts.IsCancellationRequested)
                    {
                        break;
                    }

                    string message =
                        Encoding.UTF8.GetString(
                            result.Buffer);

                    string remote =
                        result.RemoteEndPoint != null
                            ? result.RemoteEndPoint.ToString()
                            : string.Empty;

                    // UDP Server 保存最后一次客户端地址
                    if (ctx.Role == CommRole.Server)
                    {
                        ctx.RemoteEndPoint =
                            result.RemoteEndPoint;
                    }

                    _logger.Debug("[{Id}] UDP 收到消息 {Remote}: {Msg}", ctx.ConnectionId, remote, message);
                    RaiseMessageReceived(ctx.ConnectionId, message);
                }
            }
            catch (ObjectDisposedException)
            {
                // Stop() 导致 UdpClient 被关闭
            }
            catch (SocketException)
            {
                if (!ctx.Cts.IsCancellationRequested)
                {
                    _logger.Error(
                        "[{Id}] UDP Socket异常",
                        ctx.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                if (!ctx.Cts.IsCancellationRequested)
                {
                    _logger.Error(
                        ex,
                        "[{Id}] UDP 接收异常",
                        ctx.ConnectionId);
                }
            }
            finally
            {
                ctx.IsActive = false;

                _logger.Information(
                    "[{Id}] UDP 接收停止",
                    ctx.ConnectionId);

                RaiseConnectionStatusChanged(
                    ctx.ConnectionId,
                    false);
            }
        }

        #endregion

        #region TCP Server

        private void StartTcpServer(ConnectionContext ctx)
        {
            ctx.TcpListener = new TcpListener(IPAddress.Any, ctx.Port);
            ctx.TcpListener.Start();

            _logger.Information("[{Id}] TCP Server 监听端口 {Port}", ctx.ConnectionId, ctx.Port);

            Task.Run(() => TcpServerAcceptLoop(ctx), ctx.Cts.Token);
        }

        private async Task TcpServerAcceptLoop(
            ConnectionContext ctx)
        {
            while (!ctx.Cts.IsCancellationRequested)
            {
                TcpClient client = null;

                try
                {
                    client = await ctx.TcpListener.AcceptTcpClientAsync();
                    if (ctx.Cts.IsCancellationRequested)
                    {
                        client.Close();
                        break;
                    }

                    string remote = client.Client.RemoteEndPoint != null ? client.Client.RemoteEndPoint.ToString()
                            : string.Empty;

                    _logger.Information(
                        "[{Id}] 客户端连接: {Remote}",
                        ctx.ConnectionId,
                        remote);

                    // 如果之前已经有客户端
                    if (ctx.TcpServerClient != null)
                    {
                        try
                        {
                            ctx.TcpServerClient.Close();
                        }
                        catch
                        {
                        }
                    }

                    ctx.TcpServerClient = client;

                    ctx.IsActive = true;

                    RaiseConnectionStatusChanged(
                        ctx.ConnectionId,
                        true);

                    /*
                     * 不等待 Session
                     *
                     * 继续 Accept 新客户端
                     */
                    Task.Run(
                        () => HandleTcpSession(
                            ctx.ConnectionId,
                            client,
                            ctx.Cts.Token));
                }
                catch (ObjectDisposedException)
                {
                    // TcpListener 被 Stop()
                    break;
                }
                catch (SocketException)
                {
                    if (ctx.Cts.IsCancellationRequested)
                    {
                        break;
                    }

                    _logger.Error(
                        "[{Id}] TCP Server Socket异常",
                        ctx.ConnectionId);

                    break;
                }
                catch (Exception ex)
                {
                    if (ctx.Cts.IsCancellationRequested)
                    {
                        break;
                    }

                    _logger.Error(
                        ex,
                        "[{Id}] 接受客户端异常",
                        ctx.ConnectionId);

                    break;
                }
            }

            _logger.Debug(
                "[{Id}] TCP Server 监听循环退出",
                ctx.ConnectionId);
        }

        #endregion

        #region TCP Client

        private void StartTcpClient(
            ConnectionContext ctx)
        {
            Task.Run(
                () => TcpClientConnectLoop(ctx),
                ctx.Cts.Token);
        }

        private async Task TcpClientConnectLoop(
            ConnectionContext ctx)
        {
            int consecutiveFailCount = 0;

            const int logInterval = 3;

            while (!ctx.Cts.IsCancellationRequested)
            {
                TcpClient client = null;

                try
                {
                    client = new TcpClient();

                    _logger.Debug(
                        "[{Id}] 尝试连接 {IP}:{Port}",
                        ctx.ConnectionId,
                        ctx.IP,
                        ctx.Port);

                    /*
                     * .NET Framework 4.8：
                     *
                     * ConnectAsync 没有
                     * CancellationToken 重载
                     */
                    await client.ConnectAsync(
                        ctx.IP,
                        ctx.Port);

                    if (ctx.Cts.IsCancellationRequested)
                    {
                        client.Close();
                        break;
                    }

                    ctx.TcpClient = client;

                    ctx.IsActive = true;

                    consecutiveFailCount = 0;

                    RaiseConnectionStatusChanged(
                        ctx.ConnectionId,
                        true);

                    _logger.Information(
                        "[{Id}] TCP Client 已连接 {IP}:{Port}",
                        ctx.ConnectionId,
                        ctx.IP,
                        ctx.Port);

                    /*
                     * 等待当前 TCP 会话结束
                     *
                     * 对端断开后：
                     * HandleTcpSession 返回
                     *
                     * 然后进入重连流程
                     */
                    await HandleTcpSession(
                        ctx.ConnectionId,
                        client,
                        ctx.Cts.Token);
                }
                catch (ObjectDisposedException)
                {
                    if (!ctx.Cts.IsCancellationRequested)
                    {
                        ctx.IsActive = false;
                    }
                }
                catch (SocketException ex)
                {
                    if (ctx.Cts.IsCancellationRequested)
                    {
                        break;
                    }

                    HandleTcpClientConnectError(
                        ctx,
                        ex,
                        ref consecutiveFailCount,
                        logInterval);
                }
                catch (Exception ex)
                {
                    if (ctx.Cts.IsCancellationRequested)
                    {
                        break;
                    }

                    HandleTcpClientConnectError(
                        ctx,
                        ex,
                        ref consecutiveFailCount,
                        logInterval);
                }
                finally
                {
                    if (client != null)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch
                        {
                        }
                    }

                    if (ReferenceEquals(
                        ctx.TcpClient,
                        client))
                    {
                        ctx.TcpClient = null;
                    }

                    ctx.IsActive = false;
                }

                // 等待 5 秒后重连
                try
                {
                    await Task.Delay(
                        5000,
                        ctx.Cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.Debug(
                "[{Id}] TCP重连循环已退出",
                ctx.ConnectionId);
        }

        private void HandleTcpClientConnectError(
            ConnectionContext ctx,
            Exception ex,
            ref int consecutiveFailCount,
            int logInterval)
        {
            ctx.IsActive = false;

            RaiseConnectionStatusChanged(
                ctx.ConnectionId,
                false);

            consecutiveFailCount++;

            if (consecutiveFailCount == 1 ||
                consecutiveFailCount % logInterval == 0)
            {
                _logger.Error(
                    ex,
                    "[{Id}] TCP连接失败，已连续失败{FailCnt}次，5秒后重试 {IP}:{Port}",
                    ctx.ConnectionId,
                    consecutiveFailCount,
                    ctx.IP,
                    ctx.Port);
            }
            else
            {
                _logger.Debug(
                    "[{Id}] TCP持续连接失败，连续失败{FailCnt}次",
                    ctx.ConnectionId,
                    consecutiveFailCount);
            }
        }

        #endregion

        #region TCP Session

        private async Task HandleTcpSession(
            string connectionId,
            TcpClient client,
            CancellationToken token)
        {
            try
            {
                using (NetworkStream stream =
                       client.GetStream())
                {
                    byte[] buffer =
                        new byte[4096];

                    while (!token.IsCancellationRequested)
                    {
                        int read;

                        try
                        {
                            /*
                             * .NET Framework 4.8 支持
                             * NetworkStream.ReadAsync(..., CancellationToken)
                             */
                            read =
                                await stream.ReadAsync(
                                    buffer,
                                    0,
                                    buffer.Length,
                                    token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }

                        // 对方正常关闭连接
                        if (read == 0)
                        {
                            break;
                        }

                        string message =
                            Encoding.UTF8.GetString(
                                buffer,
                                0,
                                read);

                        RaiseMessageReceived(
                            connectionId,
                            message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 主动停止
            }
            catch (ObjectDisposedException)
            {
                // Socket 被关闭
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _logger.Error(
                        ex,
                        "[{Id}] TCP读取异常",
                        connectionId);
                }
            }
            finally
            {
                try
                {
                    client.Close();
                }
                catch
                {
                }

                _logger.Information(
                    "[{Id}] TCP连接断开",
                    connectionId);

                ConnectionContext ctx;

                if (_connections.TryGetValue(
                    connectionId,
                    out ctx))
                {
                    /*
                     * 只有当前 client 才允许修改状态
                     *
                     * 防止旧连接结束时把新连接状态改成 false
                     */
                    if (ctx.Role == CommRole.Client)
                    {
                        if (ReferenceEquals(
                            ctx.TcpClient,
                            client))
                        {
                            ctx.IsActive = false;
                            ctx.TcpClient = null;

                            RaiseConnectionStatusChanged(
                                connectionId,
                                false);
                        }
                    }
                    else
                    {
                        if (ReferenceEquals(
                            ctx.TcpServerClient,
                            client))
                        {
                            ctx.IsActive = false;
                            ctx.TcpServerClient = null;

                            RaiseConnectionStatusChanged(
                                connectionId,
                                false);
                        }
                    }
                }
            }
        }

        #endregion

        #region Send

        public async Task SendAsync(
            string connectionId,
            string message)
        {
            ConnectionContext ctx;

            if (!_connections.TryGetValue(
                connectionId,
                out ctx))
            {
                _logger.Warning(
                    "[{Id}] 连接不存在，无法发送",
                    connectionId);

                return;
            }

            if (!ctx.IsActive)
            {
                _logger.Warning(
                    "[{Id}] 连接不可用，无法发送",
                    connectionId);

                return;
            }

            if (message == null)
            {
                message = string.Empty;
            }

            byte[] data =
                Encoding.UTF8.GetBytes(message);

            try
            {
                if (ctx.Protocol == CommProtocol.TCP)
                {
                    TcpClient client = null;

                    if (ctx.Role == CommRole.Server)
                    {
                        client = ctx.TcpServerClient;
                    }
                    else
                    {
                        client = ctx.TcpClient;
                    }

                    if (client == null)
                    {
                        _logger.Warning(
                            "[{Id}] TCP Client不存在",
                            connectionId);

                        return;
                    }

                    NetworkStream stream =
                        client.GetStream();

                    if (stream == null ||
                        !stream.CanWrite)
                    {
                        _logger.Warning(
                            "[{Id}] TCP Stream不可写",
                            connectionId);

                        return;
                    }

                    await stream.WriteAsync(
                        data,
                        0,
                        data.Length,
                        ctx.Cts.Token);

                    await stream.FlushAsync();

                    _logger.Information(
                        "[{Id}] TCP发送消息: {Msg}",
                        connectionId,
                        message);
                }
                else if (ctx.Protocol == CommProtocol.UDP)
                {
                    if (ctx.UdpClient == null)
                    {
                        _logger.Warning(
                            "[{Id}] UDP Client不存在",
                            connectionId);

                        return;
                    }

                    if (ctx.Role == CommRole.Server)
                    {
                        if (ctx.RemoteEndPoint == null)
                        {
                            _logger.Warning(
                                "[{Id}] UDP Server没有可用发送目标",
                                connectionId);

                            return;
                        }

                        await ctx.UdpClient.SendAsync(
                            data,
                            data.Length,
                            ctx.RemoteEndPoint);
                    }
                    else
                    {
                        if (ctx.TargetEndPoint == null)
                        {
                            _logger.Warning(
                                "[{Id}] UDP Client没有目标地址",
                                connectionId);

                            return;
                        }

                        await ctx.UdpClient.SendAsync(
                            data,
                            data.Length,
                            ctx.TargetEndPoint);
                    }

                    _logger.Information(
                        "[{Id}] UDP发送消息: {Msg}",
                        connectionId,
                        message);
                }
            }
            catch (OperationCanceledException)
            {
                // 主动停止
            }
            catch (ObjectDisposedException)
            {
                _logger.Debug(
                    "[{Id}] 连接已关闭，发送取消",
                    connectionId);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "[{Id}] 发送失败",
                    connectionId);
            }
        }

        #endregion

        #region Events

        private void RaiseMessageReceived(
            string connectionId,
            string message)
        {
            var handler = MessageReceived;

            if (handler != null)
            {
                try
                {
                    handler(
                        connectionId,
                        message);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "[{Id}] MessageReceived事件处理异常",
                        connectionId);
                }
            }
        }

        private void RaiseConnectionStatusChanged(
            string connectionId,
            bool isConnected)
        {
            var handler =
                ConnectionStatusChanged;

            if (handler != null)
            {
                try
                {
                    handler(
                        connectionId,
                        isConnected);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        ex,
                        "[{Id}] ConnectionStatusChanged事件处理异常",
                        connectionId);
                }
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            foreach (string id in
                     _connections.Keys.ToArray())
            {
                Stop(id);
            }

            _connections.Clear();
        }

        #endregion

        #region Context

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

            // UDP Server最后一次通信的客户端
            public IPEndPoint RemoteEndPoint { get; set; }

            // UDP Client目标
            public IPEndPoint TargetEndPoint { get; set; }

            public CancellationTokenSource Cts { get; set; }

            public bool IsActive { get; set; }
        }

        #endregion
    }
}