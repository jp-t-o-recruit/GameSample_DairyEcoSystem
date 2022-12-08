using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

/// <summary>
/// タイトルシーン構成管理
/// </summary>
public class TitleSceneDomain : DomainBase
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "TitleSceneDomainスクリプトから設定！";
    }

    DomainParam Param;

    // IDisposableと引っ掛けて、Client自体がDisposeされたら実行中のリクエストも終了させるようにする
    private CancellationTokenSource _clientLifetimeTokenSource;

    Dictionary<SceneLayer, ILayeredScene> _layerDef;

    //[Inject]
    //public void Construct(CancellationTokenSource clientLifetimeTokenSource)
    //{
    //    _clientLifetimeTokenSource = clientLifetimeTokenSource;
    //    //Param = domainParam;
    //    Param = new DomainParam();
    //}
    [Inject]
    //public TitleSceneDomain(CancellationTokenSource clientLifetimeTokenSource,
    //                        DomainParam domainParam)
    public TitleSceneDomain()
    {
        CancellationTokenSource clientLifetimeTokenSource = new();
        _clientLifetimeTokenSource = clientLifetimeTokenSource;
        //Param = domainParam;
        Param = new DomainParam();
    }
    public void Dispose()
    {
        _clientLifetimeTokenSource?.Cancel();
        _clientLifetimeTokenSource?.Dispose();
        Logger.SetEnableLogging(false);
    }

    public async UniTask TryStart()
    {
        //var layerList = Enum.GetValues(typeof(SceneLayer)).Cast<SceneLayer>().ToList();
        //var nonLoaded = layerList.Where(v => !_layerDef.ContainsKey(v));

        var trantision = new TitleSceneTransition();
        await ExSceneManager.Instance.PushOrRetry(trantision);
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

        // TODO 設計変更中なのでシーンチェンジは呼べない
        await ExSceneManager.Instance.Replace(transition);
    }

    internal void SetLayerScene(ILayeredScene scene)
    {
        _layerDef ??= new Dictionary<SceneLayer, ILayeredScene>();
        var layer = ExSceneManager.Instance.GetLayer(scene);
        _layerDef.Add(layer, scene);
    }
}
