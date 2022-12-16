
using Cysharp.Threading.Tasks;
using System;
using System.Threading;


using Logger = MyLogger.MapBy<DomainCommonService>;

public class DomainCommonService
{
    /// <summary>
    /// �V�[���Ԉړ�
    /// </summary>
    /// <param name="cLTS"></param>
    /// <param name="sceneTransitionerCollback">�ʐM�I����V�[���ړ����̂�Ԃ��R�[���o�b�N�ireturn�O�ɒʐM��̎G���������{���Ă��悢�j</param>
    /// <param name="webCallback">�V�[���Ԉړ��ʐM�R�[���o�b�N</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException"></exception>
    public static async UniTask SceneTransition(CancellationTokenSource cLTS,
                                                Func<UniTask> sceneTransitionerCollback,
                                                Func<IWebServiceImplementation, IWebServiceImplementation.SceneTransitionReport, UniTask> webCallback = default)
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("1/3�@�����ʐM����");
        var accountService = UserAccountDomainManager.GetService();
        if (webCallback == default)
        {
            webCallback = async (webService, report) => {
                await webService.PostSceneTransitionReport(report, cLTS.Token);
            };
        }
        Logger.Debug("2/3�@�����ʐM�J�n");

        IWebServiceImplementation.SceneTransitionReport report = new()
        {
            UserId = accountService.User.UserId,
            UserName = accountService.User.UserName
        };

        await WebConnection(cLTS, async (webService) => {
            await webCallback(webService, report);
        });

        Logger.Debug("3/3�@�����ʐM�I��");

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
                // �N���C�A���g���̂�Dispose���ꂽ�̂�OperationCanceledException�A�����͓Ǝ��̗�O�𓊂���
                throw new OperationCanceledException($"{typeof(HomeSceneDomain)} is disposed.", ex, cLTS.Token);
            }
        }
    }
}
