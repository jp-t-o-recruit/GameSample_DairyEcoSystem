using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

/// <summary>
/// バトルシーン
/// </summary>
public class BattleSceneDomain : DomainBase<
    BattleScene,
    BattleUIScene,
    BattleFieldScene,
    BattleSceneDomain.DomainParam>
{
    
    public class DomainParam : IDomainParamBase
    {

    }

    [Inject]
    public BattleSceneDomain()
    {
    }

    public override void Initialize(CancellationTokenSource cts)
    {
        base.Initialize(cts);
        _uiLayer.questButton.clickable.clicked += ToHome;
    }

    public override void Discard(CancellationTokenSource cts)
    {
        base.Discard(cts);

        _uiLayer.questButton.clickable.clicked -= ToHome;
    }

    private async void ToHome()
    {
        HomeSceneDomain domain = new ();
        CancellationTokenSource cts = new();
        await domain.SceneTransition(cts);
    }
}

/// <summary>
/// バトルデータベース
/// </summary>
public class BattleDataBase
{
    public int ID;
}


public class ButtleScenario : IScenario
{
    public IScenarioParams Params { get; set; }

    public BattleResult Result { get; }

    private Func<ButtleScenario, UniTask> _completedAction;

    public ButtleScenario(string id,
                          Func<ButtleScenario, UniTask> callback)
    {
        Params = new DummyScenarioParams()
        {
            ID = id
        };
        _completedAction = callback;
        Result = BattleResult.Incomplete;
    }

    public void OnActive() { }
    /// <summary>
    /// シナリオの完了
    /// 次シナリオを呼び出す
    /// </summary>
    private async UniTask Complete()
    {
        await _completedAction(this);
    }
}
