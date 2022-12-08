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
        public string ViewLabel = "TitleSceneDomain�X�N���v�g����ݒ�I";
    }

    public override DomainParam CreateParam()
    {
        return new DomainParam();
    }

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
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
        string mockAccountString = "���Ɏg�p����Ȃ�";
        Logger.Debug("1/3�@���O�C���ʐM�J�n");

        UserInfo userInfo = null;
        try
        {
            userInfo = await webService.PutLogin(mockAccountString, _clientLifetimeTokenSource.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == _clientLifetimeTokenSource.Token)
        {
            if (_clientLifetimeTokenSource.IsCancellationRequested)
            {
                // �N���C�A���g���̂�Dispose���ꂽ�̂�OperationCanceledException�A�����͓Ǝ��̗�O�𓊂���
                throw new OperationCanceledException($"{typeof(TitleSceneDomain)} is disposed.", ex, _clientLifetimeTokenSource.Token);
            }
        }

        Logger.Debug("2/3�@���O�C���ʐM�I���ƃ��O�C����ԍX�V");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3�@���O�C���I��");

        // �V�[���J��
        await ChangeScene();
    }

    private async UniTask ChangeScene()
    {
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        var param = new HomeUIScene.CreateParameter() { ViewLabel = $"���[�U�[��:{userAccountServiceDomain.User.userName}" };
        var transition = new HomeSceneTransition() { Parameter = param };
        await ExSceneManager.Instance.Replace(transition);
    }
}
