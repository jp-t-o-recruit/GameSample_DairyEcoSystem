using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// タイトルシーン通常シナリオ
/// </summary>
public class TitleScenario : IScenario
{
    public IScenarioParams Params { get; set; }
    private Func<TitleScenario, UniTask> _completedAction;


    public TitleScenario(string id = "title",
                         Func<TitleScenario, UniTask> callback = default)
    {
        Params = new DummyScenarioParams()
        {
            ID = id
        };
        _completedAction = callback ?? ((scenario) => UniTask.Delay(0));
    }

    public async void OnActive()
    {
        // 通常のタイトルシーンを実行
        var domain = new TitleSceneDomain();
        System.Threading.CancellationTokenSource cts = new();
        await domain.SceneTransition(cts);
        Initialize(domain._uiLayer);
    }

    public void Initialize(TitleUIScene uiScene)
    {
        uiScene._nextSceneButton.clickable.clicked += OnClicked;
    }

    public void OnClicked()
    {
        // Homeシーンに移行する
        // Homeシーンのシナリオを呼び出すstring id,
        Func<HomeScenario, UniTask> callback = null;
        var s =new HomeScenario("TODO123", callback, new HomeScenarioState());
        ScenarioContainer.SetActive(s, ChinType.NotChain);
    }

    /// <summary>
    /// シナリオの完了
    /// 次シナリオを呼び出す
    /// </summary>
    private async UniTask Complete()
    {
        await _completedAction(this);
    }
}