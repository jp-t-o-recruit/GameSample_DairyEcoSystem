using System.Threading;
using VContainer;

using Logger = MyLogger.MapBy<HomeSceneDomain>;

/// <summary>
/// タイトルシーン構成管理
/// </summary>
public class HomeSceneDomain : LayeredSceneDomainBase<
    HomeScene,
    HomeUIScene,
    HomeFieldScene,
    HomeSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "HomeSceneDomainスクリプトから設定！";
    }

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
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
        // 共通開放なので最後呼び
        base.Discard(cts);
    }

    private async void ToBattleScene()
    {
        Logger.SetEnableLogging(true);
        Logger.Debug("バトルボタン押下");
        await new BattleSceneDomain().SceneTransition();
    }

    /// <summary>
    /// クレジット表記画面へ遷移ボタン押下
    /// </summary>
    private async void OnCreditNotationClicked()
    {
        await DomainCommonService.SceneTransition(_cts,
            // ホーム画面に戻るように明示的にシーンを連携する
            async () => {
                var domain = new CreditNotationSceneDomain();
                await domain.SceneTransition(editCallback: transitioner => {
                    transitioner.StackType = SceneStackType.Push;
                });
            },
            // 通信処理
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _cts.Token);
            });
    }

    /// <summary>
    /// タイトル画面へ遷移ボタン押下
    /// </summary>
    private async void OnTitleSceneButtonClicked()
    {
        Logger.SetEnableLogging(true);
        Logger.Debug($"{this} タイトルボタン押下");

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