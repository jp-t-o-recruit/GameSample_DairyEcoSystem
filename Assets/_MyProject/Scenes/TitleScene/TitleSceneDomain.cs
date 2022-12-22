using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

/// <summary>
/// タイトルシーン構成管理
/// </summary>
public class TitleSceneDomain : DomainBase<
    TitleScene,
    TitleUIScene,
    TitleFieldScene,
    TitleSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "TitleSceneDomainスクリプトから設定！";
    }

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
    private CancellationTokenSource _cts;

    [Inject]
    //public TitleSceneDomain(CancellationTokenSource clientLifetimeTokenSource)
    //                        DomainParam domainParam)
    public TitleSceneDomain()
    {
        _cts = new();
    }
    public override void Initialize(CancellationTokenSource cts)
    {
        base.Initialize(cts);
        Logger.SetEnableLogging(true);
        Logger.Debug($"Initialize {this.GetType()}");
        _uiLayer._nextSceneButton.clickable.clicked += OnButtonClicked;
        _uiLayer._creditNotationButton.clickable.clicked += OnCreditNotationClicked;

        _uiLayer._viewLabel.text = _param.ViewLabel;
    }

    public override void Suspend(CancellationTokenSource cts)
    {
        base.Suspend(cts);
    }
    public override void Resume(CancellationTokenSource cts)
    {
        base.Resume(cts);
    }
    public override void Discard(CancellationTokenSource cts)
    {
        base.Discard(cts);
        Logger.Debug($"Discard {this.GetType()}");

        _cts?.Cancel();
        _cts?.Dispose();
        _uiLayer._nextSceneButton.clickable.clicked -= OnButtonClicked;
        _uiLayer._creditNotationButton.clickable.clicked -= OnCreditNotationClicked;
        Logger.SetEnableLogging(false);
    }

    private async void OnCreditNotationClicked()
    {
        await new CreditNotationSceneDomain().SceneTransition(_cts);
    }

    private async void OnButtonClicked()
    {
        Logger.Debug("クリック！　OnButtonClicked");
        await Login();
    }

    public async UniTask Login()
    {
        Logger.Debug("1/3　ログイン通信開始");
        string mockAccountString = "特に使用されない";
        UserInfo userInfo = null;

        await DomainCommonService.WebConnection(_cts, async (webService) => {
            userInfo = await webService.PutLogin(mockAccountString, _cts.Token);
        });

        Logger.Debug("2/3　ログイン通信終了とログイン状態更新");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3　ログイン終了 シーン遷移開始");

        GoHomeSceneTransition(userInfo);
    }

    private async void GoHomeSceneTransition(UserInfo userInfo)
    {
        HomeSceneDomain homeDomain = new()
        {
            Param = new HomeSceneDomain.DomainParam() {
                ViewLabel = $"ユーザー名:{userInfo.UserName}"
            }
        };
        await homeDomain.SceneTransition(_cts);
    }
}