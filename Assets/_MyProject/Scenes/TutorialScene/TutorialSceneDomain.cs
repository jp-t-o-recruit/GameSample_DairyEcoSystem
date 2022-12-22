using Cysharp.Threading.Tasks;
using System.Threading;

using Logger = MyLogger.MapBy<TutorialSceneDomain>;

/// <summary>
/// �`���[�g���A���V�[��
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
        // TODO�@�e���}�l�[�W���[������or�����n��
        CreateParam = new DomainParam();
    }

    /// <summary>
    /// ���̃V�[���֑J��
    /// </summary>
    public async UniTask ChangeSceneNext()
    {
        Logger.Debug("CreateParam null?:" + (null == CreateParam));
        Logger.Debug("nextSceneTransition null?:" + (null == CreateParam.NextSceneTransition));
        // TODO�@�`���[�g���A���V�[�����d�˂ĊJ�n���āA���V�[���̓`���[�g���A���V�[���I�������ł�����
        // ���A�V�[���J��������ExScene�̋@�\�Őe�V�[�����q�V�[���𐧌䂷����ɂȂ��Ă��Ȃ�
        CancellationTokenSource cts = new();
        await CreateParam.NextSceneTransition(cts);
    }
}
