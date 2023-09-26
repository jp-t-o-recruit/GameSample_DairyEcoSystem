using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using My;

using Logger = MyLogger.MapBy<HomeUIScene>;

/// <summary>
/// 
/// 
/// 参考UIシーン構成
/// https://engineering.enish.jp/?p=1115
/// </summary>
public class HomeUIScene : SceneBase, ILayeredSceneUI
{
    public Label _viewLabel;
    public Button _nextSceneButton;
    public Button _toSaveDataBuilderSceneButton;
    public Button _toTitleSceneButton;

    public UnderCommonMenu _underCommonMenu;

    void Awake()
    {
        // 親シーン(Root)のルートキャンバスを取得する
        var rootCanvasParentScene = SceneManager.GetSceneByName($"{typeof(HomeScene)}").GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // 自身のシーン(Additive)のルートキャンバスを取得する
        var mySceneCanvas = SceneManager.GetSceneByName($"{typeof(HomeUIScene)}").GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // 自身のシーン(Additive)のルートキャンバスのUIカメラを削除する
        if (mySceneCanvas.worldCamera != null)
        {
            UnityEngine.Object.Destroy(mySceneCanvas.worldCamera.gameObject);
            mySceneCanvas.worldCamera = null;
        }


        // 自身のシーン(Additive)のルートキャンバスのUIカメラを親シーン(Root)のカメラに置き換える
        mySceneCanvas.worldCamera = rootCanvasParentScene.worldCamera;
    }

    /// <summary>
    /// </summary>
    void OnEnable()
    {
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _toSaveDataBuilderSceneButton = RootElement.Q<Button>("toSaveDataBuilderSceneButton");
        _toTitleSceneButton = RootElement.Q<Button>("toTitleSceneButton");
        _underCommonMenu = UnderCommonMenu.Q(RootElement);

        //RootElement.transform.scale = Vector2.zero;
    }
    /// Start is called before the first frame update
    void Start()
    {
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // TODO
        _nextSceneButton.text = $"home: {TimeSpan.FromSeconds(DateTime.Now.Second).TotalSeconds % 60}";
    }

    void OnDisable()
    {
    }

    private void OnDestroy()
    {
        _underCommonMenu = null;
        Logger.UnloadEnableLogging();
    }
}
