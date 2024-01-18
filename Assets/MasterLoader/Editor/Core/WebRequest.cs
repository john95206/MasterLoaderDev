using System.Security.Policy;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MasterLoader.Core
{
    public class WebRequest
    {
        static WebRequest() { }

        public static string SendWebRequest(string url, string progressMessage)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    var req = request.SendWebRequest();

                    while (!req.isDone)
                    {
                        EditorUtility.DisplayProgressBar(progressMessage, $"{request.downloadProgress * 100}%", request.downloadProgress);
                    }
#if UNITY_2020_1_OR_NEWER
                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (request.isHttpError || request.isNetworkError)
#endif
                    {
                        Debug.LogError("MasterLoader Info: Request has failed.");
                        throw new Exception(request.error);
                    }

                    var json = request.downloadHandler.text;
                    Debug.Log(json);
                    if (json.Contains("<!DOCTYPE html>"))
                    {
                        throw new Exception(json);
                    }
                    return json;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("MasterLoader Info: Request has failed.");
                Debug.LogException(e);
                return string.Empty;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
