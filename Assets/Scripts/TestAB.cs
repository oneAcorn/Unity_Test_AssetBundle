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
    public Button btn1, btn2, btn3, btn4;
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
            img.sprite = QAssetBundleManager.LoadResource<Sprite>("UI_Pic1", "testpic");
        });
        btn2.onClick.AddListener(() =>
        {
            img.sprite = QAssetBundleManager.LoadResource<Sprite>("UI_Pic2", "testpic");
        });
        btn3.onClick.AddListener(() =>
        {
            Permission.RequestUserPermissions(strs);
            StartCoroutine(QAssetBundleManager.DownloadAssetBundles("https://digital-hk.oss-cn-beijing.aliyuncs.com/android/test/", "AssetBundles", (path, name) =>
            {
                StartCoroutine(QAssetBundleManager.DownloadAssetBundleAndSave(path, name, () =>
                {
                    print($"ÏÂÔØÍê³É manifest:{name}");
                    //img.overrideSprite=
                }));
            }));
        });

        btn4.onClick.AddListener(() =>
        {

        });
    }
}
