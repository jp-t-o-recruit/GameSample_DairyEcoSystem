using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

using LocalLogger = MyLogger.MapBy<TitleScene>;

[PageAsset("TitleScenePage.prefab")]
public class TitleScene : SceneBase<TitleScene.CreateParameter>
{
    public class CreateParameter
    {
        string userId;
    }

    Button _nextSceneButton;
    Label _viewLabel;

    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _viewLabel = RootElement.Q<Label>("titleSceneLabel");

        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = "TitleSceneスクリプトから設定！";
    }

    void Update()
    {

    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        LocalLogger.UnloadEnableLogging();
    }

    async void OnButtonClicked()
    {
        _viewLabel.text = "TitleSceneでボタン押した！";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("タイトルでボタン押下");

        var param = new HomeScene.CreateParameter() { ViewLabel = "home をタイトルが指定" };
        var transition = new HomeSceneTransition() { Parameter = param };
        await ExSceneManager.Instance.Replace(transition);
    }
}


public class TitleSceneTransition : SceneTransition<TitleScene.CreateParameter>
{
    public TitleSceneTransition()
    {
        SceneName = "TitleScene";
    }
}
