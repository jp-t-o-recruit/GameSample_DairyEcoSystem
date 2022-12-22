using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

/// <summary>
/// �o�g���V�[��
/// </summary>
public class BattleSceneDomain : LayeredSceneDomainBase<
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
        _uiLayer.questButton.clickable.clicked -= ToHome;
        base.Discard(cts);
    }

    private async void ToHome()
    {
        await new HomeSceneDomain().SceneTransition();
    }
}

/// <summary>
/// �o�g���f�[�^�x�[�X
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
    /// �V�i���I�̊���
    /// ���V�i���I���Ăяo��
    /// </summary>
    private async UniTask Complete()
    {
        await _completedAction(this);
    }
}
