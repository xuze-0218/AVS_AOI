// <copyright file="TCP.cs" company="ATW"> 
// Copyright (c) Microsoft Corporation. All rights reserved. 
// </copyright> 
// <description> 
//包含系统中所需的TCP通信方法.
// </description>
using System.Threading;
using System.Windows.Forms;
//using VisionDevelopLibrary.SocketCommunication;
using VisionUserControls.SocketCommManage;
//using ClientSocket = VisionDevelopLibrary.SocketCommunication.ClientSocket;

namespace AVS
{
    static class Tcp
    {
        public static ClientSocket clientSocketA;                                            //PLCA对应的Socket
        public static ClientSocket clientSocketB;                                            //PLCB对应的Socket
        public static ClientSocket clientSocketC;                                            //PLCC对应的Socket
        public static ClientSocket clientSocketD;                                            //PLCC对应的Socket

        public static ClientManageData.CLIENT_PARM clientParmA;                             //CCD->PLCA通信参数
        public static ClientManageData.CLIENT_PARM clientParmB;                             //CCD->PLCB通信参数
        public static ClientManageData.CLIENT_PARM clientParmC;                             //CCD->PLCB通信参数
        public static ClientManageData.CLIENT_PARM clientParmD;                             //CCD->PLCB通信参数

        public static string receiveFromPLCA, receiveFromPLCB;                              //接收的PLC数据
        public static string receiveFromPLCC, receiveFromPLCD;                              //接收的PLC数据
        public static string sendToPLCA, sendToPLCB;                                        //发送到PLC的数据
        public static string sendToPLCC, sendToPLCD;                                      //发送到PLC的数据

        //初始化参数
        public static void TcpInit()
        {
            //清空所有Socket连接对象,创建通信参数对象实例
            clientSocketA = null;
            clientSocketB = null;
            clientSocketC = null;
            clientSocketD = null;

            clientParmA = new ClientManageData.CLIENT_PARM();


            //clientParmB = new ClientManageData.CLIENT_PARM();
            //locateDataPLCA = new CommProtoData.PLC_LOCATE_CMD();
            //locateDataPLCB = new CommProtoData.PLC_LOCATE_CMD();
            //calibDataPLCA = new CommProtoData.PLC_CALIB_CMD();
            //calibDataPLCB = new CommProtoData.PLC_CALIB_CMD();
            //dataPLCA = new CommProtoData.PLC_INSPECT_CMD();
            //dataPLCB = new CommProtoData.PLC_INSPECT_CMD();
            //dataPLCC = new CommProtoData.PLC_INSPECT_CMD();
            //dataPLCD = new CommProtoData.PLC_INSPECT_CMD();

            Global.cMD.ClientDataInit();

            for (int i = 0; i < Global.cMD.clientSocketList.Count; i++)
            {
                if (Global.cMD.clientSocketList[i].ClientName == "PLCA")
                {
                    clientSocketA = Global.cMD.clientSocketList[i];
                    clientParmA = Global.cMD.clientParamList[i];
                }
                if (Global.cMD.clientSocketList[i].ClientName == "PLCB")
                {
                    clientSocketB = Global.cMD.clientSocketList[i];
                    clientParmB = Global.cMD.clientParamList[i];
                }
                if (Global.cMD.clientSocketList[i].ClientName == "PLCC")
                {
                    clientSocketC = Global.cMD.clientSocketList[i];
                    clientParmC = Global.cMD.clientParamList[i];
                }
                if (Global.cMD.clientSocketList[i].ClientName == "PLCD")
                {
                    clientSocketD = Global.cMD.clientSocketList[i];
                    clientParmD = Global.cMD.clientParamList[i];
                }
            }
        }

        //连接Tcp
        public static void ConnectToPLC(string name)
        {
            ClientSocket sc;
            switch (name)
            {
                case "A":
                    sc = clientSocketA;
                    if (sc == null)
                    {
                        MessageBox.Show("服务器不存在:PLC" + name);
                        return;
                    }
                    sc.OnData = OnClientDataA;
                    break;
                case "B":
                    sc = clientSocketB;
                    if (sc == null)
                    {
                        MessageBox.Show("服务器不存在:PLC" + name);
                        return;
                    }
                    sc.OnData = OnClientDataB;
                    break;
                case "C":
                    sc = clientSocketC;
                    if (sc == null)
                    {
                        MessageBox.Show("客户端不存在:PLC" + name);
                        return;
                    }
                    sc.OnData = OnClientDataC;
                    break;
                case "D":
                    sc = clientSocketD;
                    if (sc == null)
                    {
                        MessageBox.Show("客户端不存在:PLC" + name);
                        return;
                    }
                    sc.OnData = OnClientDataD;
                    break;
                default:
                    return;
            }

            sc.Connected = true;

            Thread.Sleep(100);

            if (sc.Connected)
                Global.AddLog("连接成功，服务器：" + sc.ServerIP + "," + sc.ServerPort);
            else
                Global.AddLog("连接失败，服务器：" + sc.ServerIP + "," + sc.ServerPort);
        }

        //断开TCP连接
        public static void DisConnectToPLC(string name)
        {
            ClientSocket sc;
            switch (name)
            {
                case "A":
                    sc = clientSocketA;
                    break;

                case "B":
                    sc = clientSocketB;
                    break;
                case "C":
                    sc = clientSocketC;
                    break;
                case "D":
                    sc = clientSocketD;
                    break;
                default:
                    return;
            }

            if (sc == null)
            {
                Global.AddLog("断开失败，服务器不存在:PLC" + name);
                return;
            }
            else
            {
                sc.Connected = false;
            }                

            if (!sc.Connected)
            {
                Global.AddLog("断开成功，服务器：" + sc.ServerIP + "," + sc.ServerPort);
            }
            else
            {
                Global.AddLog("断开失败，服务器：" + sc.ServerIP + "," + sc.ServerPort);
            }                
        }

        //接收数据
        public static void OnClientDataA(string recvText)
        {
            receiveFromPLCA = recvText;
        }
        public static void OnClientDataB(string recvText)
        {
            receiveFromPLCB = recvText;
        }
        public static void OnClientDataC(string recvText)
        {
            receiveFromPLCC = recvText;
        }
        public static void OnClientDataD(string recvText)
        {
            receiveFromPLCD = recvText;
        }
        //发送数据
        public static bool SendDataA(string sendText)
        {
            if (clientSocketA == null)
            {
                Global.AddLog("发送失败，服务器不存在：" + "PLCA");
                return false;
            }
            if (clientSocketA.SendData(sendText))
            {
                return true;
            }
            else
            {
                Global.AddLog("发送失败，服务器：" + clientSocketA.ServerIP + "," + clientSocketA.ServerPort);
                return false;
            }
        }
        public static bool SendDataB(string sendText)
        {
            if (clientSocketB == null)
            {
                Global.AddLog("发送失败，服务器不存在：" + "PLCB");
                return false;
            }
            if (clientSocketB.SendData(sendText))
            {
                return true;
            }
            else
            {
                Global.AddLog("发送失败，服务器：" + clientSocketB.ServerIP + "," + clientSocketB.ServerPort);
                return false;
            }
        }
        public static bool SendDataC(string sendText)
        {
            if (clientSocketC == null)
            {
                Global.AddLog("发送失败，服务器不存在：" + "PLCC");
                return false;
            }
            if (clientSocketC.SendData(sendText))
            {
                return true;
            }
            else
            {
                Global.AddLog("发送失败，服务器：" + clientSocketC.ServerIP + "," + clientSocketC.ServerPort);
                return false;
            }
        }
        public static bool SendDataD(string sendText)
        {
            if (clientSocketD == null)
            {
                Global.AddLog("发送失败，服务器不存在：" + "PLCD");
                return false;
            }
            if (clientSocketD.SendData(sendText))
            {
                return true;
            }
            else
            {
                Global.AddLog("发送失败，服务器：" + clientSocketD.ServerIP + "," + clientSocketD.ServerPort);
                return false;
            }
        }
        public static bool SendData(string side, string sendText)
        {
            if (side == "A")
            {
                if (SendDataA(sendText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (side == "B")
            {
                if (SendDataB(sendText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (side == "C")
            {
                if (SendDataC(sendText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (side == "D")
            {
                if (SendDataD(sendText))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
