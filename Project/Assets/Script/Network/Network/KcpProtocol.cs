using System.Net.Sockets;
using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

namespace BaseFramework.Network
{
    class KcpProtocol : IProtocol
    {
        private bool protocolRun = false;
        private string udpIp;
        private int udpPort;
        private uint session = 0;
        private KCP mKCP = null;
        private Timer updateTimer = null;
        private bool timerFlag = false;
        private byte[] mRecvBuffer = new byte[1024 * 2];
        private byte[] sRecvBuffer = new byte[1024 * 2];
        private byte[][] redBuffer = null;
        private UInt32 mNextUpdateTime = 0;
        private int writeDelay = 10; // 默认有写入延迟，延迟为10ms
        private bool ackNoDelay = true; // 默认有ack确认延迟
        private bool streamMode = false; // 是否使用流模式，默认不启用
        // kcp 设置参数，默认使用急速模式
        private int nodelay = 1, interval = 30, resend = 2, nc = 1;
        private int recwnd = 128;
        private int sndwnd = 128;
        private int redsndidx = -1, redlen = 0;
        private List<byte>[] redData;
        private byte[] redbitmap;
        private UInt32 minredidx = 0;
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
        private Socket udpClient = null;
        private EndPoint recIEpoint;
        private IPEndPoint sndIEpoint;
        private Dictionary<string, ProtocolCfg> kcpConfig;
        public KcpProtocol(string ip, int port)
        {
            udpIp = ip;
            udpPort = port;
        }
        public void SetCfg(Dictionary<string, ProtocolCfg> cfg)
        {
            kcpConfig = cfg;
            // 设置写延迟
            if (kcpConfig.ContainsKey("writeDelay"))
            {
                writeDelay = kcpConfig["writeDelay"].intElement;
            }
            // 设置ack延迟
            if (kcpConfig.ContainsKey("ackNoDelay"))
            {
                ackNoDelay = kcpConfig["ackNoDelay"].intElement != 0;
            }
            // 设置是否启用流模式
            if (kcpConfig.ContainsKey("streamMode"))
            {
                streamMode = kcpConfig["streamMode"].intElement != 0;
            }
            // 设置是否启用流模式
            if (kcpConfig.ContainsKey("streamMode"))
            {
                streamMode = kcpConfig["streamMode"].intElement != 0;
            }

            if (kcpConfig.ContainsKey("nodelay"))
            {
                nodelay = kcpConfig["nodelay"].intElement;
            }
            if (kcpConfig.ContainsKey("interval"))
            {
                interval = kcpConfig["interval"].intElement;
            }
            if (kcpConfig.ContainsKey("resend"))
            {
                resend = kcpConfig["resend"].intElement;
            }
            if (kcpConfig.ContainsKey("nc"))
            {
                nc = kcpConfig["nc"].intElement;
            }
        }
        public Exception Connect()
        {
            udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpClient.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), udpPort));
            recIEpoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), udpPort);
            sndIEpoint = new IPEndPoint(IPAddress.Parse(udpIp), udpPort);
            // 创建KCP实例
            session = (uint)(new Random(DateTime.Now.Millisecond).Next(1, Int32.MaxValue)) + 1;
            mKCP = new KCP(session, rawSend);
            // 设置kcp模式 
            // normal:  0, 40, 2, 1
            // fast:    0, 30, 2, 1
            // fast2:   1, 20, 2, 1
            // fast3:   1, 10, 2, 1
            mKCP.NoDelay(nodelay, interval, resend, nc);
            mKCP.SetStreamMode(streamMode);
            // 设置接收和发送窗大小, 默认都为128
            mKCP.WndSize(sndwnd, recwnd);
            // 设置冗余长度，默认不使用冗余
            if (redlen > 0)
            {
                redData = new List<byte>[redlen];
                Array.Clear(redData, 0, redlen);
                int maxdatalen = redlen + recwnd;
                int bitmaplen = maxdatalen >> 3;
                if (maxdatalen > bitmaplen << 3)
                {
                    bitmaplen++;
                }
                redbitmap = new byte[bitmaplen];
                // 初始化冗余数据buffer
                redBuffer = new byte[redlen + 1][];
                for (int i = 0; i < redlen + 1; i++)
                {
                    redBuffer[i] = new byte[256];
                }
            }
            // 启动更新定时器
            if (writeDelay > 0)
            {
                updateTimer = new Timer(new TimerCallback(kcpUpdate), null, Timeout.Infinite, Timeout.Infinite);
            }
            protocolRun = true;
            return null;
        }
        // 原始的发送方法
        private void rawSend(byte[] data, int length)
        {
            if (udpClient == null)
            {
                return;
            }
            udpClient.SendTo(data, length, SocketFlags.None, sndIEpoint);
        }
        // 冗余数据处理
        private byte[] redFmt(byte[] msg)
        {
            if (redlen <= 0)
            {
                return msg;
            }
            List<byte> tmsg = new List<byte>();
            tmsg.Add(0xff);
            tmsg.AddRange(msg);
            lock (redData)
            {
                // 拼接冗余数据
                for (int i = 0; i < redlen; i++)
                {
                    int tidx = redsndidx - i;
                    if (tidx < 0)
                    {
                        tidx += redlen;
                    }
                    if (redData[tidx] == null)
                    {
                        break;
                    }
                    if (tmsg.Count + redData[tidx].Count > 1400)
                    {
                        break;
                    }
                    tmsg.AddRange(redData[tidx]);
                }
                redsndidx++;
                if (redsndidx >= redlen) redsndidx = 0;
                if (redData[redsndidx] == null)
                {
                    redData[redsndidx] = new List<byte>();
                }
                redData[redsndidx].Clear();
                if (msg.Length > 1400)
                {
                    redData[redsndidx].AddRange(new byte[] { 0, 0 });
                }
                else
                {
                    redData[redsndidx].AddRange(msg);
                }
            }
            return tmsg.ToArray();
        }

        public Exception Send(byte[] msg)
        {
            if (udpClient == null) return NetException.ProtoctoStop;
            // 处理冗余
            msg = redFmt(msg);
            var waitsnd = mKCP.WaitSnd;
            if (waitsnd < mKCP.SndWnd && waitsnd < mKCP.RmtWnd)
            {
                lock (mKCP)
                {
                    var sendBytes = 0;
                    do
                    {
                        var n = Math.Min((int)mKCP.Mss, msg.Length - sendBytes);
                        var res = 0;
                        try
                        {
                            res = mKCP.Send(msg, sendBytes, n);
                        }
                        catch (Exception e)
                        {
                            return onException(e);
                        }
                        if (res < 0)
                        {
                            return new Exception("mKCP.Send fail");
                        }
                        sendBytes += n;
                    } while (sendBytes < msg.Length);

                    waitsnd = mKCP.WaitSnd;
                    if (waitsnd >= mKCP.SndWnd || waitsnd >= mKCP.RmtWnd || writeDelay == 0)
                    {
                        mKCP.Flush(false);
                    }
                }
            }
            chkUpdate();
            return null;
        }

        private int rawRead()
        {
            var n = udpClient.ReceiveFrom(sRecvBuffer, ref recIEpoint);
            return n;
        }

        private bool chkBitmap(uint idx)
        {
            int maxdatalen = (int)(idx % (uint)(redlen + recwnd));
            int seat = maxdatalen >> 3;
            int index = maxdatalen - (seat << 3);
            return ((redbitmap[seat] & (1 << index)) > 0);
        }

        private void setBitmap(uint idx, bool val)
        {
            int maxdatalen = (int)(idx % (uint)(redlen + recwnd));
            int seat = maxdatalen >> 3;
            int index = maxdatalen - (seat << 3);
            if (val) redbitmap[seat] |= (byte)(0x01 << index);
            else redbitmap[seat] &= (byte)(~(0x01 << index));
        }

        // 处理接收冗余
        private int redRevFmt(byte[] data, int offset, int kcplen)
        {
            int pnum = 0;
            int pDataLen = 0;
            UInt32 sn = 0, csn;
            KCP.ikcp_decode32u(data, offset + 12, ref sn);
            int rIndex = 24 + offset;
            if (data[rIndex] == 0xff) rIndex += 1;
            for (UInt32 i = 0; i < (redlen + 1); i++)
            {
                if (i == 0 && (data[24 + offset] != 0xff || data[offset + 5] > 0))
                    pDataLen = kcplen - rIndex + offset;
                else pDataLen = (((int)data[rIndex]) << 8) + data[rIndex + 1] + 2;
                csn = sn - i;
                // 验证是否已被接收，第一个包只记录不做验证
                if (i >= 1)
                {
                    if (csn < minredidx) break;
                    if (chkBitmap(csn)) continue;
                    if (pDataLen <= 2) continue;
                }
                if (redBuffer[i].Length < 24 + pDataLen)
                {
                    redBuffer[i] = new byte[24 + pDataLen];
                }
                Array.Copy(data, offset, redBuffer[i], 0, 24);
                Array.Copy(data, rIndex, redBuffer[i], 24, pDataLen);
                // 修改包数据
                KCP.ikcp_encode32u(redBuffer[i], 12, csn);
                KCP.ikcp_encode32u(redBuffer[i], 20, (uint)pDataLen);
                rIndex += pDataLen;
                pnum++;
                setBitmap(csn, true);
                if (minredidx == csn)
                {
                    while (chkBitmap(minredidx))
                    {
                        // 设置为1
                        setBitmap(minredidx, false);
                        // 检查下一个是否提交，提交继续往前累加
                        minredidx++;
                    }
                }
                if (rIndex >= kcplen + offset) break;
            }
            return pnum;
        }

        private byte[] kcpRead()
        {
            while (udpClient != null)
            {
                var size = mKCP.PeekSize();
                if (size > 0)
                {
                    lock (mKCP)
                    {
                        if (size > mRecvBuffer.Length)
                        {
                            mRecvBuffer = new byte[(size * 3) / 2];
                        }
                        var n = mKCP.Recv(mRecvBuffer, 0, size);
                        var recvBytes = (((int)mRecvBuffer[0]) << 8) + mRecvBuffer[1];
                        if (recvBytes <= n - 2)
                        {
                            byte[] data = new byte[recvBytes];
                            Array.Copy(mRecvBuffer, Const.PackageHeaderLength, data, 0, recvBytes);
                            return data;
                        }
                    }
                }
                var reclen = rawRead();
                int inputN = 0;
                lock (mKCP)
                {
                    int sidx = 0;
                    while (sidx < reclen)
                    {
                        uint datalen = 0;
                        KCP.ikcp_decode32u(sRecvBuffer, sidx + 20, ref datalen);
                        int kcplen = (int)datalen + 24;
                        if (redlen <= 0 || sRecvBuffer[4 + sidx] != KCP.IKCP_CMD_PUSH)
                        {
                            inputN = mKCP.Input(sRecvBuffer, sidx, kcplen, true, ackNoDelay);
                        }
                        else
                        {
                            int pnum = redRevFmt(sRecvBuffer, sidx, kcplen);
                            for (int i = 0; i < pnum; i++)
                            {
                                uint tDataLen = 0;
                                KCP.ikcp_decode32u(redBuffer[i], 20, ref tDataLen);
                                inputN = mKCP.Input(redBuffer[i], 0, (int)tDataLen + 24, true, ackNoDelay);
                            }
                        }
                        sidx += kcplen;
                    }
                }
                if (inputN < 0)
                {
                    DebugLogger.Debug($"input err resinput {inputN}");
                }
            }
            return null;
        }

        public byte[] Read()
        {
            var msg = kcpRead();
            if (msg == null)
            {
                throw new Exception("read data err");
            }
            return msg;
        }
        private void chkUpdate()
        {
            lock (updateTimer)
            {
                if (!timerFlag)
                {
                    updateTimer.Change(writeDelay, Timeout.Infinite);
                    timerFlag = true;
                }
            }
        }
        // kcp 更新
        private void kcpUpdate(object o)
        {
            if (0 == mNextUpdateTime || mKCP.CurrentMS >= mNextUpdateTime)
            {
                lock (mKCP)
                {
                    try
                    {
                        mKCP.Update();
                    }
                    catch (Exception e)
                    {
                        var re = onException(e);
                        // 此更新循环无上层逻辑，所有报错直接忽略
                    }
                    mNextUpdateTime = mKCP.Check();
                }
            }
            lock (updateTimer)
            {
                updateTimer.Change(50, Timeout.Infinite);
                timerFlag = false;
            }
        }

        private Exception reConnect()
        {
            if (!protocolRun)
            {
                return NetException.ProtoctoStop;
            }
            udpClient.Close();
            try
            {
                udpClient.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), udpPort));
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
        // 外部传进来的错误
        public Exception OutException(Exception e)
        {
            return onException(e);
        }
        // udp 无重连逻辑
        private Exception onException(Exception e)
        {
            // kcp协议无连接，不需要重连，所有报错目前全部上抛
            return e;
        }
        public void Close()
        {
            Despose();
        }
        public void Despose()
        {
            var msg = System.Text.Encoding.UTF8.GetBytes("\0\tClient_ShutDown");
            Send(msg);
            if (updateTimer != null)
            {
                updateTimer.Dispose();
                timerFlag = true;
            }
            protocolRun = false;
            udpClient.Close();
            udpClient = null;
        }
    }
}

