using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using NinjaMessage;
using System.Text;
using MsgPack.Serialization;
using Google.Protobuf;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;


namespace BaseFramework.Network
{

	public delegate void RpcCallBackDelegate(byte[] data);

	internal class SerializeDB
	{
		public string playerName { get; set; }
		public string uid { get; set; }
	}
	internal class UserClient : MonoBehaviour
	{
		private bool clientRun = false;
		private IProtocol netProtocol;
		private Login usrLogin;
		private NetClient netClient;
		private int index;
		private Timer heartTimer;
		private Timer timeoutTimer;
		private int heartCount = 0;
		private Int64 netTick = 0;
		private AutoResetEvent sendEvent = new AutoResetEvent(false);
		private SafeQueue<byte[]> sendQueue = new SafeQueue<byte[]>();
		private SafeQueue<byte[]> reciveQueue = new SafeQueue<byte[]>();
		private Dictionary<uint, DataPakage> sessionToCallback = new Dictionary<uint, DataPakage>();
		internal Thread SendThread;
		internal Thread ReceiveThread;



		private Dictionary<string, Action<Message>> severMonitorCallback = new Dictionary<string, Action<Message>>();

		public void regServerMonitor(string funName, Action<Message> ac)
		{
			severMonitorCallback[funName] = ac;
		}

		public UserClient(Login ulogin, int idx, NetClient ncli)
		{
			netClient = ncli;
			index = idx;
			usrLogin = ulogin;
			// 超时检测定时器
			timeoutTimer = new Timer(new TimerCallback(timeoutCheck), null, Timeout.Infinite, Timeout.Infinite);

			//匹配(优先级:3)(初步完成)
			regServerMonitor("NotifyTest", NotifyTest);
			regServerMonitor("NotifyStartMatch", NotifyStartMatch);
			regServerMonitor("NotifyCancelMatch", NotifyCancelMatch);
			regServerMonitor("NotifyMatchSuccess", NotifyMatchSuccess);
			regServerMonitor("NotifyOtherPlayerEnterRoom", NotifyOtherPlayerEnterRoom);
			//end

			//位置同步
			//regServerMonitor("NotifySynchLocation", NotifySynchLocation);

			//ServerModules 接口提供(优先级:1)
			regServerMonitor("NotifyAssign", NotifyAssign);
			regServerMonitor("NotifyBallReady", NotifyBallReady);
			regServerMonitor("NotifyFightStart", NotifyFightStart);
			regServerMonitor("NotifyBallLaunch", NotifyBallLaunch);
			regServerMonitor("NotifyCheck", NotifyCheck);
			regServerMonitor("NotifyNextRound", NotifyNextRound);
			regServerMonitor("NotifyGameOver", NotifyGameOver);
			regServerMonitor("NotifyRoomClean", NotifyRoomClean);
			//end
		}

		//ServerModules 客户端逻辑实现(优先级:1)
		//注! : "..."+"Server"名称指对应的Text

		//分边
		//返回 class Side
		public class Side
		{
			public int side;
		}
		private void NotifyAssign(Message msg)
		{

			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			Side side = (Side)JsonConvert.DeserializeObject(retParam.ToString(), typeof(Side));
			GameManager.Instance.EnterMatch(side.side);

			//ServerModules.AssignServer.text += retParam.ToString();
			DebugLogger.Debug("Side object :" + side.side.ToString());
		}

		//获取对面的小球数组
		//返回 类BallRdArr
		public class BallRdArr
		{
			public BallRdArr(int f, int s, int t)
			{
				first = f;
				second = s;
				third = t;
			}
			public int first;
			public int second;
			public int third;
		}
		private void NotifyBallReady(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			BallRdArr opponBallArray = (BallRdArr)JsonConvert.DeserializeObject(retParam.ToString(), typeof(BallRdArr));

			int[] types = new int[] { opponBallArray.first, opponBallArray.second, opponBallArray.third };
			RoundManager.Instance.DeployEnemy(types);
			//ServerModules.BallReadyServer.text = opponBallArray.ToString();
			//DebugLogger.Debug("opponBallArray class :" + opponBallArray.first.ToString());
		}

		//战斗开始
		//不返回
		private void NotifyFightStart(Message msg)
		{
			RoundManager.Instance.GameStart();

			//ServerModules.FightStartServer.text = "战斗开始,FightStartSever已经触发";
			//DebugLogger.Debug("FightStart");
		}

		//获取对面选择的球的发射数据
		//返回 类BallLaunchClass
		public class BallLaunchClass
		{
			public BallLaunchClass(int bi, int si, int fo, int ra)
			{
				ball_id = bi;
				skill_id = si;
				force = fo;
				radian = ra;
			}
			public int ball_id;
			public int skill_id;
			public int force;
			public int radian;
		}
		private void NotifyBallLaunch(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			BallLaunchClass idBallLaunch = (BallLaunchClass)JsonConvert.DeserializeObject(retParam.ToString(), typeof(BallLaunchClass));

			if (idBallLaunch != null)
			{
				RoundManager.Instance.GetServerMsg(idBallLaunch.ball_id, idBallLaunch.radian, idBallLaunch.force);
			}

			//ServerModules.BallLaunchServer.text = idBallLaunch.radian.ToString();
			//DebugLogger.Debug("idBallLaunch class :" + idBallLaunch.radian.ToString());
		}

		//对方碰撞结束后获取对面球List的所有坐标进行校正
		//返回 类LocalList
		public class Local
		{
			public Local(float ix, float iy)
			{
				x = ix;
				y = iy;
			}
			public float x;
			public float y;
		}
		public class LocalList
		{
			public LocalList(Local iBall_1, Local iBall_2)
			{
				Ball_1 = iBall_1;
				Ball_2 = iBall_2;
			}
			public Local Ball_1;
			public Local Ball_2;
		}

		public class VerifyList
		{
			public VerifyList(int[] _intX, int[] _intY)
			{
				intX = _intX;
				intY = _intY;
			}

			public int[] intX;
			public int[] intY;
		}

		private void NotifyCheck(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			DebugLogger.Debug(retParam.ToString());
			VerifyList verifyList = (VerifyList)JsonConvert.DeserializeObject(retParam.ToString(), typeof(VerifyList));
			int[] x = verifyList.intX;
			int[] y = verifyList.intY;
			RoundManager.Instance.ValidatePosition(x, y);
			//LocalList local = (LocalList)JsonConvert.DeserializeObject(retParam.ToString(), typeof(LocalList));
			//Debug.Log("Check class: " + local.Ball_1.x.ToString());
			//BallMove.ltball.transform.position = new Vector3(asd.x, 0, -5);
		}

		//服务器通知双方调用此方法
		//不返回
		private void NotifyNextRound(Message msg)
		{
			//ServerModules.NextRoundServer.text = "下一回合";
			RoundManager.Instance.NextRound();
		}

		//服务器通知双方调用此方法
		//返回 对面的输赢 结合 玩家客户端自己的输赢判断
		public class IsWin
		{
			public IsWin(bool iIsWin)
			{
				Win = iIsWin;
			}
			public bool Win;
		}
		private void NotifyGameOver(Message msg)
		{
			//ServerModules.GameOverServer.text = "游戏结束";
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			IsWin isWin = (IsWin)JsonConvert.DeserializeObject(retParam.ToString(), typeof(IsWin));
			GameManager.Instance.ConfirmWin(isWin.Win);
			//DebugLogger.Debug(isWin.Win.ToString());
		}

		//测试房间清理用
		private void NotifyRoomClean(Message msg)
		{
			ServerModules.RoomCleanServer.text = "房间清理通知";
		}
		//end


		private void NotifyTest(Message msg)
		{
			//Debug.Log("NotifyTest Seq: " + seq);
			//var rpcFunc = msg.NotifyInfo.RpcParams;
			//object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			//newPeople serializedMsg = retParam as newPeople;
			//Debug.Log(retParam.ToString());
			//newPeople serializedMsg =JsonConvert.DeserializeObject(retParam.ToString()) as newPeople;
			//Debug.Log(serializedMsg.sex);
			//serializedMsg = JsonConvert.DeserializeAnonymousType<newPeople>(rpcFunc.ToString(), new newPeople());
			//Debug.Log(serializedMsg.sex);
			//return;
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			//DebugLogger.Debug(retParam.ToString()+"测试成功");
		}

		private void NotifyStartMatch(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			//Debug.Log(retParam.ToString());
			ServerModules.AssignServer.text += "匹配中...\n";
		}

		private void NotifyCancelMatch(Message msg)
		{

			//object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			//GameNodeRpc.MatchButtonText.text += retParam.ToString();
		}

		private void NotifyMatchSuccess(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			ServerModules.AssignServer.text += "与玩家 " + retParam.ToString() + " 匹配成功\n";
			//GameNodeRpc.JumpFlag = true;
		}

		private void NotifyOtherPlayerEnterRoom(Message msg)
		{

			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			ServerModules.AssignServer.text += retParam.ToString() + " 玩家加入房间\n";
		}

		private void NotifyYourSide(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			GameNodeRpc.TextInfo.text += "\n" + retParam.ToString() + " 边";
		}

		private void NotifySynchLocation(Message msg)
		{
			object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
			//DebugLogger.Debug(retParam.ToString());
			//AttrOfBall asd = (AttrOfBall)JsonConvert.DeserializeObject(retParam.ToString(), typeof(AttrOfBall));
			//Debug.Log(asd.x.ToString());
			//BallMove.ltball.transform.position = new Vector3(asd.x, 0, -5);
			//GameNodeRpc.TextInfo.text += "\n" + retParam.ToString() + " 边";
		}


		public class AttrOfBall
		{
			public float x;
			//public float y;
		}


		// 对发送给服务端的数据进行打包
		public void rpcCall(string funcname, string parameters = null, RpcCallBackDelegate callback = null)
		{
			// generate protobuf msg
			var tmp = new NinjaMessage.Message
			{
				OpCode = NinjaMessage.OPCODE.RpcCall,
				Uid = int.Parse(netProtocol.Node.uid)
			};
			tmp.Request = new NinjaMessage.Request();
			tmp.Request.RpcFunc = funcname;

			// use msgpack to pack the parameters
			if (parameters != null)
			{
				string paramString = parameters;
				var serializer = MessagePackSerializer.Get<string>();
				using (var byteStream = new MemoryStream())
				{
					serializer.Pack(byteStream, paramString);
					byte[] tmpBytes = new byte[byteStream.Length];
					int iter = 0;
					byteStream.Seek(0, SeekOrigin.Begin);
					StringBuilder myStringBuilder = new StringBuilder();
					while (iter < tmpBytes.Length)
					{
						byte numIter = (byte)(byteStream.ReadByte());
						char tempChar = '\u0000';
						tempChar += (char)numIter;
						myStringBuilder.Append(tempChar);
						tmpBytes[iter++] = numIter;
					}
					tmp.Request.RpcParams = Convert.ToBase64String(tmpBytes);
				}
			}

			// use protobuf to encode message
			byte[] protoMsg;
			using (var byteStream = new MemoryStream())
			{
				tmp.WriteTo(byteStream);
				protoMsg = new byte[byteStream.Length];
				int iter = 0;
				byteStream.Seek(0, SeekOrigin.Begin);
				while (iter < protoMsg.Length)
				{
					protoMsg[iter++] = (byte)(byteStream.ReadByte());
				}
			}

			// DebugLogger.Debug(tmp.Request.RpcParams);
			SendMessage(protoMsg, "RPC_CALL", callback);
		}

		private void RpcNotify(byte[] data)
		{
			var msg = ProtobufDecoder(data);

			//DebugLogger.Debug(msg.OpCode.ToString());

			if (msg.OpCode == OPCODE.NotifyInfo)
			{
				var seq = msg.NotifyInfo.Sequence;
				if (seq > 0)
				{
					//DebugLogger.Debug("Seq: " + seq.ToString());
					//GameNodeRpc.NotificationMsg.text = seq.ToString() + "\n";
					// clientReceiveSeq = seq;
				}

				var rpcFunc = msg.NotifyInfo.RpcFunc;
				//Debug.Log(rpcFunc.ToString());
				if (severMonitorCallback.ContainsKey(rpcFunc.ToString()))
				{
					severMonitorCallback[rpcFunc.ToString()](msg);
				}
				if (rpcFunc == null)
				{
					DebugLogger.DebugError("RpcNotify wrong fucntion code");
				}
				else
				{
					object retParam = MessagePackDecoder<object>(msg.NotifyInfo.RpcParams);
					//int i = 0;
					//GameNodeRpc.NotificationMsg.text += retParam.ToString();
				}
			}
		}

		private Message ProtobufDecoder(byte[] data)
		{
			Message msg = new Message();
			return Message.Parser.ParseFrom(data);
		}

		private T MessagePackDecoder<T>(string param)
		{
			var msgPackDecoder = MessagePackSerializer.Get<T>();
			byte[] byteArray = Convert.FromBase64String(param);
			T RspMsg;
			using (var rpcRspStream = new MemoryStream(byteArray))
			{
				RspMsg = msgPackDecoder.Unpack(rpcRspStream);
			}
			return RspMsg;
		}

		internal void SerializeRpcCallback(byte[] data)
		{
			var msg = ProtobufDecoder(data);

			if (msg.Response.RpcRsp != null)
			{
				var RspMsg = MessagePackDecoder<object>(msg.Response.RpcRsp);
				DebugLogger.Debug(RspMsg.ToString());

				SerializeDB serializedMsg;
				serializedMsg = JsonConvert.DeserializeAnonymousType<SerializeDB>(RspMsg.ToString(), new SerializeDB());
				GameNodeRpc.SerializedPlayername.text = serializedMsg.playerName;
				GameNodeRpc.SerializedUid.text = serializedMsg.uid;
			}
		}


		internal void LoginRpcCallback(byte[] data)
		{
			var msg = ProtobufDecoder(data);

			if (msg.Response.RpcRsp != null)
			{
				var needCreate = MessagePackDecoder<long>(msg.Response.RpcRsp);
				DebugLogger.Debug(needCreate.ToString());
				if (needCreate == 1)
				{
					RpcCallBackDelegate createPlayerCallback = new RpcCallBackDelegate(userCreateCallback);
					rpcCall("user.create", "player_" + netProtocol.Node.uid, createPlayerCallback);
				}
			}

		}

		private void userCreateCallback(byte[] data)
		{
			var msg = ProtobufDecoder(data);

			if (msg.Response.RpcRsp != null)
			{
				var createdUserInfo = MessagePackDecoder<object>(msg.Response.RpcRsp);

				SerializeDB serializedMsg;
				serializedMsg = JsonConvert.DeserializeAnonymousType<SerializeDB>(createdUserInfo.ToString(), new SerializeDB());

				DebugLogger.Debug("Created game agent for player" + serializedMsg.playerName);
			}

		}


		// 接受服务端发送的消息 调用相关的回调函数
		public void MyUpdate()
		{
			while (reciveQueue.Count > 0)
			{
				var msg = reciveQueue.Dequeue();
				uint session;
				var recData = DataPack.UnPack(msg, out session);
				if (recData.Length <= 0)
				{
					// 心跳包，重置心跳计数
					heartCount = 0;
					continue;
				}
				if (session == 0)
				{
					RpcNotify(recData);
				}
				else if (sessionToCallback.ContainsKey(session))
				{
					var callback = sessionToCallback[session].CallBackFunc;
					if (null != callback)
					{
						callback(recData);
					}
					// 加锁删除数据
					lock (sessionToCallback)
					{
						sessionToCallback.Remove(session);
						netProtocol.WaitRecive--;
					}
				}
				else
				{
					DebugLogger.DebugError("Session invalid:" + session);
				}
			}

		}

		private void timeoutCheck(object o)
		{
			if (netProtocol.WaitRecive > 20)
			{
				List<uint> delList = new List<uint>();
				var sessions = sessionToCallback.Keys;
				foreach (uint sess in sessions)
				{
					if (sessionToCallback[sess].CheckExpire(netTick))
					{
						delList.Add(sess);
					}
				}
				lock (sessionToCallback)
				{
					foreach (uint sess in delList)
					{
						sessionToCallback.Remove(sess);
					}
				}
			}
			timeoutTimer.Change(30000, Timeout.Infinite);
		}

		internal bool ChkException(Exception e)
		{
			if (e == null)
			{
				return false;
			}
			int err;
			if (e is SocketException)
			{
				err = (int)ErrorCode.SocketError;
			}
			else if (e is NetworkingException)
			{
				err = ((NetworkingException)e).code;
			}
			else
			{
				err = (int)ErrorCode.DataError;
			}
			DebugLogger.DebugNetworkError("ConnectLogin Error " + err + " " + e.GetType() + e.Message + e.StackTrace);
			return true;
		}

		public bool Login(string ip, int port, string secret)
		{
			// 连接服务器
			if (ChkException(usrLogin(ip, port, secret, out netProtocol)))
			{
				OnError("ConnectLogin");
				rpcCall("user.get_login_info", null, null);
				return false;
			}
			else
			{
				// 登陆成功
				DebugLogger.Debug("Login success");
			}
			RpcCallBackDelegate callback = new RpcCallBackDelegate(LoginRpcCallback);
			rpcCall("user.get_login_info", null, callback);

			// 启动发送和接收
			StartNetWorkService();
			return true;
		}

		internal void StartNetWorkService()
		{
			StartSendAndReceive();
			// 启动心跳
			startHeart(null);
			// 启动超时包检测
			timeoutTimer.Change(30000, Timeout.Infinite);
		}


		public string GetUserID()
		{
			return netProtocol.Node.uid;
		}

		// 向服务器发信并注册回调函数
		public void SendMessage(byte[] message, string opCode, RpcCallBackDelegate callback)
		{
			uint session = 0;
			if (callback != null)
			{
				session = NetUtil.GetSessionID();

				lock (sessionToCallback)
				{
					sessionToCallback[session] = new DataPakage { CallBackFunc = callback, Session = session, cTick = netTick };
					netProtocol.WaitRecive++;
				}
			}
			var data = DataPack.Pack(session, opCode, message);
			sendQueue.Enqueue(data);
			sendEvent.Set();
		}

		private void onRecive(byte[] msg)
		{
			reciveQueue.Enqueue(msg);
			if (heartTimer != null)
			{
				heartTimer.Change(5000, Timeout.Infinite);
			}
		}

		private void startHeart(object o)
		{
			heartTimer = new Timer(new TimerCallback(heartBeat), o, 3000, Timeout.Infinite);
		}


		private void heartBeat(object o)
		{
			if (heartCount > 3)
			{
				heartCount = 0;
				OnError("heart beat timeout");
				var e = netProtocol.OutException(NetException.HeartBeatTimeOut);
				if (e != null)
				{
					throw e;
				}
			}
			else
			{
				sendQueue.Enqueue(new byte[3] { 0, 0x01, 0 });
				sendEvent.Set();
				heartCount++;
			}
			heartTimer.Change(3000, Timeout.Infinite);
		}

		// 开始接收和发送线程
		private void StartSendAndReceive()
		{
			clientRun = true;
			SendThread = new Thread(startSend);
			SendThread.Start();
			ReceiveThread = new Thread(startRecive);
			ReceiveThread.Start();
		}

		private void startSend()
		{
			Exception e = null;
			while (clientRun)
			{
				var msg = sendQueue.Dequeue();
				if (msg == null)
				{
					sendEvent.WaitOne();
					continue;
				}
				e = netProtocol.Send(msg);
				if (e != null)
				{
					OnError(e.ToString());
					if (clientRun)
					{
						throw e;
					}
				}
			}
		}
		// 接收逻辑
		private void startRecive()
		{
			Exception e = null;
			while (clientRun)
			{
				try
				{
					var msg = netProtocol.Read();
					onRecive(msg);
				}
				catch (Exception ce)
				{
					// 此处捕获的异常为不可恢复异常，直接退回重登
					OnError(ce.ToString());
					if (clientRun)
					{
						e = ce;
						throw ce;
					}
				}
			}
		}

		public void OnError(string err)
		{
			DebugLogger.DebugNetworkError("NetClient OnError:" + err);
			lock (sessionToCallback)
			{
				sessionToCallback.Clear();
				netProtocol.WaitRecive = 0;
			}
		}

		// 主动关闭连接
		public void Close()
		{
			netClient.Close(index);
		}

		public void CloseSelf()
		{
			Dispose();
		}

		public void Dispose()
		{
			clientRun = false;
			netProtocol.Close();
			heartTimer.Dispose();
			timeoutTimer.Dispose();

			lock (sessionToCallback)
			{
				sessionToCallback.Clear();
				netProtocol.WaitRecive = 0;
			}
		}
	}
}

