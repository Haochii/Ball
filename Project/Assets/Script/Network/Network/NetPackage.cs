using System;
using System.Text;

namespace BaseFramework.Network

{
    public class DataPakage
    {
        public uint Session;
        public RpcCallBackDelegate CallBackFunc;
        public Int64 cTick  = 0;
        // 检测是否过期
        public bool CheckExpire(Int64 ntick)
        {
            // TODO 超时时间暂定为30s
            if (cTick < (ntick- 900))
            {
                return true;
            }
            return false;
        }
    }
    // 打包发送数据(注意已经包含数据长度的数据)
    public static class DataPack
    {
        public static byte[] Pack(uint sess, string opCode, byte[] data)
        {
            int index = 0;
            int msglen = data.Length + Const.PackageSessionLength + Const.PackageHmacLength;
            byte[] header = NetUtil.ushort2bigEndian((ushort)msglen);
            var SendData = new byte[header.Length + msglen];
            // 拷贝文件长度数据
            Buffer.BlockCopy(header, 0, SendData, index, header.Length);
            index += header.Length;
            //先将数据拷贝到数据包//
            Buffer.BlockCopy(data, 0, SendData, index, data.Length);
            index += data.Length;
            //获得Session的Byte数据//
            byte[] session = NetUtil.uInt2BigEndian(sess);
            //拷贝Session数据//
            Buffer.BlockCopy(session, 0, SendData, index, session.Length);
            index += session.Length;
            //计算Hmac数据//
            byte[] hmac = Encoding.Default.GetBytes(NetCrypter.CryptData(SendData, header.Length, index));
            //拷贝Hmac数据//
            Buffer.BlockCopy(hmac, 0, SendData, index, hmac.Length);
            return SendData;
        }
        public static byte[] UnPack(byte[] data, out uint sess)
        {
            sess = 0;
            var len = data.Length;
            if (len == Const.HeartBeatLength)
            {
                return new byte[] { };
            }
            //先检查下最小长度//
            if (len < Const.PackageSessionLength + Const.PackageHmacLength)
            {
                DebugLogger.DebugNetworkError("Receve Data Length Error! Got " + len + " Byte, Less than Session Length plus Hmac Length");
                throw new NetworkingException((int)ErrorCode.DataError);
            }
            string hmac_pkg = NetCrypter.CryptData(data, 0, len - Const.PackageHmacLength);
            string hmac_server = Encoding.Default.GetString(data, len - Const.PackageHmacLength, Const.PackageHmacLength);
            if (hmac_pkg != hmac_server)
            {
                DebugLogger.DebugNetworkError("HMAC Invalid! hmac_pkg:" + hmac_pkg + ", hmac_server: " + hmac_server);
                throw new NetworkingException((int)ErrorCode.DataError);
            }
            int dataLength = len - Const.PackageHmacLength - Const.PackageSessionLength;
            //获得SessionID//
            sess = NetUtil.bigEndian2UInt(data, dataLength, Const.PackageSessionLength);
            //拷贝获得的接收数据//
            var ReceiveData = new byte[dataLength];
            Buffer.BlockCopy(data, 0, ReceiveData, 0, dataLength);
            return ReceiveData;
        }
        // 将长度封装到数据包
        public static byte[] PackLength(byte[] data){
            var msg = new byte[Const.PackageHeaderLength + data.Length];
            byte[] header = NetUtil.ushort2bigEndian((ushort)data.Length);
            Buffer.BlockCopy(header, 0, msg, 0, Const.PackageHeaderLength);
            Buffer.BlockCopy(data, 0, msg, Const.PackageHeaderLength, data.Length);
            return msg;
        }
    }
}

