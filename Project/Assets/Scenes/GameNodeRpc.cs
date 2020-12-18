using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace BaseFramework.Network
{
    public class GameNodeRpc : MonoBehaviour
    {
        Button SerializeButton;
        Text SerializeButtonText;
        Button NotificationButton;
        Text NotificationButtonText;
        internal static Text SerializedPlayername;
        internal static Text SerializedUid;
        internal static Text NotificationMsg;
        internal static Text TextInfo;


        Button MatchButton;
        internal static Text MatchButtonText;

        Button CancleButton;
        Text CancleButtonText;

        internal static bool JumpFlag = false;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            SerializeButton = GameObject.Find("SerilizeRpc").GetComponent<Button>();
            SerializeButtonText = SerializeButton.transform.Find("Text").GetComponent<Text>();

            NotificationButton = GameObject.Find("NotificationRpc").GetComponent<Button>();
            NotificationButtonText = NotificationButton.transform.Find("Text").GetComponent<Text>();

            SerializedPlayername = GameObject.Find("playername").GetComponent<Text>();
            SerializedUid = GameObject.Find("uid").GetComponent<Text>();
            NotificationMsg = GameObject.Find("NoficationMsg").GetComponent<Text>();


            SerializeButtonText.text = "序列化";
            NotificationButtonText.text = "Notify";

            SerializedPlayername.color = Color.white;
            SerializedPlayername.text = "用户名";
            SerializedUid.color = Color.white;
            SerializedUid.text = "UID";
            NotificationMsg.color = Color.white;
            NotificationMsg.text = "Notify 消息";

            SerializeButton.onClick.AddListener(SerializeRpc);
            NotificationButton.onClick.AddListener(StartNotify);


            MatchButton = GameObject.Find("Match").GetComponent<Button>();
            MatchButtonText = MatchButton.transform.Find("Text").GetComponent<Text>();
            MatchButtonText.text = "开始匹配";
            MatchButton.onClick.AddListener(MatchStart);

            CancleButton = GameObject.Find("Cancle").GetComponent<Button>();
            CancleButtonText = CancleButton.transform.Find("Text").GetComponent<Text>();
            CancleButtonText.text = "取消匹配";
            CancleButton.onClick.AddListener(MatchCancle);

            TextInfo = GameObject.Find("TextSet").GetComponent<Text>();
            TextInfo.text = "";

        }

        void MatchStart()
        {
            LoginRequist.ucl.rpcCall("combat.start_match", null, null);
        }

        void MatchCancle()
        {
            LoginRequist.ucl.rpcCall("combat.cancel_match", null, null);
        }

        void SerializeRpc()
        {
            RpcCallBackDelegate callback = new RpcCallBackDelegate(LoginRequist.ucl.SerializeRpcCallback);
            LoginRequist.ucl.rpcCall("user.serialize", null, callback);
        }

        void StartNotify()
        {
            People people = new People();
            people.name = "11";
            people.id = 3;
            string tt = JsonConvert.SerializeObject(people);
            object c = JsonConvert.DeserializeObject(tt);
            People people1 = c as People;
            Debug.Log(c.ToString());
            Debug.Log(people1.name);
            Debug.Log(JsonConvert.SerializeObject(people));
            //object转换成为想要的类型
            //People smsrespon = (People)JsonConvert.DeserializeObject(rr, typeof(People));
            //LoginRequist.ucl.rpcCall("notifytester.rpc_start_notify", JsonConvert.SerializeObject(people), null);
            LoginRequist.ucl.rpcCall("notifytester.rpc_start_notify", "3", null);
        }

        public void switchScene(string sceneName)
        {
            StartCoroutine(Load(sceneName));
        }

        private IEnumerator Load(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            DebugLogger.Debug(op.ToString());
            yield return new WaitForEndOfFrame();
            op.allowSceneActivation = true;
        }
    }
    public class People
    {
        public string name;
        public int id;
    }

}
