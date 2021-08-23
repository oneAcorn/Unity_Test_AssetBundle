using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 
/// </summary>
namespace com.acorn.utils
{
    public class ABUtils : SingletonManager<ABUtils>
    {
        public T LoadResource<T>(string assetBundleName, string assetBundleGroupName,bool isDownloadPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetBundleGroupName))
            {
                return default(T);
            }

            //从文件中获取
            AssetBundle assetbundle = AssetBundle.LoadFromFile(GetStreamingAssetsPath(isDownloadPath) + "/" + assetBundleGroupName);
            object obj = assetbundle.LoadAsset(assetBundleName, typeof(T));
            var one = obj as T;
            assetbundle.Unload(false);
            return one;
        }

        /// <summary>
        ///   //从服务器下载到本地
        /// </summary>
        /// <param name="AssetsHost">服务器路径</param>
        /// <param name="RootAssetsName">总依赖文件目录路径</param>
        /// <param name="AssetName">请求资源名称</param>
        /// <param name="saveLocalPath">保存到本地路径,一般存在Application.persistentDataPath</param>
        /// <returns></returns>
        public IEnumerator DownLoadAssetsWithDependencies2Local(string AssetsHost, string RootAssetsName, string AssetName, OnDownloadFinish OnDownloadOver = null)
        {
            WWW ServerManifestWWW = null;        //用于存储依赖关系的 AssetBundle
            AssetBundle LocalManifestAssetBundle = null;    //用于存储依赖关系的 AssetBundle
            AssetBundleManifest assetBundleManifestServer = null;  //服务器 总的依赖关系    
            AssetBundleManifest assetBundleManifestLocal = null;   //本地 总的依赖关系

            if (RootAssetsName != "")    //总依赖项为空的时候去加载总依赖项
            {
                ServerManifestWWW = new WWW(AssetsHost + "/" + RootAssetsName);

                Debug.Log("___当前请求总依赖文件~\n");

                yield return ServerManifestWWW;
                if (ServerManifestWWW.isDone)
                {
                    //加载总的配置文件
                    assetBundleManifestServer = ServerManifestWWW.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    Debug.Log("___当前请求总依赖文件~\n");
                }
                else
                {
                    throw new Exception("总依赖文件下载失败~~~\n");
                }
            }

            //获取需要加载物体的所有依赖项
            string[] AllDependencies = new string[0];
            if (assetBundleManifestServer != null)
            {
                //根据名称获取依赖项
                AllDependencies = assetBundleManifestServer.GetAllDependencies(AssetName);
            }

            //下载队列 并获取每个资源的Hash值
            Dictionary<string, Hash128> dicDownloadInfos = new Dictionary<string, Hash128>();
            for (int i = AllDependencies.Length - 1; i >= 0; i--)
            {
                dicDownloadInfos.Add(AllDependencies[i], assetBundleManifestServer.GetAssetBundleHash(AllDependencies[i]));
            }
            dicDownloadInfos.Add(AssetName, assetBundleManifestServer.GetAssetBundleHash(AssetName));
            if (assetBundleManifestServer != null)   //依赖文件不为空的话下载依赖文件
            {
                Debug.Log("Hash:" + assetBundleManifestServer.GetHashCode());
                dicDownloadInfos.Add(RootAssetsName, new Hash128(0, 0, 0, 0));
            }

            //卸载掉,无法同时加载多个配置文件
            ServerManifestWWW.assetBundle.Unload(true);

            string saveLocalPath = GetStreamingAssetsPath(true);
            if (File.Exists(saveLocalPath + "/" + RootAssetsName))
            {
                LocalManifestAssetBundle = AssetBundle.LoadFromFile(saveLocalPath + "/" + RootAssetsName);
                assetBundleManifestLocal = LocalManifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }

            foreach (var item in dicDownloadInfos)
            {
                if (!CheckLocalFileNeedUpdate(item.Key, item.Value, RootAssetsName, saveLocalPath, assetBundleManifestLocal))
                {
                    Debug.Log("无需下载:" + item.Key);
                    continue;
                }
                else
                {
                    DeleteFile(saveLocalPath + "/" + item.Key);
                }

                //直接加载所有的依赖项就好了
                WWW wwwAsset = new WWW(AssetsHost + "/" + item.Key);
                //获取加载进度
                while (!wwwAsset.isDone)
                {
                    Debug.Log(string.Format("下载 {0} : {1:N1}%", item.Key, (wwwAsset.progress * 100)));
                    //yield return new WaitForSeconds(0.2f);
                }
                //保存到本地
                SaveAsset2LocalFile(saveLocalPath, item.Key, wwwAsset.bytes, wwwAsset.bytes.Length);

            }

            if (LocalManifestAssetBundle != null)
            {
                LocalManifestAssetBundle.Unload(true);
            }

            if (OnDownloadOver != null)
            {
                OnDownloadOver();
            }
        }

        /// <summary>
        /// 检测本地文件是否存在已经是否是最新
        /// </summary>
        /// <param name="AssetName"></param>
        /// <param name="RootAssetsName"></param>
        /// <param name="localPath"></param>
        /// <param name="serverAssetManifestfest"></param>
        /// <param name="CheckCount"></param>
        /// <returns></returns>
        bool CheckLocalFileNeedUpdate(string AssetName, Hash128 hash128Server, string RootAssetsName, string localPath, AssetBundleManifest assetBundleManifestLocal)
        {
            Hash128 hash128Local;
            bool isNeedUpdate = false;
            if (!File.Exists(localPath + "/" + AssetName))
            {
                return true;   //本地不存在,则一定更新
            }

            if (!File.Exists(localPath + "/" + RootAssetsName))   //当本地依赖信息不存在时,更新
            {
                isNeedUpdate = true;
            }
            else   //总的依赖信息存在切文件已存在  对比本地和服务器两个文件的Hash值
            {
                if (hash128Server == new Hash128(0, 0, 0, 0))
                {
                    return true;  //保证每次都下载总依赖文件
                }
                hash128Local = assetBundleManifestLocal.GetAssetBundleHash(AssetName);
                //对比本地与服务器上的AssetBundleHash  版本不一致就下载
                if (hash128Local != hash128Server)
                {
                    isNeedUpdate = true;
                }
            }
            return isNeedUpdate;
        }

        /// <summary>
        /// 将文件模型创建到本地
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="length"></param>
        void SaveAsset2LocalFile(string path, string name, byte[] info, int length)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            Stream sw = null;
            FileInfo fileInfo = new FileInfo(path + "/" + name);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            //如果此文件不存在则创建
            sw = fileInfo.Create();
            //写入
            sw.Write(info, 0, length);

            sw.Flush();
            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                Debug.Log("Catch exception At DeleteFile");
            }
        }

        public string GetStreamingAssetsPath(bool isDownloadPath)
        {
            string res;
#if UNITY_EDITOR
            res = Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
            if (isDownloadPath)
            {
                res = Application.persistentDataPath + "/AssetBundle";
            }
            else
            {
                res = "jar:file://" + Application.dataPath + "!/assets";
            }
#elif UNITY_IPHONE
            res = Application.dataPath + "/Raw/";
#else
            res = string.Empty;
#endif
            return res;
        }

    }

    public delegate void OnDownloadFinish();
}