using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

// とりあえず動作検証用にここに書く
public class TitleFieldScene : ILayeredSceneField
{

}

/// <summary>
/// タイトルシーン構成管理
/// </summary>
public class TitleSceneDomain : DomainBase<
    TitleScene,
    TitleUIScene,
    TitleFieldScene,
    TitleSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "TitleSceneDomainスクリプトから設定！";
    }

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
    private CancellationTokenSource _clientLifetimeTokenSource;

    Dictionary<SceneLayer, ILayeredScene> _layerDef;

    [Inject]
    //public TitleSceneDomain(CancellationTokenSource clientLifetimeTokenSource)
    //                        DomainParam domainParam)
    public TitleSceneDomain()
    {
        CancellationTokenSource clientLifetimeTokenSource = new();
        _clientLifetimeTokenSource = clientLifetimeTokenSource;
        //_clientLifetimeTokenSource = clientLifetimeTokenSource;
        //Param = domainParam;
        Param = new DomainParam();
    }
    public override async UniTask Initialize()
    {
        await base.Initialize();
        _uiLayer._nextSceneButton.clickable.clicked += OnButtonClicked;
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
        _uiLayer._nextSceneButton.clickable.clicked -= OnButtonClicked;
        Logger.SetEnableLogging(false);
    }

    private async void OnButtonClicked()
    {
        await Login();
    }

    public async UniTask Login()
    {
        Logger.Debug("1/3　ログイン通信開始");
        string mockAccountString = "特に使用されない";
        UserInfo userInfo = null;

        await DomainCommonService.WebConnection(_clientLifetimeTokenSource, async (webService) => {
            userInfo = await webService.PutLogin(mockAccountString, _clientLifetimeTokenSource.Token);
        });

        Logger.Debug("2/3　ログイン通信終了とログイン状態更新");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3　ログイン終了 シーン遷移開始");

        var tran = GetHomeSceneTransitioner(userInfo);
        await tran.Transition();
    }

    private HomeSceneTransitioner GetHomeSceneTransitioner(UserInfo userInfo)
    {
        var param = new HomeSceneDomain.DomainParam() { ViewLabel = $"ユーザー名:{userInfo.UserName}" };
        return new HomeSceneTransitioner() { NextRelation = SceneRelation.Free, Parameter = param };
    }
}