using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BaseFramework.Network
{
    class TcpProtocol : IProtocol
    {
        private bool protocolRun = false;
        private string tcpIp;
        private int tcpPort;
        private ReConnect connector;
        private NodeInfo node = null;
        private int connStatus = 0;
        private TcpClient tcpClient = null;
        private NetworkStream tcpStream;
        private Dictionary<string, ProtocolCfg> tcpConfig;
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
        public TcpProtocol(string ip, int port, ReConnect reconnect)
        {
            tcpIp = ip;
            tcpPort = port;
            connector = reconnect;
        }
        public void SetCfg(Dictionary<string, ProtocolCfg> cfg)
        {
            tcpConfig = cfg;
        }
        public Exception Connect()
        {
            tcpClient = new TcpClient(tcpIp, tcpPort);
            tcpStream = tcpClient.GetStream();
            //  设置相关配置
            if (tcpConfig.ContainsKey("SendBufferSize"))
            {
                tcpClient.SendBufferSize = tcpConfig["SendBufferSize"].intElement;
            }
            if (tcpConfig.ContainsKey("ReceiveBufferSize"))
            {
                tcpClient.ReceiveBufferSize = tcpConfig["ReceiveBufferSize"].intElement;
            }
            if (tcpConfig.ContainsKey("ReceiveTimeout"))
            {
                tcpClient.ReceiveTimeout = tcpConfig["ReceiveTimeout"].intElement;
            }
            protocolRun = true;
            connStatus = 1;
            return null;
        }
        public Exception Send(byte[] msg)
        {
            try
            {
                tcpStream.Write(msg, 0, msg.Length);
            }
            catch (Exception e)
            {
                e = onException(e);
                if (e != null) return e;
                // 再次尝试发送数据，只会尝试一次，如果还报错，直接上抛错误
                tcpStream.Write(msg, 0, msg.Length);
            }
            return null;
        }
        public byte[] Read()
        {
            var recLen = 0;
            byte[] msg = new byte[Const.PackageHeaderLength];
            try
            {
                while (recLen < Const.PackageHeaderLength)
                {
                    recLen += tcpStream.Read(msg, recLen, Const.PackageHeaderLength - recLen);
                }
                var dataLen = NetUtil.bigEndian2ushort(msg, 0, Const.PackageHeaderLength);
                msg = new byte[dataLen];
                recLen = 0;
                while (recLen < dataLen)
                {
                    recLen += tcpStream.Read(msg, recLen, dataLen - recLen);
                }
            }
            catch (Exception e)
            {
                // 尝试恢复
                var ne = onException(e);
                // 返回原来的错误，方便排查问题
                if (ne != null) throw e;
                else msg = Read();
            }
            return msg;
        }

        private bool chkRecovery(Exception e)
        {
            if (e is System.IO.IOException) return true;
            if (e is System.ObjectDisposedException) return true;
            if (e == NetException.HeartBeatTimeOut) return true;
            return false;
        }

        public Exception OutException(Exception e)
        {
            return onException(e);
        }

        // 异常拦截器
        private Exception onException(Exception e)
        {
            // 检查状态是否正常
            if (!protocolRun)
            {
                return NetException.ProtoctoStop;
            }
            // 设置错误状态
            connStatus = 2;
            lock (tcpClient)
            {
                if (connStatus == 1) return null;
                int reConnectNum = 0;
                // 断线异常，或者心跳超时，直接重连
                if (waitRec <= 0 && chkRecovery(e))
                {
                    // 暂时定尝试20次， TODO(CQ)
                    while (reConnectNum < 20)
                    {
                        Thread.Sleep(500);
                        e = reConnect();
                        if (e == null) { connStatus = 1; break; };
                        reConnectNum++;
                    }
                }
            }
            return e;
        }

        private void closeTCP()
        {
            tcpClient?.Close();
            tcpStream?.Close();
            connStatus = 0;
        }
        public void Close()
        {
            protocolRun = false;
            this.closeTCP();
        }
        public void Despose()
        {
            protocolRun = false;
            tcpClient?.Dispose();
            tcpStream?.Dispose();
            tcpClient = null;
            tcpStream = null;
        }
        // 重连
        private Exception reConnect()
        {
            if (!protocolRun)
            {
                return NetException.ProtoctoStop;
            }
            this.closeTCP();
            if (this.connector == null) {
                return new Exception("connector not exists");
            }
            return connector(node, this);
        }
    }
}