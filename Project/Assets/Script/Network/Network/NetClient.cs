using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseFramework.Network
{
    // 网络业务层，控制玩家先连接login，再连接node，为上层提供send接口并根据response回调上层
    //
    class NetClient : SingletonBehaviour<NetClient>
    {
        private static int sClientId = 0;
        internal static Dictionary<int, UserClient> sClients = new Dictionary<int, UserClient>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        // 指定连接类型，用于创建战斗类
        public static UserClient GetInstance(string ctype)
        {
            var ncl = new UserClient(NetConfigDict.config[ctype], ++sClientId, Instance);
            sClients[sClientId] = ncl;
            sClientId++;
            return ncl;
        }

        private void Update()
        {
            var keys = new List<int>(sClients.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                sClients[keys[i]].MyUpdate();
            }
        }

        protected override void OnApplicationQuit()
        {
            var keys = new List<int>(sClients.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                sClients[keys[i]].Dispose();
            }
            base.OnApplicationQuit();
        }

        // 主动关闭连接，删除列表中的客户端实例
        public void Close(int idx)
        {
            if (!sClients.ContainsKey(idx))
            {
                return;
            }
            sClients[idx].CloseSelf();
            sClients.Remove(idx);
        }
    }

    public class NetworkingException : Exception
    {
        public int code;

        public string message;

        public NetworkingException(int code)
        {
            this.code = code;
        }

        public NetworkingException(int code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
    public enum ErrorCode
    {
        /// <summary>
        /// 什么都没发生
        /// </summary>
        None = 0,

        /// <summary>
        /// 正常，什么都没发生
        /// </summary>
        Success = 200,

        /// <summary>
        /// 数据长度超过65535
        /// </summary>
        TooLong = 300,

        /// <summary>
        /// 加密错误
        /// </summary>
        HmacError = 301,

        /// <summary>
        /// 调用错误，一般出现在服务器运行错误
        /// </summary>
        CallError = 302,

        /// <summary>
        /// 认证失败，认证过程中出现任何问题都会出现这个错误，
        /// 客户端重连的过程中没有把重连索引+1也会产生这个问题
        /// </summary>
        Unauthorized = 303,

        /// <summary>
        /// 索引过期，这时候已经登陆不上Node服务器了
        /// </summary>
        IndexExpired = 304,

        /// <summary>
        /// 登陆票据失效
        /// </summary>
        TokenError = 305,

        /// <summary>
        /// 需要通过手机号登陆
        /// </summary>
        BindPhoneError = 306,

        /// <summary>
        /// 已经重新登陆
        /// </summary>
        AlreadyLogin = 307,

        /// <summary>
        /// 已经被封号
        /// </summary>
        Forbidden = 308,

        /// <summary>
        /// 手机解绑建号冷冻期
        /// </summary>
        PhoneFrozenStage = 309,

        /// <summary>
        /// 需要手机验证码
        /// </summary>
        NeedVerificationCode = 310,

        /// <summary>
        /// 验证码错误
        /// </summary>
        WrongVerificationCode = 311,

        /// <summary>
        /// 登录目标繁忙
        /// </summary>
        LoginBusy = 314,

        /// <summary>
        /// 包体签名错误
        /// </summary>
        SignError = 315,

        /// <summary>
        /// 登录包体错误
        /// </summary>
        PackageError = 316,

        /// <summary>
        /// Notify序列过期
        /// </summary>
        NotifySequenceError = 323,

        /// <summary>
        /// 接连NodeDns错误
        /// </summary>
        DnsError = 351,

        /// <summary>
        /// 连接NodeSocket错误
        /// </summary>
        SocketError = 352,

        /// <summary>
        /// 连接Node数据错误
        /// </summary>
        DataError = 353,

        /// <summary>
        /// 没有网络连接，现在通过引擎的函数判断
        /// </summary>
        NoNetwork = 400,

        /// <summary>
        /// 发送数据包超时
        /// </summary>
        SendTimeout = 401,

        /// <summary>
        /// 资源服找不到资源
        /// </summary>
        NotFound = 2002,
    }
}

