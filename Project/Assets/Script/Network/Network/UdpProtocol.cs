using System.Net.Sockets;
using System;
using System.Net;
using System.Collections.Generic;

namespace BaseFramework.Network
{
    class UdpProtocol : IProtocol
    {
        private string udpIp;
        private int udpPort;
        private NodeInfo node = null;
        NodeInfo IProtocol.Node
        {
            get { return node; }
            set { node = value; }
        }
        private int waitRec;
        int IProtocol.WaitRecive
        {
            get { return waitRec; }
            set { waitRec = value; }
        }
        private UdpClient udpClient = null;
        private IPEndPoint recIEpoint;
        private Dictionary<string, ProtocolCfg> udpConfig;
        public UdpProtocol(string ip, int port)
        {
            udpIp = ip;
            udpPort = port;
        }
        public void SetCfg(Dictionary<string, ProtocolCfg> cfg)
        {
            udpConfig = cfg;
        }
        public Exception Connect()
        {
            udpClient = new UdpClient(udpIp, udpPort);
            recIEpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), udpPort);
            return null;
        }
        public Exception Send(byte[] msg)
        {
            try
            {
                udpClient.Send(msg, msg.Length);
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        public byte[] Read()
        {
            var msg = udpClient.Receive(ref recIEpoint);
            // 去除头
            var dataLen = msg.Length - Const.PackageHeaderLength;
            byte[] retMsg = new byte[dataLen];
            Array.Copy(msg, Const.PackageHeaderLength, retMsg, 0, dataLen);
            return retMsg;
        }

        // 外部传进来的错误
        public Exception OutException(Exception e){
            return onException(e);
        }
        // udp 无重连逻辑
        private Exception onException(Exception e)
        {
            return e;
        }

        public void Despose()
        {
            var msg = System.Text.Encoding.UTF8.GetBytes("\0\tClient_ShutDown");
            udpClient.Send(msg, msg.Length);
        }
        public void Close()
        {
            var msg = System.Text.Encoding.UTF8.GetBytes("\0\tClient_ShutDown");
            udpClient.Send(msg, msg.Length);
        }
    }
}

