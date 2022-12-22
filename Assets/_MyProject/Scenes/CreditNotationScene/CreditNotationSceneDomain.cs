using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

/// <summary>
/// �N���W�b�g�\�L�V�[��
/// </summary>
public class CreditNotationSceneDomain : DomainBase<
    CreditNotationScene,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    CreditNotationSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "CreditNotationSceneDomain�X�N���v�g����ݒ�I";
        /// <summary>
        /// �J�ڏ����I�[�o�[���[�h�ҏW
        /// </summary>
        public Func<ILayeredSceneDomain, CancellationTokenSource, UniTask> sceneTransitionerCollback = async (domain, cts) => {
            await domain.SceneTransition(cts, transitioner => { transitioner.StackType = SceneStackType.PopTry; });
        };
    }

    private CancellationTokenSource _clientLifetimeTokenSource;

    [Inject]
    public CreditNotationSceneDomain()
    {
        _clientLifetimeTokenSource = new();
    }

    public override void Initialize(CancellationTokenSource cts)
    {
        base.Initialize(cts);

        _logicLayer.BackButton.visible = true;
        _logicLayer.BackButton.clickable.clicked += OnNextSceneButtonClicked;
        //_buttons.Add(_logicLayer.BackButton);
        // TODO
        //_logicLayer.ViewLabel.text = _initialParam.ViewLabel;
        _logicLayer.ViewLabel.text = CreditNotationSceneParamsSO.Entity.ViewLabel.ToString();

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

        _clientLifetimeTokenSource?.Cancel();
        _clientLifetimeTokenSource?.Dispose();
        _clientLifetimeTokenSource = null;
        _logicLayer.BackButton.clickable.clicked -= OnNextSceneButtonClicked;
    }

    /// <summary>
    /// �߂�{�^������
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        await DomainCommonService.SceneTransition(_clientLifetimeTokenSource,
            async () => {
                await _initialParam.sceneTransitionerCollback(new TitleSceneDomain(), _clientLifetimeTokenSource);
            },
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, _clientLifetimeTokenSource.Token);
            });
    }
}