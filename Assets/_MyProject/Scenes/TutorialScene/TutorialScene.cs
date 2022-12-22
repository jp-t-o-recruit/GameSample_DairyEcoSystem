using UnityEngine;
using UnityEngine.UIElements;
using Logger = MyLogger.MapBy<TutorialScene>;


[RequireComponent(typeof(UIDocument))]
public class TutorialScene : SceneBase, ILayeredSceneLogic
{
    public Button _skipButton;

    void Start()
    {
        // TODO ドメインは親レイヤーなので上から取り付ける形
        //_domein = new TutorialSceneDomain();
        Logger.SetEnableLogging(true);
        Logger.Debug("ドキュメントある？" + (null != _uiDocument));

        _skipButton = RootElement.Q<Button>("skipButton");
        _skipButton.clickable.clicked += OnSkipClick;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _skipButton.clickable.clicked -= OnSkipClick;
        Logger.UnloadEnableLogging();
        //_domein = null;
    }

    private async void OnSkipClick()
    {
        _skipButton.pickingMode = PickingMode.Ignore;
        Logger.Debug("スキップボタン押下");
        
        _skipButton.pickingMode = PickingMode.Position;
    }
}
