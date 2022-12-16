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
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "CreditNotationSceneDomain�X�N���v�g����ݒ�I";
        public Func<ISceneTransitioner> sceneTransitionerCollback = () =>
        {
            return new HomeSceneTransitioner() { NextRelation = SceneRelation.Free };
        };
    }

    private CancellationTokenSource _clientLifetimeTokenSource;

    [Inject]
    public CreditNotationSceneDomain()
    {
        _clientLifetimeTokenSource = new();
    }

    public override async UniTask Initialize()
    {
        await base.Initialize();

        _logicLayer.BackButton.clickable.clicked += OnNextSceneButtonClicked;
        // TODO
        //_logicLayer.ViewLabel.text = _initialParam.ViewLabel;
        _logicLayer.ViewLabel.text = CreditNotationSceneParamsSO.Entity.ViewLabel.ToString();
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
        _clientLifetimeTokenSource = null;
        _logicLayer.BackButton.clickable.clicked -= OnNextSceneButtonClicked;
    }

    /// <summary>
    /// �߂�{�^������
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        // TODO
        // �X�g�b�N����Ă���Ȃ�e�ɖ߂�iPop�j
        // �w��悪����΂����ɑJ�ځiPushuAsync�j
        // �ǂ���̏󋵂ł��Ȃ���΃f�t�H���g��Home�ɑJ��(Replace)

        await DomainCommonService.SceneTransition(_clientLifetimeTokenSource,
            async () => {
                var transition = _initialParam.sceneTransitionerCollback();
                await transition.Transition();
            },
            async (webService, report) =>
            {
                await webService.PostSceneTransitionReport(report, _clientLifetimeTokenSource.Token);
            });
    }
}