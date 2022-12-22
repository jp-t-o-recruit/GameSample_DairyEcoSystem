using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;
using Logger = MyLogger.MapBy<TitleSceneDomain>;

/// <summary>
/// �^�C�g���V�[���\���Ǘ�
/// </summary>
public class TitleSceneDomain : DomainBase<
    TitleScene,
    TitleUIScene,
    TitleFieldScene,
    TitleSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "TitleSceneDomain�X�N���v�g����ݒ�I";
    }

    // IDisposable�ƈ����|���āAClient���̂�Dispose���ꂽ����s���̃��N�G�X�g���I��������悤�ɂ���
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
        Logger.Debug("�N���b�N�I�@OnButtonClicked");
        await Login();
    }

    public async UniTask Login()
    {
        Logger.Debug("1/3�@���O�C���ʐM�J�n");
        string mockAccountString = "���Ɏg�p����Ȃ�";
        UserInfo userInfo = null;

        await DomainCommonService.WebConnection(_cts, async (webService) => {
            userInfo = await webService.PutLogin(mockAccountString, _cts.Token);
        });

        Logger.Debug("2/3�@���O�C���ʐM�I���ƃ��O�C����ԍX�V");
        var userAccountServiceDomain = UserAccountDomainManager.GetService();
        userAccountServiceDomain.LoginUser(userInfo);
        Logger.Debug("3/3�@���O�C���I�� �V�[���J�ڊJ�n");

        GoHomeSceneTransition(userInfo);
    }

    private async void GoHomeSceneTransition(UserInfo userInfo)
    {
        HomeSceneDomain homeDomain = new()
        {
            Param = new HomeSceneDomain.DomainParam() {
                ViewLabel = $"���[�U�[��:{userInfo.UserName}"
            }
        };
        await homeDomain.SceneTransition(_cts);
    }
}