using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Manager;
using System.Text;
using System;
namespace APIReq
{
    [System.Serializable]
    public class CodeInfo
    {
        public string code;
    }
    [System.Serializable]
    public class AuthInfo
    {
        public bool success;
    }
    [System.Serializable]
    public class AudioInfo
    {
        public string audio;
        public string id;
        public string code;
        public AudioInfo(string code, string id, byte[] audio){
            this.code = code;
            this.audio = Convert.ToBase64String(audio);
            this.id = id;
        }
    }
    [System.Serializable]
    public class APIReqs : MonoBehaviour
    {
        static string baseUrl = "http://206.189.188.227:5000/";

        
        public static IEnumerator GetCode(Text codeText)
        {
            string uri = APIReqs.baseUrl+"instance";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)){
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        CodeInfo codeInfo = JsonUtility.FromJson<CodeInfo>(webRequest.downloadHandler.text);

                        codeText.text = codeInfo.code; 
                        Manager.DataManager.Instance.code = codeInfo;
                        break;
                }
            }

        }
        public static IEnumerator PollAuth(){
            if(Manager.DataManager.Instance.authenticated||Manager.DataManager.Instance.code==null){
                yield break;
            }
            


            string uri = APIReqs.baseUrl+"check_authenticated";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(Manager.DataManager.Instance.code));
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);

                        Manager.DataManager.Instance.authenticated = authInfo.success;
                        break;
                }
            }


        }
        public static IEnumerator SendAudio(byte[] data){
            Debug.Log("sendAudio");
            string uri = APIReqs.baseUrl+"/answer_audio";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new AudioInfo(Manager.DataManager.Instance.code.code,"1",data)));
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        AuthInfo authInfo = JsonUtility.FromJson<AuthInfo>(webRequest.downloadHandler.text);

                        break;
                }

                        
            }

        }



        // Update is called once per frame
        void Update()
        {
            
        }
    }
    
}