
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace BaseFramework.Network
{
    internal static class UsrLogin
    {
        internal static Exception Login(string ip, int port, string account, out IProtocol p)
        {
            p = new TcpProtocol(ip, port, null);
            var cfg = new Dictionary<string, ProtocolCfg>(){
                {"SendBufferSize", new ProtocolCfg{intElement=Const.PackageMaxLength}},
                {"ReceiveBufferSize", new ProtocolCfg{intElement=Const.PackageMaxLength}},
                {"ReceiveTimeout", new ProtocolCfg{intElement=3000}}};
            p.SetCfg(cfg);
            NodeInfo node = null;
            try
            {
                // 第一步建立连接
                p.Connect();
                //第二步，收到服务端发回的base64(8bytes random challenge)随机串，用于后序的握手验证//
                byte[] data = p.Read();

                //第三步 生成 8bytes 随机串 client key
                byte[] client_key = Convert.FromBase64String(Encoding.Default.GetString(data, 0, data.Length));
                data = LoginPass.GenSecretKey(data, client_key);

                //第四步，向服务端发送base64(DH-Exchange(client key)) 用于生成 secret 的 key
                p.Send(DataPack.PackLength(data));

                //第五步，收到服务端发送过来的base64(DH-Exchange(server key)) 用于生成 secret 的 key
                data = p.Read();

                //第六步，secret := DH-Secret(client key/server key)服务器和客户端都可以计算出同一个 8 字节的 secret 用来加密数据
                byte[] secret = LoginPass.GenVerifySecret(data, client_key);
                NetCrypter.setSecret(secret);

                //第七步，base64(HMAC(challenge, secret))回应服务器第一步握手的挑战码，确认握手正常
                data = LoginPass.GenFirstChallenge(client_key, secret);
                p.Send(DataPack.PackLength(data));

                //第八步，DES(secret, base64(token))使用 DES 算法，以 secret 做 key 加密传输 token，token 包含帐号信息[user，sdkid]
                data = LoginPass.GenSecretUserToken(account, secret);
                p.Send(DataPack.PackLength(data));

                //第九步，认证结果信息，前2个字节retcode 200 表示成功，只有成功才解析后续内容(base64(uid:subid)@base64(server)#base64(info))
                data = p.Read();
                node = LoginPass.ProcessLoginData(data);
                // 登陆完毕，关闭当前连接，准备连接node
                p.Close();

            }
            catch (Exception e)
            {
                return e;
            }
            DebugLogger.Debug("login node" + node.nodeHost + node.nodePort);
            // 连接节点
            p = new TcpProtocol(node.nodeHost, node.nodePort, new ReConnect(UsrLogin.ConnectNode));
            cfg["ReceiveTimeout"] = new ProtocolCfg { intElement = 10000 };
            p.SetCfg(cfg);
            p.Node = node;
            return ConnectNode(p.Node, p);
        }
        private static Exception ConnectNode(NodeInfo node, IProtocol p)
        {
            try
            {
                // 建立连接，注册重连函数
                p.Connect();
                byte[] data = LoginPass.GenHandShake(node.uid.ToString(), node.gameServer, node.subid, ++node.index);
                p.Send(DataPack.PackLength(data));
                data = p.Read();
                if (data.Length < 2)
                {
                    DebugLogger.DebugNetworkError("Process Hand Shake Message Error: Data Length is Less Than 2");
                    throw new NetworkingException((int)ErrorCode.DataError);
                }
                UInt32 code = ((UInt32)data[0] << 8) + (UInt32)data[1];
                if (code != (int)ErrorCode.Success)
                {
                    DebugLogger.DebugNetworkError("Process Hand Shake Message Error: Connect Node Code is " + code);
                    throw new NetworkingException((int)code);
                }
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
    }

    class LoginPass
    {
        public static byte[] GenSecretKey(byte[] data, byte[] key)
        {
            byte[] clientKeyDH = Crypt.dh_exchange(key);
            byte[] clientKeyDHBase64 = Encoding.Default.GetBytes(Convert.ToBase64String(clientKeyDH));
            return clientKeyDHBase64;
        }

        public static byte[] GenVerifySecret(byte[] data, byte[] client_key)
        {
            byte[] serverKey = Convert.FromBase64String(Encoding.Default.GetString(data, 0, data.Length));
            return Crypt.dh_secret(serverKey, client_key);
        }

        public static byte[] GenFirstChallenge(byte[] client_key, byte[] secret)
        {
            byte[] hmac = Crypt.hmac64(client_key, secret);
            byte[] hmacBase64 = Encoding.Default.GetBytes(Convert.ToBase64String(hmac));
            return hmacBase64;
        }

        public static byte[] GenSecretUserToken(string account, byte[] secret)
        {
            string sdkId = "OFFICIAL";
            string device = Utils.GetDeviceID();
            List<string> loginParameters = new List<string>();
            loginParameters.Add(account);
            loginParameters.Add(device);
            loginParameters.Add("mscode");
            string parameters = string.Empty;
            for (int i = 0; i < loginParameters.Count; i++)
            {
                parameters += Convert.ToBase64String(Encoding.UTF8.GetBytes(loginParameters[i])) + ":";
            }

            string sign = "sign";

            string token_base64 = "0" + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(sdkId)) + ":" + parameters + "#" + Convert.ToBase64String(Encoding.UTF8.GetBytes(sign)) + "#" + Convert.ToBase64String(Encoding.UTF8.GetBytes(device));
            DebugLogger.Debug("GenSecretUserToken ----    " + token_base64);
            byte[] token_base64_bits = Encoding.Default.GetBytes(token_base64);
            byte[] token_base64_des_bits = Crypt.des_encode(secret, token_base64_bits);
            byte[] token_base64_des_bits2 = Crypt.des_decode(secret, token_base64_des_bits);
            byte[] token_base64_des_base64bits = Encoding.Default.GetBytes(Convert.ToBase64String(token_base64_des_bits));

            return token_base64_des_base64bits;
        }

        public static NodeInfo ProcessLoginData(byte[] data)
        {
            if (data.Length < 2)
            {
                DebugLogger.Debug("Data Length is Less Than 2 ! Got " + data.Length, NetUtil.ErrorColor);
                throw new NetworkingException((int)ErrorCode.DataError);
            }

            UInt32 code = ((UInt32)data[0] << 8) + (UInt32)data[1];

            if (code != (int)ErrorCode.Success)
            {
                if (code == (int)ErrorCode.Forbidden || code == (int)ErrorCode.LoginBusy)
                {
                    byte[] b_message = new byte[data.Length - 2];
                    Buffer.BlockCopy(data, 2, b_message, 0, data.Length - 2);
                    var base64Str = Encoding.Default.GetString(b_message);
                    string message = Encoding.Default.GetString(Convert.FromBase64String(base64Str));//Convert.ToBase64String(b_message);
                    DebugLogger.DebugError("Forbidden LoginBusy");
                    throw new NetworkingException((int)code, message);
                }
                else
                {
                    DebugLogger.Debug("Login Code is Not Equal 200 ! Got " + code, NetUtil.ErrorColor);
                    throw new NetworkingException((int)code);
                }
            }

            byte[] auth_return_bytes = new byte[data.Length - 2];
            Buffer.BlockCopy(data, 2, auth_return_bytes, 0, data.Length - 2);
            string auth_return = Encoding.Default.GetString(auth_return_bytes);

            string[] strs = Regex.Split(auth_return, "@([^#]*)#");
            if (strs.Length != 3)
            {
                DebugLogger.Debug("Network Strs Length Error, Required 3 Got " + strs.Length, NetUtil.ErrorColor);
                throw new NetworkingException((int)ErrorCode.DataError);
            }

            string uid = Encoding.Default.GetString(Convert.FromBase64String(strs[0]));
            string server = Encoding.Default.GetString(Convert.FromBase64String(strs[1]));

            string[] infos = Regex.Split(strs[2], "#");
            if (infos.Length != 2)
            {
                DebugLogger.Debug("Network Infos Length Error, Required 2 Got " + infos.Length, NetUtil.ErrorColor);
                throw new NetworkingException((int)ErrorCode.DataError);
            }

            string netinfo = Encoding.Default.GetString(Convert.FromBase64String(infos[0]));
            string loginToken = Encoding.Default.GetString(Convert.FromBase64String(infos[1]));

            string[] netStrs = Regex.Split(netinfo, ":");
            if (netStrs.Length != 2)
            {
                DebugLogger.Debug("Network NetStrs Length Error, Required 2 Got " + netStrs.Length, NetUtil.ErrorColor);
                throw new NetworkingException((int)ErrorCode.DataError);
            }
            string[] ids = Regex.Split(uid, ":");
            if (ids.Length != 2)
            {
                DebugLogger.Debug("Network Ids Length Error, Required 2 Got " + ids.Length, NetUtil.ErrorColor);
                throw new NetworkingException((int)ErrorCode.DataError);
            }
            DebugLogger.Debug("ProcessLoginData2");
            //#if UNITY_EDITOR
            DebugLogger.Debug("login success，UID：" + ids[0] + "  subid: " + ids[1] +
                                "  gameServer: " + server + "  nodeHost:" + netStrs[0] +
                                "  nodePort:" + netStrs[1] + "  token：" + loginToken);
//#endif
            return new NodeInfo(ids[0], ids[1], server, netStrs[0], Convert.ToInt32(netStrs[1]), loginToken);
        }

        public static byte[] GenHandShake(string uid, string gameServer, string subid, uint connectionIndex)
        {
            int clientReceivedSeq = 0;
            DebugLogger.Debug("GenHandShake uid:" + uid + " gameServer:" + gameServer + " subid:" + subid);
            string handshake = string.Format
            (
                "{0}@{1}#{2}:{3}:{4}",
                Convert.ToBase64String(Encoding.Default.GetBytes(uid)),
                Convert.ToBase64String(Encoding.Default.GetBytes(gameServer)),
                Convert.ToBase64String(Encoding.Default.GetBytes(subid)),
                connectionIndex,
                clientReceivedSeq
            );
            string hmac_encode64 = handshake + ":" + NetCrypter.CryptData(handshake);
            byte[] hmac_encode64_bits = Encoding.Default.GetBytes(hmac_encode64);
            return hmac_encode64_bits;
        }
    }

    public class NodeInfo
    {
        public string uid;
        public string subid;
        public string gameServer;
        public string nodeHost;
        public int nodePort;
        public string token;
        public uint index = 0;

        public NodeInfo(string _uid, string _subid, string _gameServer, string _nodeHost, int _nodePort, string _token)
        {
            uid = _uid;
            subid = _subid;
            gameServer = _gameServer;
            nodeHost = _nodeHost;
            nodePort = _nodePort;
            token = _token;
        }
    }
}


