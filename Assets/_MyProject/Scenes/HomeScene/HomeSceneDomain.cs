using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;

using Logger = MyLogger.MapBy<HomeSceneDomain>;

/// <summary>
/// タイトルシーン構成管理
/// </summary>
public class HomeSceneDomain : DomainBase<
    HomeScene,
    HomeUIScene,
    HomeFieldScene,
    HomeSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "HomeSceneDomainスクリプトから設定！";
    }

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
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
    /// クレジット表記画面へ遷移ボタン押下
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        await DomainCommonService.SceneTransition(_clientLifetimeTokenSource,
            // ホーム画面に戻るように明示的にシーンを連携する
            async () => {
                var param = new CreditNotationSceneDomain.DomainParam() {
                    sceneTransitionerCollback = () => new HomeSceneTransitioner()
                    {
                        // TODO
                        // 表示中シーンが前（親）の状態として次に遷移するシーンの条件として参照する
                        PrevRelation = SceneRelation.HookLink,
                        // 表示中シーンが自分の状態として次に遷移するシーンの条件として参照する
                        // リンクする
                        SelfRelation = SceneRelation.HookLink,
                        // 表示中シーンが次に遷移するシーンの条件として参照する
                        // リンクする
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
            // 通信処理
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _clientLifetimeTokenSource.Token);
            });
    }

    /// <summary>
    /// タイトル画面へ遷移ボタン押下
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