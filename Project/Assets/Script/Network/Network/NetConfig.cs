using System.Collections.Generic;

namespace BaseFramework.Network
{
    public static class NetConfigDict
    {
        internal static Dictionary<string, Login> config = new Dictionary<string, Login>();
        public static void Init()
        {
            // 普通逻辑服
            config["logic"] = new Login(UsrLogin.Login);
            // 战斗逻辑
            config["battle"] = new Login(BattleLogin.Login);
        }
    }
}