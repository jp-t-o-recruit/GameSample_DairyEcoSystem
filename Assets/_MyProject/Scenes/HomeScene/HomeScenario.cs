using Cysharp.Threading.Tasks;
using System;
using VContainer;
using Logger = MyLogger.MapBy<HomeScenario>;

public class HomeScenarioState
{

}
/// <summary>
/// ホームシナリオ
/// </summary>
public class HomeScenario : IScenario, IDisposable
{
    public IScenarioParams Params { get; set; }

    private Func<HomeScenario, UniTask> _completedAction;

    HomeUIScene UIScene;

    System.Threading.CancellationTokenSource _cts;
    public HomeScenarioState State { get; set; }

    [Inject]
    public HomeScenario(string id,
                        Func<HomeScenario, UniTask> callback,
                        HomeScenarioState state)
    {
        Params = new DummyScenarioParams()
        {
            ID = id
        };
        _completedAction = callback;
        State = state;

        Logger.SetEnableLogging(true);

        _cts = new();
    }

    public async void OnActive()
    {
        Logger.Debug($"アクティブ{this.GetType()}");
   
        // 通常のホームシーンを実行
        var domain = new HomeSceneDomain();
        await domain.SceneTransition(_cts, (transitioner) => transitioner.StackType = SceneStackType.PushOrRetry);

        this.Initialize(domain._uiLayer).Forget();
    }

    public async UniTask Initialize(HomeUIScene homeUIScene)
    {
        await UniTask.WaitForEndOfFrame(homeUIScene);
        Logger.Debug($"Initialize {this.GetType()}");
        UIScene = homeUIScene;
        //UIScene._toTitleSceneButton.clickable.clicked += OnClicked;
    }

    public void Dispose()
    {
        //UIScene._toTitleSceneButton.clickable.clicked -= OnClicked;
        UIScene = null;
    }

    private void OnClicked()
    {
        Logger.Debug("ホームシーンでタイトル遷移ボタンクリック");
        var scenario = new TitleScenario();
        ScenarioContainer.SetActive(scenario, ChinType.NotChain);
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