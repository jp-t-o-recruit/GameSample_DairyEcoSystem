using Cysharp.Threading.Tasks;
using System.Threading;

using Logger = MyLogger.MapBy<TutorialSceneDomain>;

/// <summary>
/// チュートリアルシーン
/// </summary>
public class TutorialSceneDomain : DomainBase<
    TutorialScene,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    TutorialSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public System.Func<CancellationTokenSource, UniTask> NextSceneTransition = async (cts) => await new TitleSceneDomain().SceneTransition(cts);
    }
    DomainParam CreateParam;

    public TutorialSceneDomain()
    {
        // TODO　親かマネージャーが生成or引き渡し
        CreateParam = new DomainParam();
    }

    /// <summary>
    /// 次のシーンへ遷移
    /// </summary>
    public async UniTask ChangeSceneNext()
    {
        Logger.Debug("CreateParam null?:" + (null == CreateParam));
        Logger.Debug("nextSceneTransition null?:" + (null == CreateParam.NextSceneTransition));
        // TODO　チュートリアルシーンを重ねて開始して、次シーンはチュートリアルシーン終了だけでもいい
        // が、シーン開発分離とExSceneの機構で親シーンが子シーンを制御する作りになっていない
        CancellationTokenSource cts = new();
        await CreateParam.NextSceneTransition(cts);
    }
}
