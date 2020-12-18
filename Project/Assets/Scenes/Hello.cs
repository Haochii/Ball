using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace BaseFramework.Network
{
    public class Hello : MonoBehaviour
    {
        Button HelloButton;
        Text HelloButtonText;
        internal static Text HelloText;

        private void OnEnable()
        {
            HelloButton = GameObject.Find("Hello").GetComponent<Button>();
            HelloButtonText = HelloButton.transform.Find("Text").GetComponent<Text>();

            HelloText = GameObject.Find("HelloText").GetComponent<Text>();


            HelloButtonText.text = "hello";

            HelloText.color = Color.white;
            HelloText.text = "Hello world";

            HelloButton.onClick.AddListener(HelloRpc);
        }

        void HelloRpc()
        {
            RpcCallBackDelegate callback = new RpcCallBackDelegate(LoginRequist.ucl.SerializeRpcCallback);
            LoginRequist.ucl.rpcCall("user.serialize", null, callback);
        }

    }
}
