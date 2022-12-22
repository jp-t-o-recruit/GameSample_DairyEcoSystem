using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;
using LocalLogger = MyLogger.MapBy<TitleUIScene>;

/// <summary>
/// タイトルシーンの内UIレイヤーシーン
/// 
/// クラス名はシーンオブジェクトと同一名にする
/// 
/// 参考UIシーン構成
/// https://engineering.enish.jp/?p=1115
/// </summary>
public class TitleUIScene : SceneBase, ILayeredSceneUI
{
    public Button _nextSceneButton;
    public Button _creditNotationButton;
    public Label _viewLabel;

    [Inject]
    public void Construct()
    {
    }

    void Awake()
    {
        // 親シーン(Root)のルートキャンバスを取得する
        var rootCanvasParentScene = SceneManager.GetSceneByName($"{typeof(TitleScene)}").GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // 自身のシーン(Additive)のルートキャンバスを取得する
        var mySceneCanvas = SceneManager.GetSceneByName($"{typeof(TitleUIScene)}").GetRootGameObjects()
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

    // Start is called before the first frame update
    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _creditNotationButton = RootElement.Q<Button>("toCreditButton");
        _viewLabel = RootElement.Q<Label>("titleSceneLabel");
    }

    // Update is called once per frame
    void Update()
    {
        _viewLabel.text = TitleSceneParamsSO.Entity.ViewLabel.ToString();
        //_nextSceneButton.text = $"title: {TimeSpan.FromSeconds(DateTime.Now.Second).TotalSeconds % 60}";
    }

    private void OnDestroy()
    {
        LocalLogger.UnloadEnableLogging();
    }
}
