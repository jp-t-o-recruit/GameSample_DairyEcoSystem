using Cysharp.Threading.Tasks;
using System;
using Logger = MyLogger.MapBy<ScenarioState>;


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

    bool ParentLock { get; set; }
    bool ChildLock { get; set; }


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
    public bool ParentLock { get; set; }
    public bool ChildLock { get; set; }

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

// TODO �V�i���I�r�[���Y���V�i���I�������ɓǂݍ��ނȂ炱�̎w�W�v��Ȃ�
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
        if(default != FindAncestral((ancestor) => ancestor.Params.ID == scenario.Params.ID))
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
        if (Active == default) return default;
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

/// <summary>
/// ���̃V�i���I�r�[���Y�ł�肽������
/// </summary>
public enum ScenarioBeansCategory
{
    Default,
    Adventure,
    Battle,
    Tutorial,
}

/// <summary>
/// �V�i���I�r�[���Y�̃f�[�^�x�[�X���R�[�h
/// </summary>
public class ScenarioDataBase
{
    public int ID;
    public ScenarioBeansCategory Category;
    public int CategoryID;
}

/// <summary>
/// �V�i���I�r�[���Y
/// �V�[���̒ʏ퐧����㏑������V�i���I�\������ŏ��P��
/// </summary>
public class IScenarioBeans2
{

}
public class ScenarioBuilder {
    public bool isHoge = false;
}


public class ScenarioFactory
{
    public static IScenarioBeans2 Create(ScenarioBuilder builder)
    {
        int scenarioId = builder.isHoge ? 10 : 12;
        return Factory(scenarioId);
    }


    private static IScenarioBeans2 Factory(int scenarioId)
    {
        int scenarioType = scenarioId;
        IScenarioBeans2 s = new();
        switch(scenarioType)
        {
            case 1:
                s = TryHome(s);
                break;
            default:
                break;
        }
        return s;
    }

    private static IScenarioBeans2 TryHome(IScenarioBeans2 s)
    {
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
            }
            else
            {
                await UniTask.Delay(0);
            }
        };
        return new ButtleScenario(id, callback);
    }
}