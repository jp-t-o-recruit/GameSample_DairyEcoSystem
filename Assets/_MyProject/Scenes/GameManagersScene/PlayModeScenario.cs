#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine.SceneManagement;
using System;
using Logger = MyLogger.MapBy<PlayModeScenario>;
using System.Linq;
using Cysharp.Threading.Tasks;

/// <summary>
/// UnityのPlayModeで呼び出すシナリオ
/// </summary>
public class PlayModeScenario : IScenario
{
    public IScenarioParams Params { get; set; }

    public PlayModeScenario()
    {
        Params = new DummyScenarioParams()
        {
            ID = $"{typeof(PlayModeScenario)}"
        };

    }
    /// <summary>
    /// プレイモードなのでアクティブシーンに則って動作開始
    /// 指定不足なデータは各シーンが自前で補う
    /// </summary>
    public void OnActive()
    {
        // playモードでインスタンスが続いてるかもしれないので初期化
        ExSceneManager.Instance.Initialize();
        //SetupScenarioBySaveData();

        Scene scene = SceneManager.GetActiveScene();
        IScenario scenario = null;

        if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.TitleScene, () => new TitleScenario())) { }
        else if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.HomeScene, () => new HomeScenario("TODOgame", null, new HomeScenarioState()))) { }
        else if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.BattleScene, () => new DefaultScenario("TODO999"))) { }

        Logger.SetEnableLogging(true);
        Logger.Debug($"PlayModeで{scenario.GetType()}シナリオを開始");

        ScenarioContainer.SetActive(scenario, ChinType.NotChain);
    }

    private bool TryParceSceneToScenario(out IScenario scenario, string sceneName, SceneEnum sceneEnum, Func<IScenario> callback)
    {
        scenario = default;
        if (sceneName == Enum.GetName(typeof(SceneEnum), sceneEnum))
        {
            scenario = callback();
        }
        return scenario != default;
    }

    /// <summary>
    /// セーブデータからシナリオ作成&シーン遷移までやる
    /// </summary>
    /// <returns></returns>
    private bool SetupScenarioBySaveData()
    {
        var accountService = UserAccountDomainManager.GetService();
        var first = accountService.User.Build.ScenarioProgression.AsEnumerable().First();
        if (first != default)
        {
            // TODO まだセーブからの読み込みを作っていない
            // debug開始用のセーブデータがあるならそれで開始
            // TODO　シナリオIDからシナリオ作成するサービスを作る
            // TODO　シナリオ作成したらシーン遷移する処理を作る
            new HomeSceneDomain().SceneTransition().Forget();
        }
        return first != default;
    }
}

#endif