using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace BaseFramework.Network
{
    public class ServerModules : MonoBehaviour
    {
        internal static Text AssignServer;
        internal static Text BallReadyServer;
        internal static Text FightStartServer;
        internal static Text BallLaunchServer;
        internal static Text CheckServer;
        internal static Text NextRoundServer;
        internal static Text GameOverServer;
        internal static Text RoomCleanServer;

        Button AssignButton;
        Button BallReadyButton;
        Button FightStartButton;
        Button BallLaunchButton;
        Button CheckButton;
        Button NextRoundButton;
        Button GameOverButton;
        Button RoomCleanButton;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            AssignServer = GameObject.Find("AssignText").GetComponent<Text>();
            BallReadyServer = GameObject.Find("BallReadyText").GetComponent<Text>();
            FightStartServer = GameObject.Find("FightStartText").GetComponent<Text>();
            BallLaunchServer = GameObject.Find("BallLaunchText").GetComponent<Text>();
            CheckServer = GameObject.Find("CheckText").GetComponent<Text>();
            NextRoundServer = GameObject.Find("NextRoundText").GetComponent<Text>();
            GameOverServer = GameObject.Find("GameOverText").GetComponent<Text>();
            RoomCleanServer = GameObject.Find("RoomCleanText").GetComponent<Text>();

            AssignButton = GameObject.Find("BtnAssign").GetComponent<Button>();
            BallReadyButton = GameObject.Find("BtnBallReady").GetComponent<Button>();
            FightStartButton = GameObject.Find("BtnFightStart").GetComponent<Button>();
            BallLaunchButton = GameObject.Find("BtnBallLaunch").GetComponent<Button>();
            CheckButton = GameObject.Find("BtnCheck").GetComponent<Button>();
            NextRoundButton = GameObject.Find("BtnNextRound").GetComponent<Button>();
            GameOverButton = GameObject.Find("BtnGameOver").GetComponent<Button>();
            RoomCleanButton = GameObject.Find("BtnRoomClean").GetComponent<Button>();

            AssignButton.onClick.AddListener(AssignFunc);
            BallReadyButton.onClick.AddListener(BallReadyFunc);
            FightStartButton.onClick.AddListener(FightStartFunc);
            BallLaunchButton.onClick.AddListener(BallLaunchFunc);
            CheckButton.onClick.AddListener(CheckFunc);
            NextRoundButton.onClick.AddListener(NextRoundFunc);
            GameOverButton.onClick.AddListener(GameOverFunc);
            RoomCleanButton.onClick.AddListener(RoomCleanFunc);
        }

        //ServerModules 给服务端发送消息(优先级:1)
        void AssignFunc()
        {
            //AssignServer.text = "";
            //匹配成功则返回side
            LoginRequist.ucl.rpcCall("combat.start_match", null, null);
        }

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
        void BallReadyFunc()
        {
            BallRdArr ballArr = new BallRdArr(1,2,3);
            LoginRequist.ucl.rpcCall("play.ball_ready", JsonConvert.SerializeObject(ballArr), null);
        }
        //客户端都准备好后(BallReadFunc)会自动通知回来，不需要客户端触发FightStartFunc
        void FightStartFunc()
        {
           
        }

        public class BallLaunchClass
        {
            public BallLaunchClass(int bi, int si, float fo, float ra)
            {
                ball_id = bi;
                skill_id = si;
                force = fo;
                radian = ra;
            }
            public int ball_id;
            public int skill_id;
            public float force;
            public float radian;
        }
        void BallLaunchFunc()
        {
            BallLaunchClass ballLaunchAttr = new BallLaunchClass(1, -1, 12f,1f);
            LoginRequist.ucl.rpcCall("play.id_ball_launch", JsonConvert.SerializeObject(ballLaunchAttr), null);
        }

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
        void CheckFunc()
        {
            Local Ball_1 = new Local(1f,1f);
            Local Ball_2 = new Local(2f, 2f);
            LocalList localList = new LocalList(Ball_1,Ball_2);
            LoginRequist.ucl.rpcCall("location.get_location", JsonConvert.SerializeObject(localList), null);
        }
        void NextRoundFunc()
        {
            LoginRequist.ucl.rpcCall("play.round_over", null, null);
        }

        public class IsWin
        {
            public IsWin(bool iIsWin)
            {
                Win = iIsWin;
            }
            public bool Win;
        }
        void GameOverFunc()
        {
            IsWin myWin = new IsWin(true);
            LoginRequist.ucl.rpcCall("play.game_over", JsonConvert.SerializeObject(myWin), null);
        }

        //测试房间清理接口，客户端不需要
        void RoomCleanFunc()
        {
            LoginRequist.ucl.rpcCall("play.room_over", null, null);
        }
        //end
    }
}
