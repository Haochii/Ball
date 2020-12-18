using System;
using System.Text;

namespace BaseFramework.Network
{
    public class NetCrypter
    {
        private static byte[] m_secret;

        public static void setSecret(byte[] secret)
        {
            m_secret = secret;
        }

        public static string CryptData(string _data)
        {
            byte[] bytes = Encoding.Default.GetBytes(_data);
            return CryptData(bytes, 0, bytes.Length);
        }

        public static string CryptData(byte[] bytes)
        {
            return CryptData(bytes, 0, bytes.Length);
        }

        public static string CryptData(byte[] _data, int _index, int _length)
        {
            if (_index < 0 || _index + _length > _data.Length)
            {
                return null;
            }
            return Convert.ToBase64String(Crypt.hmac64(Crypt.hashkey(_data, _index, _length), m_secret)); ;
        }
    }
}