using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

/// <summary>
/// クレジット表記シーン
/// </summary>
public class CreditNotationSceneDomain : DomainBase<
    CreditNotationScene,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    CreditNotationSceneDomain.DomainParam>
{
    public class DomainParam : IDomainBaseParam
    {
        public string ViewLabel = "CreditNotationSceneDomainスクリプトから設定！";
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
    /// 戻るボタン押下
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        // TODO
        // ストックされているなら親に戻る（Pop）
        // 指定先があればそこに遷移（PushuAsync）
        // どちらの状況でもなければデフォルトでHomeに遷移(Replace)

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