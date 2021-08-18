using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.IO;

public class QAssetBundleManager
{
    static AssetBundle assetbundle = null;

    static Dictionary<string, AssetBundle> DicAssetBundle = new Dictionary<string, AssetBundle>();

    public static T LoadResource<T>(string assetBundleName, string assetBundleGroupName) where T : UnityEngine.Object
    {
        Debug.Log("LoadResource");
        if (string.IsNullOrEmpty(assetBundleGroupName))
        {
            return default(T);
        }

        if (!DicAssetBundle.TryGetValue(assetBundleGroupName, out assetbundle)) //从内存中获取失败
        {
            //从文件中获取
            assetbundle = AssetBundle.LoadFromFile(Application.persistentDataPath+"/" + assetBundleGroupName);//+ ".assetbundle"
            DicAssetBundle.Add(assetBundleGroupName, assetbundle); //放入内存
        }
        object obj = assetbundle.LoadAsset(assetBundleName, typeof(T));
        var one = obj as T;
        return one;
    }

    public static void UnLoadResource(string assetBundleGroupName)
    {
        if (DicAssetBundle.TryGetValue(assetBundleGroupName, out assetbundle))
        {
            assetbundle.Unload(false);
            if (assetbundle != null)
            {
                assetbundle = null;
            }
            DicAssetBundle.Remove(assetBundleGroupName);
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// 下载根目录AssetBundle文件
    /// </summary>
    /// <returns></returns>
    public static IEnumerator DownloadAssetBundles(string assetBundlePath, string mainAssetBundleName,
        Action<string, string> getAssetsCallback = null)
    {
        Debug.Log("DownloadAssetBundles");
        using (UnityWebRequest www = UnityWebRequest.Get(assetBundlePath + mainAssetBundleName))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("ConnectionError");
                yield return null;
            }

            byte[] datas = www.downloadHandler.data;
            SaveAssetBundle(mainAssetBundleName, datas);
            string localPath = Application.persistentDataPath +"/" + mainAssetBundleName;
            AssetBundle mainAssetBundle = AssetBundle.LoadFromFile(localPath);
            Debug.Log($"maniAsset:{mainAssetBundle}");
            if (mainAssetBundle == null)
                yield return null;
            //获取AssetBundleManifest文件
            AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            Debug.Log($"manifest:{manifest}");
            if (getAssetsCallback != null)
            {
                //获取AssetBundleManifest中的所有AssetBundle的名称信息
                string[] assets = manifest.GetAllAssetBundles();
                for (int i = 0; i < assets.Length; i++)
                {
                    Debug.Log(assetBundlePath + assets[i]);
                    ////开启协程下载所有的
                    //StartCoroutine(DownloadAssetBundleAndSave(assetBundlePath, assets[i], () =>
                    //{
                    //    //下载完成，按照之前的方法，从本地加载AssetBundle并设置。
                    //    Image_BackGround.overrideSprite = QAssetBundleManager.LoadResource<Sprite>("UI_1003", "uibackground");
                    //}));
                    getAssetsCallback(assetBundlePath, assets[i]);
                }
            }
        }
    }

    public static IEnumerator DownloadAssetBundleAndSave(string url, string name, Action saveLocalComplate = null)
    {
        WWW www = new WWW(url + name);
        yield return www;
        if (www.isDone)
        {
            SaveAssetBundle(name, www.bytes, saveLocalComplate);
        }
    }

    public static void SaveAssetBundle(string fileName, byte[] bytes, Action saveLocalComplate = null)
    {
        string dirPath = Application.persistentDataPath;
            Debug.Log($"dir path:{dirPath}");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string path = dirPath + "/" + fileName;
        FileInfo fileInfo = new FileInfo(path);
        FileStream fs = fileInfo.Create();

        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();

        if (saveLocalComplate != null)
        {
            saveLocalComplate();
        }
    }

    public static string GetStreamingAssetsPath()
    {
        string StreamingAssetsPath =
#if UNITY_EDITOR
        Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
        "jar:file://" + Application.dataPath + "!/assets";
#elif UNITY_IPHONE
        Application.dataPath + "/Raw/";
#else
        string.Empty;
#endif
        return StreamingAssetsPath;
    }
}