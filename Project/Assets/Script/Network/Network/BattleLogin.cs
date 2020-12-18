
using System;
using System.Collections.Generic;

namespace BaseFramework.Network
{
    internal static class BattleLogin
    {
        internal static Exception Login(string ip, int port, string token, out IProtocol p)
        {
            p = new TcpProtocol(ip, port, new ReConnect(BattleLogin.Connect));
            var cfg = new Dictionary<string, ProtocolCfg>(){
                {"SendBufferSize", new ProtocolCfg{intElement=Const.PackageMaxLength}},
                {"ReceiveBufferSize", new ProtocolCfg{intElement=Const.PackageMaxLength}},
                {"ReceiveTimeout", new ProtocolCfg{intElement=0}}};
            p.SetCfg(cfg);
            p.Node = new NodeInfo("", "", "", ip, port, token);
            return Connect(p.Node, p);
        }
        internal static Exception Connect(NodeInfo node, IProtocol p)
        {
            p.Connect();
            // 连接后第一步，使用约定的加密密钥传输rid和uid来寻找对应的房间,之后采用加密通信传输数据
            // 暂时采用明文传输房间号 todo...
            byte[] data = System.Text.Encoding.Default.GetBytes(node.token);
            p.Send(DataPack.PackLength(data));
            // 等待服务端确认
            data = p.Read();
            string uid = System.Text.Encoding.Default.GetString(data);
            DebugLogger.Debug(string.Format("login to server {0}", uid));
            // 战斗不需要额外连接node
            return null;
        }
    }
}