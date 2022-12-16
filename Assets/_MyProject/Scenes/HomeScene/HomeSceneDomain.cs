using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;

using Logger = MyLogger.MapBy<HomeSceneDomain>;

/// <summary>
/// �^�C�g���V�[���\���Ǘ�
/// </summary>
public class HomeSceneDomain : DomainBase<
    HomeScene,
    HomeUIScene,
    HomeFieldScene,
    HomeSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "HomeSceneDomain�X�N���v�g����ݒ�I";
    }

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
    private CancellationTokenSource _clientLifetimeTokenSource;

    [Inject]
    //public HomeSceneDomain(CancellationTokenSource clientLifetimeTokenSource)
    //                        DomainParam domainParam)
    public HomeSceneDomain()
    {
        _clientLifetimeTokenSource = new();

        Logger.SetEnableLogging(false);
    }

    public override async UniTask Initialize()
    {
        await base.Initialize();
        Logger.Debug($"_viewLabel: {_uiLayer._viewLabel != null },_initialParam: {_initialParam != null}, ViewLabel: {_initialParam.ViewLabel != null}");
        _uiLayer._viewLabel.text = _initialParam.ViewLabel;
        _uiLayer._nextSceneButton.clickable.clicked += OnNextSceneButtonClicked;
        // TODO
        //_uiLayer._toSaveDataBuilderSceneButton.clickable.clicked += OnButtonClicked;
        _uiLayer._toTitleSceneButton.clickable.clicked += OnTitleSceneButtonClicked;
    }

    public override async UniTask Suspend()
    {
        await base.Suspend();
    }
    public override async UniTask Resume()
    {
        await base.Resume();
    }
    public override async UniTask Discard()
    {
        await base.Discard();

        _clientLifetimeTokenSource?.Cancel();
        _clientLifetimeTokenSource?.Dispose();
        _clientLifetimeTokenSource = null;
        _uiLayer._nextSceneButton.clickable.clicked -= OnNextSceneButtonClicked;
        // TODO
        //_uiLayer._toSaveDataBuilderSceneButton.clickable.clicked -= OnButtonClicked;
        _uiLayer._toTitleSceneButton.clickable.clicked -= OnTitleSceneButtonClicked;
        Logger.SetEnableLogging(false);
    }

    /// <summary>
    /// �N���W�b�g�\�L��ʂ֑J�ڃ{�^������
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        await DomainCommonService.SceneTransition(_clientLifetimeTokenSource,
            // �z�[����ʂɖ߂�悤�ɖ����I�ɃV�[����A�g����
            async () => {
                var param = new CreditNotationSceneDomain.DomainParam() {
                    sceneTransitionerCollback = () => new HomeSceneTransitioner()
                    {
                        // TODO
                        // �\�����V�[�����O�i�e�j�̏�ԂƂ��Ď��ɑJ�ڂ���V�[���̏����Ƃ��ĎQ�Ƃ���
                        PrevRelation = SceneRelation.HookLink,
                        // �\�����V�[���������̏�ԂƂ��Ď��ɑJ�ڂ���V�[���̏����Ƃ��ĎQ�Ƃ���
                        // �����N����
                        SelfRelation = SceneRelation.HookLink,
                        // �\�����V�[�������ɑJ�ڂ���V�[���̏����Ƃ��ĎQ�Ƃ���
                        // �����N����
                        NextRelation = SceneRelation.None,
                    }
                };
                var transitioner = new CreditNotationSceneTransitioner() {
                    Parameter = param,
                    PrevRelation = SceneRelation.StartLink,
                    SelfRelation = SceneRelation.HookLink,
                    NextRelation = SceneRelation.HookLink,
                };
                await transitioner.Transition();
            },
            // �ʐM����
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _clientLifetimeTokenSource.Token);
            });
    }

    /// <summary>
    /// �^�C�g����ʂ֑J�ڃ{�^������
    /// </summary>
    private async void OnTitleSceneButtonClicked()
    {
        await DomainCommonService.SceneTransition(_clientLifetimeTokenSource,
            async () => {
                var transitioner = new TitleSceneTransitioner();
                await transitioner.Transition();
            },
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _clientLifetimeTokenSource.Token);
            });
    }
}