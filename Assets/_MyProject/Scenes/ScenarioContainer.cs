using Cysharp.Threading.Tasks;
using System;
using Logger = MyLogger.MapBy<ScenarioState>;


/// <summary>
/// 実行時点でのシナリオ状態
/// </summary>
public enum ScenarioState
{
    /// <summary>
    /// シナリオ未実施
    /// </summary>
    Wait,
    /// <summary>
    /// シナリオ実行中
    /// </summary>
    Active,
    /// <summary>
    /// シナリオ実行済み
    /// </summary>
    Finished,
}

public interface IScenarioParams
{
    /// <summary>
    /// 同型識別用ID
    /// </summary>
    string ID { get; }

    /// <summary>
    /// 実行時点の前シナリオ
    /// </summary>
    IScenario Parent { get; set; }

    /// <summary>
    /// 実行終了後時点の次シナリオ
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
    /// 実行時点の前シナリオ
    /// </summary>
    public IScenario Parent { get; set; }
    /// <summary>
    /// 実行終了後時点の次シナリオ
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
/// ゲーム進行のシーン対応粒度
/// </summary>
public interface IScenario
{

    public IScenarioParams Params { get; }
    public void OnActive();
}

// TODO シナリオビーンズをシナリオ処理毎に読み込むならこの指標要らない
/// <summary>
/// シナリオ結合タイプ
/// </summary>
public enum ChinType 
{
    Chain,
    NotChain
}


/// <summary>
/// ゲーム進行のパラメータをまとめた
/// </summary>
public static class ScenarioContainer
{
    /// <summary>
    /// シナリオ実体順序
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
    /// 実施状態のシナリオを交代する
    /// </summary>
    /// <param name="scenario"></param>
    public static void SetActive(IScenario scenario, ChinType chain)
    {
        if(default != FindAncestral((ancestor) => ancestor.Params.ID == scenario.Params.ID))
        // 設定しようとしているシナリオが既にアクティブシナリオの祖先である（設定すると循環する）場合
        {
            Logger.Warning($"循環するシナリオをアクティブにしています。ID:{scenario.Params.ID}");
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
    /// 実行済み状態に設定
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
    /// 祖先方向に同一処理をする機構
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
    /// 祖先方向のシナリオの開放処理
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
    /// 指定シナリオから子供シナリオを解放
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
    /// 祖先方向に検索する
    /// </summary>
    /// <param name="matcher">検索判定コールバック</param>
    /// <returns>一致しない場合default</returns>
    public static IScenario FindAncestral(Func<IScenario, bool> matcher)
    {
        if (Active == default) return default;
        return AncestralControl(Active, matcher);
    }

    /// <summary>
    /// ルートシナリオの取得
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
/// そのシナリオビーンズでやりたいこと
/// </summary>
public enum ScenarioBeansCategory
{
    Default,
    Adventure,
    Battle,
    Tutorial,
}

/// <summary>
/// シナリオビーンズのデータベースレコード
/// </summary>
public class ScenarioDataBase
{
    public int ID;
    public ScenarioBeansCategory Category;
    public int CategoryID;
}

/// <summary>
/// シナリオビーンズ
/// シーンの通常制御を上書きするシナリオ構成する最小単位
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
        // TODOコールバックで組み立てると組み立てのスコープがずっと残る
        // かといって他にやりようがない？
        // コールバック作成ファクトリ？ステートクラス？
        Func<ButtleScenario, UniTask> callback = async (buttleScenario) => {
            if (buttleScenario.Result == BattleResult.Win)
            {
                // ギブアップ時の次シナリオ
                // リトライ時の次シナリオ
                // 敗北時の次シナリオ
                // 引き分け時の次シナリオ
                // 勝った時の次シナリオ
                // 条件による次シナリオ
            }
            else
            {
                await UniTask.Delay(0);
            }
        };
        return new ButtleScenario(id, callback);
    }
}