using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseFramework
{
    public class DebugLogger
    {
        public static void Debug(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void Debug(string msg, Color color)
        {
            string result = "<color=#" + Utils.ColorToHex(color) + ">" + msg + "</color>";
        }

        public static void DebugWarning(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
        }

        public static void DebugError(string msg)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError(msg);
#else
        if (UnityEngine.Debug.isDebugBuild)
        {
            UnityEngine.Debug.LogError(msg);
        }
        else
        {
            UnityEngine.Debug.LogException(new Exception(msg));
        }
#endif
        }

        public static void DebugNetwork(string msg)
        {
            DebugLogger.Debug(msg, Color.green);
        }

        public static void DebugNetworkError(string msg)
        {
            DebugError("Network Error: " + msg);
        }

        public static void RealtimeSinceStartup(string tag = "")
        {
            UnityEngine.Debug.Log(tag + Time.realtimeSinceStartup);
        }
    }
}
