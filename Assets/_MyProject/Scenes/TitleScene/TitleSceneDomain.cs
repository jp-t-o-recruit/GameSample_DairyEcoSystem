using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

// �Ƃ肠�������쌟�ؗp�ɂ����ɏ���
public class TitleFieldScene : ILayeredSceneField
{

}

/// <summary>
/// �^�C�g���V�[���\���Ǘ�
/// </summary>
public class TitleSceneDomain : DomainBase<
    TitleScene,
    TitleUIScene,
    TitleFieldScene,
    TitleSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "TitleSceneDomain�X�N���v�g����ݒ�I";
    }

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
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
        Logger.Debug("1/3�@���O�C���ʐM�J�n");
        string mockAccountString = "���Ɏg�p����Ȃ�";
        UserInfo userInfo = null;

        await DomainCommonService.WebConnection(_clientLifetimeTokenSource, async (webService) => {
            userInfo = await webService.PutLogin(mockAccountString, _clientLifetimeTokenSource.Token);
        });

        Logger.Debug("2/3�@���O�C���ʐM�I���ƃ��O�C����ԍX�V");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3�@���O�C���I�� �V�[���J�ڊJ�n");

        var tran = GetHomeSceneTransitioner(userInfo);
        await tran.Transition();
    }

    private HomeSceneTransitioner GetHomeSceneTransitioner(UserInfo userInfo)
    {
        var param = new HomeSceneDomain.DomainParam() { ViewLabel = $"���[�U�[��:{userInfo.UserName}" };
        return new HomeSceneTransitioner() { NextRelation = SceneRelation.Free, Parameter = param };
    }
}