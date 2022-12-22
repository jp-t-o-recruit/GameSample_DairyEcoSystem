using UnityEngine;

/// <summary>
/// バトル結果
/// </summary>
public enum BattleResult
{
    /// <summary>
    /// 未完了（初期値）
    /// </summary>
    Incomplete,
    /// <summary>
    /// エラー
    /// </summary>
    Error,
    /// <summary>
    /// ギブアップ
    /// </summary>
    Giveup,
    /// <summary>
    /// リトライ
    /// </summary>
    Retry,
    /// <summary>
    /// 敗北
    /// </summary>
    Lose,
    /// <summary>
    /// 引き分け
    /// </summary>
    Draw,
    /// <summary>
    /// 勝利
    /// </summary>
    Win,
}

public class BattleScene : MonoBehaviour, ILayeredSceneLogic
{
    public IScenario Scenario { get; }
    BattleScene(IScenario scenario)
    {
        Scenario = scenario;
    }

    private void BuildBattleScene(IScenario scenario)
    {
        // バトルに必要なリソースを準備する
        // Scenarioが持っている情報をもとに敵味方やマップを用意する
    }

    /// <summary>
    /// ギブアップ
    /// </summary>
    private void Giveup()
    {
        // シナリオにギブアップを伝える
    }

    /// <summary>
    /// バトル終了リザルト
    /// </summary>
    private void Result()
    {
        var f = Scenario.Params.State;

        if (f == default)
        {
            // アドヴェンチャーを呼び出す？
            // シーンドメインが呼び出しを管理すべきなのか？
            // シーンに終了を伝え、結果を返す
        }
        else
        {
            // リザルトを表示して、その後ミッション選択画面に戻る
        }
    }
}
