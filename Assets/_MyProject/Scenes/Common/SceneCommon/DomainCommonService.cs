
using Cysharp.Threading.Tasks;
using System;
using System.Threading;


using Logger = MyLogger.MapBy<DomainCommonService>;

public class DomainCommonService
{
    /// <summary>
    /// シーン間移動
    /// </summary>
    /// <param name="cLTS"></param>
    /// <param name="sceneTransitionerCollback">通信終了後シーン移動実体を返すコールバック（return前に通信後の雑処理を実施してもよい）</param>
    /// <param name="webCallback">シーン間移動通信コールバック</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    public static async UniTask SceneTransition(CancellationTokenSource cLTS,
                                                Func<UniTask> sceneTransitionerCollback,
                                                Func<IWebServiceImplementation, IWebServiceImplementation.SceneTransitionReport, UniTask> webCallback = default)
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("1/3　同期通信準備");
        var accountService = UserAccountDomainManager.GetService();
        if (webCallback == default)
        {
            webCallback = async (webService, report) => {
                await webService.PostSceneTransitionReport(report, cLTS.Token);
            };
        }
        Logger.Debug("2/3　同期通信開始");

        IWebServiceImplementation.SceneTransitionReport report = new()
        {
            UserId = accountService.User.UserId,
            UserName = accountService.User.UserName
        };

        await WebConnection(cLTS, async (webService) => {
            await webCallback(webService, report);
        });

        Logger.Debug("3/3　同期通信終了");

        await sceneTransitionerCollback();
    }

    public async static UniTask WebConnection(CancellationTokenSource cLTS,
                                              Func<IWebServiceImplementation, UniTask> webCallback)
    {
        var webService = WebServiceManager.GetWebService();
        try
        {
            await webCallback(webService);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cLTS.Token)
        {
            if (cLTS.IsCancellationRequested)
            {
                // クライアント自体がDisposeされたのでOperationCanceledException、或いは独自の例外を投げる
                throw new OperationCanceledException($"{typeof(HomeSceneDomain)} is disposed.", ex, cLTS.Token);
            }
        }
    }
}
