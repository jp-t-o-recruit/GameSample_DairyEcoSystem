using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


using Logger = MyLogger.MapBy<HomeUIScene>;

/// <summary>
/// 
/// 
/// 参考UIシーン構成
/// https://engineering.enish.jp/?p=1115
/// </summary>
public class HomeUIScene : SceneBase
{
    public class CreateParameter
    {
        public string ViewLabel = "HomeUISceneデフォルトのラベル";
    }

    Button _nextSceneButton;
    Label _viewLabel;

    void Awake()
    {
        // 親シーン(Root)のルートキャンバスを取得する
        var rootCanvasParentScene = SceneManager.GetSceneByName(typeof(HomeScene).ToString()).GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // 自身のシーン(Additive)のルートキャンバスを取得する
        var rootCanvas = GetComponent<Canvas>();

        // 自身のシーン(Additive)のルートキャンバスのUIカメラを削除する
        if (rootCanvas.worldCamera != null)
        {
            Object.Destroy(rootCanvas.worldCamera.gameObject);
            rootCanvas.worldCamera = null;
        }


        // 自身のシーン(Additive)のルートキャンバスのUIカメラを親シーン(Root)のカメラに置き換える
        rootCanvas.worldCamera = rootCanvasParentScene.worldCamera;
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");

        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = new CreateParameter().ViewLabel;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        Logger.UnloadEnableLogging();
    }

    async void OnButtonClicked()
    {
        _viewLabel.text = "HomeUISceneボタンから設定！";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        Logger.SetEnableLogging(false);
        Logger.Debug("ホームでボタン押下");

        TitleSceneTransition transition = new ();
        await ExSceneManager.Instance.Replace(transition);
        ////ロード済みのシーンであれば、名前で別シーンを取得できる
        //Scene scene = SceneManager.GetSceneByName("ManagerScene");

        //PageManager pageManager;

        ////GetRootGameObjectsで、そのシーンのルートGameObjects
        ////つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        //foreach (var rootGameObject in scene.GetRootGameObjects())
        //{
        //    pageManager = rootGameObject.GetComponent<PageManager>();
        //    if (pageManager != null)
        //    {
        //        TitleScene.Transition transition = new TitleScene.Transition() { Parameter = { viewStr ="HomeUISceneで設定！！！" }};
        //        await pageManager.Replace(transition);
        //        break;
        //    }
        //}
    }
}
