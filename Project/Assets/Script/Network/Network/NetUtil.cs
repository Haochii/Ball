using UnityEngine;
using System;
using System.Collections.Generic;


namespace BaseFramework.Network
{
    // 登陆函数
    internal delegate Exception Login(string ip, int prot, string account, out IProtocol p);
    internal delegate Exception ReConnect(NodeInfo node, IProtocol p); // 接收消息时的委托
    // 协议配置的数据结构体，如果所有的配置均为数字可以去掉这个，暂时等待KCP完成后再来决定这个
    internal struct ProtocolCfg
    {
        public int intElement;
        public string strElement;
    }
    // 协议接口，主要用于实现TCP，UDP等具体的收发消息的方法
    internal interface IProtocol
    {
        void SetCfg(Dictionary<string, ProtocolCfg> cfg);
        NodeInfo Node { get; set; }
        int WaitRecive { get; set; }
        Exception Connect();
        Exception Send(byte[] msg);
        byte[] Read();
        Exception OutException(Exception e);
        void Close();
    }

    public static class NetException {
        // 心跳超时
        public static Exception HeartBeatTimeOut = new Exception("heartbeattimeout");
        // 协议已经停止
        public static Exception ProtoctoStop = new Exception("protocol already stop");

    }
    // 数值转字节或字节转数值类
    public class NetUtil
    {
        public static ushort bigEndian2ushort(byte[] data, int offset, int len)
        {
            int _length = 2;
            ushort num = 0;
            int shiftWidth = 8;

            for (int i = 0; i < _length && i + offset < len; i++)
            {
                num <<= shiftWidth;
                num += data[i + offset];
            }

            return num;
        }

        public static ushort bigEndian2ushort(byte[] data)
        {
            return bigEndian2ushort(data, 0, data.Length);
        }

        public static byte[] ushort2bigEndian(ushort _num)
        {
            byte[] result = new byte[2];
            ushort2bigEndian(result, _num, 0);
            return result;
        }

        public static void ushort2bigEndian(byte[] _data, ushort _num, int _offset)
        {
            int _length = 2;
            int shiftWidth = 8;
            while (--_length >= 0 && _length + _offset < _data.Length)
            {
                _data[_length + _offset] = (byte)(_num);
                _num >>= shiftWidth;
            }
        }

        public static uint bigEndian2UInt(byte[] _data, int _offset, int _len)
        {
            int _length = 4;
            uint num = 0;
            int shiftWidth = 8;

            for (int i = 0; i < _length && i < _len; i++)
            {
                num <<= shiftWidth;
                num += _data[i + _offset];
            }

            return num;
        }

        public static byte[] uInt2BigEndian(uint _num)
        {
            byte[] result = new byte[4];
            uInt2BigEndian(result, _num, 0);
            return result;
        }

        public static void uInt2BigEndian(byte[] _data, uint _num, int _offset)
        {
            int _length = 4;
            int shiftWidth = 8;
            while (--_length >= 0 && _length + _offset < _data.Length)
            {
                _data[_length + _offset] = (byte)(_num);
                _num >>= shiftWidth;
            }
        }


        static Color errorColor;

        public static Color ErrorColor
        {
            get
            {
                if (errorColor == Color.clear)
                {
                    errorColor = new Color(1, 0.5f, 0.5f, 1);
                }
                return errorColor;
            }
        }

        static uint m_session = 1;

        public static uint GetSessionID()
        {
            uint result = m_session;
            m_session++;
            return result;
        }

    }
}