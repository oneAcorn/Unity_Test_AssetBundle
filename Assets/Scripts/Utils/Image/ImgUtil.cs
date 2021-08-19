using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lru;
using UnityEngine.UI;
using System.IO;

namespace com.acorn.utils
{
    public static class ImgUtil
    {
        private static LRUCache<int, Texture2D> memoryCaches=new LRUCache<int, Texture2D>(20);

        public static IEnumerator LoadImg(string url, Image image)
        {
            int urlHash = url.GetHashCode();
            Texture2D texture2D;
            if (memoryCaches.TryGet(urlHash, out texture2D))
            {
                Debug.Log("内存缓存");
                SetupImage(texture2D, image);
                yield break;
            }
            string filePath = path + url.GetHashCode();
            Debug.Log($"filePath:{filePath}");
            if (File.Exists(filePath))
            {
                Debug.Log("文件缓存");
                yield return LoadLocalImage(url, image);
                yield break;
            }
            Debug.Log("网络下载");
            yield return DownloadImage(url, image);
        }

        static IEnumerator DownloadImage(string url, Image image)
        {
            Debug.Log("downloading new image:" + path + url.GetHashCode());//url转换HD5作为名字
            WWW www = new WWW(url);
            yield return www;

            Texture2D tex2d = www.texture;
            //将图片保存至缓存路径
            byte[] pngData = tex2d.EncodeToPNG();
            File.WriteAllBytes(path + url.GetHashCode(), pngData);

            memoryCaches.Set(url.GetHashCode(), tex2d);
            SetupImage(tex2d, image);
        }

        static IEnumerator LoadLocalImage(string url, Image image)
        {
            string filePath = "file:///" + path + url.GetHashCode();

            Debug.Log("getting local image:" + filePath);
            WWW www = new WWW(filePath);
            yield return www;

            Texture2D texture = www.texture;
            memoryCaches.Set(url.GetHashCode(), texture);
            SetupImage(texture, image);
        }

        private static void SetupImage(Texture2D texture, Image image)
        {
            Sprite m_sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            image.sprite = m_sprite;
            image.SetNativeSize();
        }

        public static string path
        {
            get
            {
                string mPath =
#if UNITY_EDITOR
        Application.temporaryCachePath + "/";
#elif UNITY_ANDROID
        Application.persistentDataPath + "/cache/";
#elif UNITY_IPHONE
        Application.dataPath + "/Raw/";
#else
        string.Empty;
#endif
                return mPath;
            }
        }
    }
}