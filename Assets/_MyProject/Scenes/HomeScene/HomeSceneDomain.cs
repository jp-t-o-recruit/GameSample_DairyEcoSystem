using System.Threading;
using VContainer;

using Logger = MyLogger.MapBy<HomeSceneDomain>;

/// <summary>
/// �^�C�g���V�[���\���Ǘ�
/// </summary>
public class HomeSceneDomain : LayeredSceneDomainBase<
    HomeScene,
    HomeUIScene,
    HomeFieldScene,
    HomeSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "HomeSceneDomain�X�N���v�g����ݒ�I";
    }

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
    private CancellationTokenSource _cts;

    [Inject]
    //public HomeSceneDomain(CancellationTokenSource clientLifetimeTokenSource)
    //                        DomainParam domainParam)
    public HomeSceneDomain()
    {
        _cts = new();

        Logger.SetEnableLogging(true);
    }

    public override void Initialize(CancellationTokenSource cts)
    {
        base.Initialize(cts);
        Logger.Debug($"_viewLabel: {_uiLayer._viewLabel != null},_initialParam: {_initialParam != null}, ViewLabel: {_initialParam.ViewLabel != null}");
        _uiLayer._viewLabel.text = _initialParam.ViewLabel;
        _uiLayer._nextSceneButton.clickable.clicked += ToBattleScene;
        // TODO
        //_uiLayer._toSaveDataBuilderSceneButton.clickable.clicked += OnButtonClicked;
        _uiLayer._toTitleSceneButton.clickable.clicked += OnTitleSceneButtonClicked;
    }

    public override void Suspend(CancellationTokenSource cts)
    {
        base.Suspend(cts);
        Logger.Debug($"Suspend {this.GetType()}");
    }
    public override void Resume(CancellationTokenSource cts)
    {
        base.Resume(cts);
        Logger.Debug($"Resume {this.GetType()}");
    }
    public override void Discard(CancellationTokenSource cts)
    {
        Logger.Debug($"Discard {this.GetType()}");

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _uiLayer._nextSceneButton.clickable.clicked -= ToBattleScene;
        // TODO
        //_uiLayer._toSaveDataBuilderSceneButton.clickable.clicked -= OnButtonClicked;
        _uiLayer._toTitleSceneButton.clickable.clicked -= OnTitleSceneButtonClicked;
        Logger.UnloadEnableLogging();
        // ���ʊJ���Ȃ̂ōŌ�Ă�
        base.Discard(cts);
    }

    private async void ToBattleScene()
    {
        Logger.SetEnableLogging(true);
        Logger.Debug("�o�g���{�^������");
        await new BattleSceneDomain().SceneTransition();
    }

    /// <summary>
    /// �N���W�b�g�\�L��ʂ֑J�ڃ{�^������
    /// </summary>
    private async void OnCreditNotationClicked()
    {
        await DomainCommonService.SceneTransition(_cts,
            // �z�[����ʂɖ߂�悤�ɖ����I�ɃV�[����A�g����
            async () => {
                var domain = new CreditNotationSceneDomain();
                await domain.SceneTransition(editCallback: transitioner => {
                    transitioner.StackType = SceneStackType.Push;
                });
            },
            // �ʐM����
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _cts.Token);
            });
    }

    /// <summary>
    /// �^�C�g����ʂ֑J�ڃ{�^������
    /// </summary>
    private async void OnTitleSceneButtonClicked()
    {
        Logger.SetEnableLogging(true);
        Logger.Debug($"{this} �^�C�g���{�^������");

        CancellationTokenSource endDomainCts = new ();

        await DomainCommonService.SceneTransition(endDomainCts,
            async () => {
                await new TitleSceneDomain().SceneTransition();
            },
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, endDomainCts.Token);
            });
    }
}