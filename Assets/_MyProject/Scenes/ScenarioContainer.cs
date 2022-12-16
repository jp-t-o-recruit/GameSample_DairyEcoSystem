using Cysharp.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using static PlasticGui.LaunchDiffParameters;
using Logger = MyLogger.MapBy<ScenarioState>;

public class AdventureParams
{

}

/// <summary>
/// �o�g������
/// </summary>
public enum BattleResult
{
    /// <summary>
    /// �������i�����l�j
    /// </summary>
    Incomplete,
    /// <summary>
    /// �G���[
    /// </summary>
    Error,
    /// <summary>
    /// �M�u�A�b�v
    /// </summary>
    Giveup,
    /// <summary>
    /// ���g���C
    /// </summary>
    Retry,
    /// <summary>
    /// �s�k
    /// </summary>
    Lose,
    /// <summary>
    /// ��������
    /// </summary>
    Draw,
    /// <summary>
    /// ����
    /// </summary>
    Win,
}

public class BattleScene
{
    public IScenario Scenario { get; }
    BattleScene(IScenario scenario)
    {
        Scenario = scenario;
    }

    private void BuildBattleScene(IScenario scenario)
    {
        // �o�g���ɕK�v�ȃ��\�[�X����������
        // Scenario�������Ă���������ƂɓG������}�b�v��p�ӂ���
    }

    /// <summary>
    /// �M�u�A�b�v
    /// </summary>
    private void Giveup(){
        // �V�i���I�ɃM�u�A�b�v��`����
    }

    /// <summary>
    /// �o�g���I�����U���g
    /// </summary>
    private void Result()
    {
        var f = Scenario.Params.State;

        if (f == default)
        {
            // �A�h���F���`���[���Ăяo���H
            // �V�[���h���C�����Ăяo�����Ǘ����ׂ��Ȃ̂��H
            // �V�[���ɏI����`���A���ʂ�Ԃ�
        }else
        {
            // ���U���g��\�����āA���̌�~�b�V�����I����ʂɖ߂�
        }
    }
}

/// <summary>
/// ���s���_�ł̃V�i���I���
/// </summary>
public enum ScenarioState
{
    /// <summary>
    /// �V�i���I�����{
    /// </summary>
    Wait,
    /// <summary>
    /// �V�i���I���s��
    /// </summary>
    Active,
    /// <summary>
    /// �V�i���I���s�ς�
    /// </summary>
    Finished,
}

public interface IScenarioParams
{
    /// <summary>
    /// ���^���ʗpID
    /// </summary>
    string ID { get; }

    /// <summary>
    /// ���s���_�̑O�V�i���I
    /// </summary>
    IScenario Parent { get; set; }

    /// <summary>
    /// ���s�I���㎞�_�̎��V�i���I
    /// </summary>
    IScenario Child { get; set; }
    
    ScenarioState State { get; set; }
}

public class DummyScenarioParams: IScenarioParams
{
    public string ID { get; set; }
    /// <summary>
    /// ���s���_�̑O�V�i���I
    /// </summary>
    public IScenario Parent { get; set; }
    /// <summary>
    /// ���s�I���㎞�_�̎��V�i���I
    /// </summary>
    public IScenario Child { get; set; }

    public ScenarioState State { get; set; }
    public DummyScenarioParams()
    {
        Initialize();
    }
    public void Initialize()
    {
        State = ScenarioState.Wait;
    }
}

/// <summary>
/// �Q�[���i�s�̃V�[���Ή����x
/// </summary>
public interface IScenario
{

    public IScenarioParams Params { get; }
    public void OnActive();
}

/// <summary>
/// �V�i���I�����^�C�v
/// </summary>
public enum ChinType 
{
    Chain,
    NotChain
}


/// <summary>
/// �Q�[���i�s�̃p�����[�^���܂Ƃ߂�
/// </summary>
public static class ScenarioContainer
{
    private static int _listIndex = 0;

    /// <summary>
    /// �V�i���I���̏���
    /// </summary>
    public static IScenario Active { get; private set; }

    public class DummyScenario : IScenario
    {
        public IScenarioParams Params { get; set; }
        public void OnActive() {  }
    }

    public static DummyScenario CreateDummyScenario()
    {
        return new DummyScenario();
    }

    /// <summary>
    /// ���{��Ԃ̃V�i���I����シ��
    /// </summary>
    /// <param name="scenario"></param>
    public static void SetActive(IScenario scenario, ChinType chain)
    {
        if(null != FindAncestral((ancestor) => ancestor.Params.ID == scenario.Params.ID))
        // �ݒ肵�悤�Ƃ��Ă���V�i���I�����ɃA�N�e�B�u�V�i���I�̑c��ł���i�ݒ肷��Əz����j�ꍇ
        {
            Logger.Warning($"�z����V�i���I���A�N�e�B�u�ɂ��Ă��܂��BID:{scenario.Params.ID}");
        }
        var parent = Active;
        SetFinish(parent, scenario, chain);

        Active = scenario;
        Active.Params.State = ScenarioState.Active;

        // TODO
        //parent.OnFinished();
        // TODO
        Active.OnActive();
    }
    /// <summary>
    /// ���s�ςݏ�Ԃɐݒ�
    /// </summary>
    /// <param name="scenario"></param>
    private static void SetFinish(IScenario scenario, IScenario child, ChinType chain)
    {
        if (scenario == default) return;
        scenario.Params.State = ScenarioState.Finished;
        if (chain != ChinType.Chain) return;
        child.Params.Parent = scenario;
    }

    /// <summary>
    /// �c������ɓ��ꏈ��������@�\
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="matcher"></param>
    /// <returns></returns>
    private static IScenario AncestralControl(IScenario scenario,
                                              Func<IScenario, bool> matcher)
    {
        bool isMattched;
        do {
            scenario = scenario.Params.Parent;
            if (scenario == default)
            {
                return null;
            }
            isMattched = matcher(scenario);
        } while (!isMattched);
        return scenario;
    }

    /// <summary>
    /// �c������̃V�i���I�̊J������
    /// </summary>
    /// <param name="startScenario"></param>
    public static void ReleaseScenario(IScenario startScenario)
    {
        Func<IScenario, bool> callback = (scenario) => {
            ReleaseChild(scenario);
            return false;
        };
        callback(startScenario);
        AncestralControl(startScenario, callback);
    }

    /// <summary>
    /// �w��V�i���I����q���V�i���I�����
    /// </summary>
    /// <param name="scenario"></param>
    private static void ReleaseChild(IScenario scenario)
    {
        IScenario child = scenario.Params.Child;
        scenario.Params.Child = default;

        if (child == default) return;
        child.Params.Parent = default;
    }

    /// <summary>
    /// �c������Ɍ�������
    /// </summary>
    /// <param name="matcher">��������R�[���o�b�N</param>
    /// <returns>��v���Ȃ��ꍇdefault</returns>
    public static IScenario FindAncestral(Func<IScenario, bool> matcher)
    {
        return AncestralControl(Active, matcher);
    }

    /// <summary>
    /// ���[�g�V�i���I�̎擾
    /// </summary>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static IScenario GetRootScenario(IScenario scenario)
    {
        var root = scenario;
        if (root.Params.Parent != default)
        {
            root = GetRootScenario(root.Params.Parent);
        }
        return root;
    }
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

public class ScenarioBuilder
{
    public static IScenario SceneBuild(SceneEnum sceneEnum, string detail)
    {
        IScenario s = new PlayModeScenario();
        if (sceneEnum == SceneEnum.HomeScene)
        {
            s = HomeScenarioFactory.Build(detail);
        }
        return s;
    }

    public static ButtleScenario BuildBattle()
    {
        string id = "id999";
        // TODO�R�[���o�b�N�őg�ݗ��Ă�Ƒg�ݗ��ẴX�R�[�v�������Ǝc��
        // ���Ƃ����đ��ɂ��悤���Ȃ��H
        // �R�[���o�b�N�쐬�t�@�N�g���H�X�e�[�g�N���X�H
        Func<ButtleScenario, UniTask> callback = async (buttleScenario) => {
            if (buttleScenario.Result == BattleResult.Win)
            {
                // �M�u�A�b�v���̎��V�i���I
                // ���g���C���̎��V�i���I
                // �s�k���̎��V�i���I
                // �����������̎��V�i���I
                // ���������̎��V�i���I
                // �����ɂ�鎟�V�i���I
                await new HomeSceneTransitioner().Transition();
            }
            else
            {
                await UniTask.Delay(0);
            }
        };
        return new ButtleScenario(id, callback);
    }
}