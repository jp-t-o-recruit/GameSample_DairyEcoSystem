using Cysharp.Threading.Tasks;

using Logger = MyLogger.MapBy<TutorialSceneDomain>;

public class TutorialSceneDomain : DomainBase<
    TutorialScene,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    TutorialSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public ISceneTransitioner NextSceneTransition = new TitleSceneTransitioner();
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
        await CreateParam.NextSceneTransition.Transition();
    }
}
