namespace BaseFramework.Network
{
    public class Const
    {
        /// <summary>
        /// 包体的头文件长度
        /// </summary>
        public const int PackageHeaderLength = 2;

        /// <summary>
        /// 一个包体的最大长度
        /// </summary>
        public const int PackageMaxLength = 65535;

        /// <summary>
        /// 包体的Session长度
        /// </summary>
        public const int PackageSessionLength = 4;

        /// <summary>
        /// 包体的Hmac加密长度
        /// </summary>
        public const int PackageHmacLength = 12;

        /// <summary>
        /// 心跳包长度
        /// </summary>
        public const int HeartBeatLength = 1;

        /// <summary>
        /// 发送数据包的超时时间
        /// </summary>
        public static float PackageTimeout = 5;

        /// <summary>
        /// 重发数据包静默重连次数
        /// </summary>
        public static int PackageSendLimit = 5;
    }
}