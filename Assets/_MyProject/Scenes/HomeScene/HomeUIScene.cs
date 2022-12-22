using System;
using System.Linq;
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
public class HomeUIScene : SceneBase, ILayeredSceneUI
{
    public Label _viewLabel;
    public Button _nextSceneButton;
    public Button _toSaveDataBuilderSceneButton;
    public Button _toTitleSceneButton;

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
            UnityEngine.Object.Destroy(rootCanvas.worldCamera.gameObject);
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
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _toSaveDataBuilderSceneButton = RootElement.Q<Button>("toSaveDataBuilderSceneButton");
        _toTitleSceneButton = RootElement.Q<Button>("toTitleSceneButton");

        //_viewLabel.text = ViewLabel. ;
        //_nextSceneButton.clickable.clicked += OnButtonClicked;
        //_toSaveDataBuilderSceneButton.clickable.clicked += OnToSaveDataBuilderSceneButtonClicked;
        //_toTitleSceneButton.clickable.clicked += OnToTitleSceneButtonClicked;

        Logger.SetEnableLogging(true);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // TODO
        _nextSceneButton.text = $"home: {TimeSpan.FromSeconds(DateTime.Now.Second).TotalSeconds % 60}";
    }

    private void OnDestroy()
    {
        //_nextSceneButton.clickable.clicked -= OnButtonClicked;
        Logger.UnloadEnableLogging();
    }

    void OnButtonClicked()
    {
        Logger.Debug($"ホームで次シーンボタンがクリック IsInputLock:{IsInputLock}");

        if (IsInputLock) return;
        IsInputLock = true;

        _viewLabel.text = "HomeUISceneボタンから設定！";
        //_nextSceneButton.pickingMode = PickingMode.Ignore;
        Logger.Debug("ホームでボタン押下");
        IsInputLock = false;
    }
    

    private void OnToSaveDataBuilderSceneButtonClicked()
    {
        if (IsInputLock) return;
        IsInputLock = true;
        IsInputLock = false;
    }

    private void OnToTitleSceneButtonClicked()
    {
        if (IsInputLock)　return;
        IsInputLock = true;
        IsInputLock = false;
    }
}
