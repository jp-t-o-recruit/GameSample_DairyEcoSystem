using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine.UIElements;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

public class TitleSceneDomain : DomainBase<TitleSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "TitleSceneDomainスクリプトから設定！";
    }

    public override DomainParam CreateParam()
    {
        return new DomainParam();
    }

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
    readonly CancellationTokenSource _clientLifetimeTokenSource;

    public void Dispose()
    {
        _clientLifetimeTokenSource?.Cancel();
        _clientLifetimeTokenSource?.Dispose();
    }

    [Inject]
    public TitleSceneDomain(CancellationTokenSource clientLifetimeTokenSource,
                            DomainParam domainBaseParam)
    {
        _clientLifetimeTokenSource = clientLifetimeTokenSource;
        Param = domainBaseParam;
    }

    public async UniTask Login()
    {
        var webService = WebServiceManager.GetWebService();
        string mockAccountString = "特に使用されない";
        Logger.Debug("1/3　ログイン通信開始");

        UserInfo userInfo = null;
        try
        {
            userInfo = await webService.PutLogin(mockAccountString, _clientLifetimeTokenSource.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == _clientLifetimeTokenSource.Token)
        {
            if (_clientLifetimeTokenSource.IsCancellationRequested)
            {
                // クライアント自体がDisposeされたのでOperationCanceledException、或いは独自の例外を投げる
                throw new OperationCanceledException($"{typeof(TitleSceneDomain)} is disposed.", ex, _clientLifetimeTokenSource.Token);
            }
        }

        Logger.Debug("2/3　ログイン通信終了とログイン状態更新");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3　ログイン終了");

        // シーン遷移
        await ChangeScene();
    }

    private async UniTask ChangeScene()
    {
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        var param = new HomeUIScene.CreateParameter() { ViewLabel = $"ユーザー名:{userAccountServiceDomain.User.userName}" };
        var transition = new HomeSceneTransition() { Parameter = param };
        await ExSceneManager.Instance.Replace(transition);
    }
}
