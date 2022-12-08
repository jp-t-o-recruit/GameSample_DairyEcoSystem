using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

/// <summary>
/// �^�C�g���V�[���\���Ǘ�
/// </summary>
public class TitleSceneDomain : DomainBase
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "TitleSceneDomain�X�N���v�g����ݒ�I";
    }

    DomainParam Param;

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
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

        // TODO �݌v�ύX���Ȃ̂ŃV�[���`�F���W�͌ĂׂȂ�
        await ExSceneManager.Instance.Replace(transition);
    }

    internal void SetLayerScene(ILayeredScene scene)
    {
        _layerDef ??= new Dictionary<SceneLayer, ILayeredScene>();
        var layer = ExSceneManager.Instance.GetLayer(scene);
        _layerDef.Add(layer, scene);
    }
}
