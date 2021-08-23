using com.acorn.utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

//==========================
// - FileName: TestAB.cs
// - Created: acorn
// - CreateTime: #CreateTime#
// - Description:
//==========================
public class TestAB : MonoBehaviour
{
    public Button btn1, btn2, btn3, btn4, btn5;
    public Image img;

    string[] strs = new string[] {
        "android.permission.INTERNET",
        "android.permission.READ_PHONE_STATE",
        "android.permission.READ_EXTERNAL_STORAGE",
        "android.permission.WRITE_EXTERNAL_STORAGE",
        "android.permission.ACCESS_WIFI_STATE",
        "android.permission.ACCESS_NETWORK_STATE",
        "android.permission.ACCESS_FINE_LOCATION",
        "android.permission.MOUNT_UNMOUNT_FILESYSTEMS"
    };


    // Start is called before the first frame update
    void Start()
    {
        btn1.onClick.AddListener(() =>
        {
            img.sprite = ABUtils.Instance.LoadResource<Sprite>("UI_Pic1", "testpic", true);
        });
        btn2.onClick.AddListener(() =>
        {
            img.sprite = ABUtils.Instance.LoadResource<Sprite>("UI_Pic2", "testpic", true);
        });
        btn3.onClick.AddListener(() =>
        {
            Permission.RequestUserPermissions(strs);
            StartCoroutine(QAssetBundleManager.DownloadAssetBundles("https://digital-hk.oss-cn-beijing.aliyuncs.com/android/test/", "AssetBundles", (path, name) =>
            {
                print($"下载中{path},{name}");
                StartCoroutine(QAssetBundleManager.DownloadAssetBundleAndSave(path, name, () =>
                {
                    print($"下载完成 manifest:{name}");
                    //img.overrideSprite=
                }));
            }));
        });

        btn4.onClick.AddListener(() =>
        {
            string url = "https://ss0.baidu.com/94o3dSag_xI4khGko9WTAnF6hhy/zhidao/pic/item/8b13632762d0f70379f2e1750cfa513d2797c5d3.jpg";
            StartCoroutine(ImgUtil.LoadImg(url, img));
        });

        btn5.onClick.AddListener(() =>
        {
            string url = "https://digital-hk.oss-cn-beijing.aliyuncs.com/android/test2";
            StartCoroutine(ABUtils.Instance.DownLoadAssetsWithDependencies2Local(url, "Android", "testpic", () =>
            {
                print("都完成了");
            }));
            //print("res:"+ABUtils.Instance.GetStreamingAssetsPath(false));
        });
    }
}
