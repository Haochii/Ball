using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BaseFramework.Network
{

    public class LoginRequist : MonoBehaviour
    {
        private InputField LoginInputAccount;
        //private readonly string ServerIP = "47.114.188.59";
        private readonly string ServerIP = "121.196.14.73";
        private readonly int ServerPort = 7002;
        Button LoginButton;
        Text LoginButtonText;
        internal static UserClient ucl;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            // init basic UI and network status
            LoginButton = GameObject.Find("LoginBtn").GetComponent<Button>();
            LoginButtonText = LoginButton.transform.Find("Text").GetComponent<Text>();
            LoginInputAccount = GameObject.Find("AccountInputField").GetComponent<InputField>();
            LoginButtonText.text = "登录";
            LoginInputAccount.placeholder.GetComponent<Text>().text = "输入账号";
            NetConfigDict.Init();

            // active UI event
            LoginButton.onClick.AddListener(startLoginProcess);
            LoginInputAccount.ActivateInputField();
        }

        void startLoginProcess()
        {
            DebugLogger.Debug("start login process " + LoginInputAccount.text, Color.blue);
            ucl = NetClient.GetInstance("logic");
            if (ucl.Login(ServerIP, ServerPort, LoginInputAccount.text))
            {
                switchScene("ServerScene");
                //LoginRequist.ucl.rpcCall("notifytester.rpc_start_notify", "1", null);
            }
            else
            {
                DebugLogger.Debug("Login Error");
            }
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

        private void OnDestroy()
        {
            //ucl.Close();
            //ucl.Dispose();
        }
    }
}