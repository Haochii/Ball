using System.Collections;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BaseFramework
{
    public static class Utils
    {
        static readonly string s_RandomURLheader = "?nocache=";

        public static string GetRandomURL(string origin)
        {
            return origin + s_RandomURLheader + UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        public static string URLCombine(string first, params string[] others)
        {
            StringBuilder sb = new StringBuilder(first);
            foreach (var param in others)
            {
                sb.Append('/');
                sb.Append(param);
            }

            return sb.ToString();
        }

        public static string GetFileHash(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    using (MD5 md5 = new MD5CryptoServiceProvider())
                    {
                        byte[] result = md5.ComputeHash(fs);
                        StringBuilder sb = new StringBuilder();
                        foreach (byte b in result)
                        {
                            sb.Append(b.ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Can not Get File Hash " + e.Message + e.StackTrace);
                return string.Empty;
            }
        }

        //public static async Task<byte[]> ReadStreamingAsset(string path)
        //{
        //    var url = Path.Combine(Application.streamingAssetsPath, path);
        //    UnityWebRequest www = UnityWebRequest.Get(url);
        //    await www.SendWebRequest();
        //    byte[] b = null;
        //    if (!string.IsNullOrEmpty(www.error))
        //    {
        //        Debug.LogError(string.Format("ReadStreamingAsset Error! {0}", www.error));
        //    }
        //    else
        //    {
        //        b = www.downloadHandler.data;
        //    }
        //    www.Dispose();
        //    return b;
        //}

        public static void RecursivelyCreateDirectory(string fullDirectoryPath)
        {
            if (Directory.Exists(fullDirectoryPath))
                return;
            string text = string.Empty;
            string text2 = fullDirectoryPath;
            List<string> list = new List<string>();
            while (!Directory.Exists(text2))
            {
                if (text2.LastIndexOf(Path.DirectorySeparatorChar) == text2.Length - 1)
                {
                    text2 = text2.Substring(0, text2.Length - 1);
                }
                int num = text2.LastIndexOf(Path.DirectorySeparatorChar);
                text = text2.Substring(num + 1, text2.Length - (num + 1));
                list.Add(text);
                text2 = text2.Substring(0, num);
            }
            while (list.Count > 0)
            {
                int num = list.Count - 1;
                text = list[num];
                list.RemoveAt(num);
                text2 = text2 + Path.DirectorySeparatorChar + text;
                Directory.CreateDirectory(text2);
            }
        }

        public static bool SaveToFile(string filePath, byte[] byteArray)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                RecursivelyCreateDirectory(dirPath);
            }

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(byteArray, 0, byteArray.Length);
                };
                return true;
            }
            catch (System.Exception e)
            {
                DebugLogger.DebugError("SaveToFile Failed " + e.ToString());
            }

            return false;
        }

        public static byte[] EncryptStringToBytesAes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            if (Key.Length != 32)
            {
                byte[] fullKey = new byte[32];
                Array.Copy(Key, fullKey, Key.Length > 32 ? 32 : Key.Length);
                Key = fullKey;
            }

            if (IV.Length != 16)
            {
                byte[] fullIV = new byte[16];
                Array.Copy(IV, fullIV, IV.Length > 16 ? 16 : IV.Length);
                IV = fullIV;
            }

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static string DecryptStringFromBytesAes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            if (Key.Length != 32)
            {
                byte[] fullKey = new byte[32];
                Array.Copy(Key, fullKey, Key.Length > 32 ? 32 : Key.Length);
                Key = fullKey;
            }

            if (IV.Length != 16)
            {
                byte[] fullIV = new byte[16];
                Array.Copy(IV, fullIV, IV.Length > 16 ? 16 : IV.Length);
                IV = fullIV;
            }

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        //-----20/08/03
      
        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        public static bool CompareVersion(string version1, string version2)
        {
            if (version2 == "") return true;

            Version v1 = new Version(version1);
            Version v2 = new Version(version2);
            return v1 > v2;
        }

        public static Coroutine DelayCallFunction(MonoBehaviour com, System.Action function, float time)
        {
            return com.StartCoroutine(OnDelayCallFunction(function, time));
        }

        static Dictionary<float, WaitForSeconds> waitDic = new Dictionary<float, WaitForSeconds>();
        static IEnumerator OnDelayCallFunction(System.Action function, float time)
        {
            WaitForSeconds ws;
            if (waitDic.ContainsKey(time))
            {
                ws = waitDic[time];
            }
            else
            {
                ws = new WaitForSeconds(time);
                waitDic[time] = ws;
            }
            yield return ws;
            function?.Invoke();
        }

        /// <summary>
        /// 获取手机设备号
        /// </summary>
        public static string GetDeviceUniqueIdentifier()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static string GetDeviceID()
        {
#if UNITY_IOS
        return UnityEngine.iOS.Device.advertisingIdentifier;
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }


        //-----20/08/04

        /// <summary>
        /// 判断物体是否为空，主要给Lua调用
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsObjectNull(UnityEngine.Object o)
        {
            return o == null;
        }


        #region PlayerPrefs

        public static int GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static bool HasString(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void SetInt(string key, int value = default(int))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetInt(key, value);
        }

        public static string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public static void SetString(string key, string value = default(string))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.SetString(key, value);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public static void RemoveKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public static bool CheckKeyAccord2User(string key)
        {
            string k = GetKeyAccord2User(key);
            return PlayerPrefs.HasKey(k);
        }

        public static string GetKeyAccord2User(string key)
        {
            throw new Exception("Abandon");
            //sTmpStringBuidler.Length = 0;
            //sTmpStringBuidler.AppendFormat("{0}@{1}", key, NetworkingUserToken.uid);
            //return sTmpStringBuidler.ToString();
        }

        public static void SetIntAccord2User(string key, int value)
        {
            string k = GetKeyAccord2User(key);
            PlayerPrefs.DeleteKey(k);
            PlayerPrefs.SetInt(k, value);
        }

        public static int GetIntAccord2User(string key, int value = default(int))
        {
            return PlayerPrefs.GetInt(GetKeyAccord2User(key), value);
        }

        public static int GetInt4User(string key)
        {
            return PlayerPrefs.GetInt(key);
        }

        public static void SetStringAccord2User(string key, string value)
        {
            string k = GetKeyAccord2User(key);
            PlayerPrefs.DeleteKey(k);
            PlayerPrefs.SetString(k, value);
        }

        public static string GetStringAccord2User(string key, string value = default(string))
        {
            return PlayerPrefs.GetString(GetKeyAccord2User(key), value);
        }

        public static void RemoveKeyAccord2User(string key)
        {
            string k = GetKeyAccord2User(key);
            PlayerPrefs.DeleteKey(k);
        }

        #endregion
    }
}
