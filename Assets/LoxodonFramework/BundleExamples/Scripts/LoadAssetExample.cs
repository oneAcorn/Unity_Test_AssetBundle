using System;
using System.Collections;
using UnityEngine;

using Loxodon.Framework.Bundles;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Contexts;
using System.Collections.Generic;

namespace Loxodon.Framework.Examples.Bundle
{

    public class LoadAssetExample : MonoBehaviour
    {
        private IResources resources;
        private bool downloading = false;

        void Start()
        {
            ApplicationContext context = Context.GetApplicationContext();
            this.resources = context.GetService<IResources>();

            this.Load(new string[] { "LoxodonFramework/BundleExamples/Models/Red/Red.prefab", "LoxodonFramework/BundleExamples/Models/Green/Green.prefab" });
            this.StartCoroutine(Load2("LoxodonFramework/BundleExamples/Models/Plane/Plane.prefab"));
        }

        private void OnGUI()
        {
            if (GUILayout.Button("测试下"))
            {
                //StartCoroutine(HaveATest());
                TestLoadByBundleName("Blazer-Sport1", "blazer_sport1");
            }
        }


        private IEnumerator HaveATest()
        {
            yield return Download("blazer_sport1");
            IProgressResult<float, GameObject> result = GetResources().LoadAssetAsync<GameObject>("Res/Clothes/Body/Prefabs/Blazer-Sport1.prefab");

            while (!result.IsDone)
            {
                Debug.LogFormat("Progress:{0}%", result.Progress * 100);
                yield return null;
            }

            try
            {
                if (result.Exception != null)
                    throw result.Exception;

                GameObject.Instantiate(result.Result);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Load failure.Error:{0}", e);
            }
        }

        IEnumerator Download(string bundleName)
        {
            this.downloading = true;
            IDownloader downloader = new UnityWebRequestDownloader(new Uri("https://digital-hk.oss-cn-beijing.aliyuncs.com/AssetBundle/production/windows/"));
            try
            {
                IProgressResult<Progress, BundleManifest> manifestResult = downloader.DownloadManifest("root.dat");

                yield return manifestResult.WaitForDone();

                if (manifestResult.Exception != null)
                {
                    Debug.LogFormat("Downloads BundleManifest failure.Error:{0}", manifestResult.Exception);
                    yield break;
                }

                BundleManifest manifest = manifestResult.Result;
                BundleInfo[] dependencies = manifest.GetDependencies(bundleName, true);
                //foreach(var info in what)
                //{
                //    Debug.Log($"内容:{info.FullName},{info.Name}");
                //}

                BundleInfo bundleInfo = manifest.GetBundleInfo(bundleName);

                List<BundleInfo> bundles = new List<BundleInfo>();
                if (dependencies != null && dependencies.Length > 0)
                {
                    bundles.AddRange(dependencies);
                }
                bundles.Add(bundleInfo);

                if (bundles == null || bundles.Count <= 0)
                {
                    Debug.LogFormat("Please clear cache and remove StreamingAssets,try again.");
                    yield break;
                }

                IProgressResult<Progress, bool> downloadResult = downloader.DownloadBundles(bundles);
                downloadResult.Callbackable().OnProgressCallback(p =>
                {
                    Debug.LogFormat("Downloading {0:F2}KB/{1:F2}KB {2:F3}KB/S", p.GetCompletedSize(UNIT.KB), p.GetTotalSize(UNIT.KB), p.GetSpeed(UNIT.KB));
                });

                yield return downloadResult.WaitForDone();

                if (downloadResult.Exception != null)
                {
                    Debug.LogFormat("Downloads AssetBundle failure.Error:{0}", downloadResult.Exception);
                    yield break;
                }

                Debug.Log("OK");

                //if (this.resources != null)
                //{
                //    BundleResources bundleResources = (this.resources as BundleResources);
                //    bundleResources.BundleManifest = manifest;
                //}
            }
            finally
            {
                this.downloading = false;
            }
        }

        IResources GetResources()
        {
            if (this.resources != null)
                return this.resources;

            /* Create a BundleManifestLoader. */
            IBundleManifestLoader manifestLoader = new BundleManifestLoader();

            Debug.Log($"路径:{BundleUtil.GetStorableDirectory()}");
            /* Loads BundleManifest. */
            BundleManifest manifest = manifestLoader.Load(BundleUtil.GetStorableDirectory() + "root.dat");

            //manifest.ActiveVariants = new string[] { "", "sd" };
            //manifest.ActiveVariants = new string[] { "", "hd" };

            /* Create a PathInfoParser. */
            IPathInfoParser pathInfoParser = new AutoMappingPathInfoParser(manifest);

            /* Use a custom BundleLoaderBuilder */
            ILoaderBuilder builder = new CustomBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false);

            /* Create a BundleManager */
            IBundleManager manager = new BundleManager(manifest, builder);

            /* Create a BundleResources */
            this.resources = new BundleResources(pathInfoParser, manager);
            Debug.Log($"GetResources path1:{BundleUtil.GetStorableDirectory()},path2:{BundleUtil.GetReadOnlyDirectory()}");
            return this.resources;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        void Load(string[] names)
        {
            IProgressResult<float, GameObject[]> result = resources.LoadAssetsAsync<GameObject>(names);
            result.Callbackable().OnProgressCallback(p =>
            {
                Debug.LogFormat("Progress:{0}%", p * 100);
            });
            result.Callbackable().OnCallback((r) =>
            {
                try
                {
                    if (r.Exception != null)
                        throw r.Exception;

                    foreach (GameObject template in r.Result)
                    {
                        GameObject.Instantiate(template);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Load failure.Error:{0}", e);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerator Load2(string name)
        {
            IProgressResult<float, GameObject> result = resources.LoadAssetAsync<GameObject>(name);

            while (!result.IsDone)
            {
                Debug.LogFormat("Progress:{0}%", result.Progress * 100);
                yield return null;
            }

            try
            {
                if (result.Exception != null)
                    throw result.Exception;

                GameObject.Instantiate(result.Result);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Load failure.Error:{0}", e);
            }
        }

        private void TestLoadByBundleName(string bundleName, string bundleGroupName)
        {
            BundleResources _resources = this.resources as BundleResources;
            BundleInfo bundleInfo = _resources.BundleManifest.GetBundleInfo(bundleGroupName);
            Debug.Log($"bundleInfo:{bundleInfo.Name},{bundleInfo.Filename}");
            int length = bundleInfo.Assets.Length;
            string path = null;
            for (int i = 0; i < length; i++)
            {
                string assetPath = bundleInfo.Assets[i];
                int lastIndexOfSlash = assetPath.LastIndexOf('/') + 1;
                int lastIndexOfDot = assetPath.LastIndexOf('.');
                if (lastIndexOfDot == -1)
                {
                    lastIndexOfDot = assetPath.Length;
                }
                string name = assetPath.Substring(lastIndexOfSlash, lastIndexOfDot - lastIndexOfSlash);
                Debug.Log($"path:{assetPath},slash:{lastIndexOfSlash},dot:{lastIndexOfDot},length:{assetPath.Length},name:{name}");
                if (name == bundleName)
                {
                    int indexOfSlash = assetPath.IndexOf('/') + 1;
                    path = assetPath.Substring(indexOfSlash, assetPath.Length - indexOfSlash);
                    break;
                }
            }
            if (path == null)
            {
                Debug.LogError("路径获取失败");
                return;
            }
            Debug.Log($"path:{path}");
            Load(new string[] { path });
        }
    }
}
