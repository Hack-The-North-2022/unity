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
    public class QuestionInfos
    {
        public List<QuestionInfo> questions;
    }
    [System.Serializable]
    public class QuestionInfo
    {
        public string text;
        public string audio;
        public string question_id;
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
                /* Debug.Log(JsonUtility.ToJson(new AudioInfo */
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

        public static IEnumerator QuestionAudio(){
            string uri = APIReqs.baseUrl+"/question_audio";
            using (UnityWebRequest webRequest = new UnityWebRequest(uri,"POST")){
                webRequest.SetRequestHeader("Content-Type","application/json");
                webRequest.uploadHandler = new UploadHandlerRaw(new byte[10]);
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
                        QuestionInfos questions = JsonUtility.FromJson<QuestionInfos>(webRequest.downloadHandler.text);

                        Manager.DataManager.Instance.questions = questions.questions;
                        
                         


                        break;
                }

                        
            }


        }
        public static AudioClip RetrieveAudio(APIReq.QuestionInfo q){
            byte[] receivedBytes = Convert.FromBase64String(q.audio);
            float[] samples = new float[receivedBytes.Length / 4]; //size of a float is 4 bytes
            Buffer.BlockCopy(receivedBytes, 0, samples, 0, receivedBytes.Length);
            int channels = 1; //Assuming audio is mono because microphone input usually is
            int sampleRate = 44100; //Assuming your samplerate is 44100 or change to 48000 o
            AudioClip clip = AudioClip.Create("ClipName", samples.Length, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;

        }



        // Update is called once per frame
        void Update()
        {
            
        }
    }
    
}
