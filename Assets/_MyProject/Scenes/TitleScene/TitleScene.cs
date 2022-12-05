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
        _viewLabel.text = "TitleScene�X�N���v�g����ݒ�I";
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
        _viewLabel.text = "TitleScene�Ń{�^���������I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("�^�C�g���Ń{�^������");

        var param = new HomeScene.CreateParameter() { ViewLabel = "home ���^�C�g�����w��" };
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
