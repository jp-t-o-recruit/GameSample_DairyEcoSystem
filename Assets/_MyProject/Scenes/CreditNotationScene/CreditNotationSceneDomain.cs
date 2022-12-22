using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;

/// <summary>
/// クレジット表記シーン
/// </summary>
public class CreditNotationSceneDomain : LayeredSceneDomainBase<
    CreditNotationScene,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    CreditNotationSceneDomain.DomainParam>
{
    public class DomainParam : IDomainParamBase
    {
        public string ViewLabel = "CreditNotationSceneDomainスクリプトから設定！";
        /// <summary>
        /// 遷移処理オーバーロード編集
        /// </summary>
        public Func<ILayeredSceneDomain, CancellationTokenSource, UniTask> sceneTransitionerCollback = async (domain, cts) => {
            await domain.SceneTransition(cts, transitioner => { transitioner.StackType = SceneStackType.PopTry; });
        };
    }

    private CancellationTokenSource _cts;

    [Inject]
    public CreditNotationSceneDomain()
    {
        _cts = new();
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
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _logicLayer.BackButton.clickable.clicked -= OnNextSceneButtonClicked;
        base.Discard(cts);
    }

    /// <summary>
    /// 戻るボタン押下
    /// </summary>
    private async void OnNextSceneButtonClicked()
    {
        CancellationTokenSource endDomainCts = new();
        await DomainCommonService.SceneTransition(endDomainCts,
            async () => {
                await _initialParam.sceneTransitionerCollback(new TitleSceneDomain(), endDomainCts);
            },
            async (webService, report) => {
                await webService.PostSceneTransitionReport(report, endDomainCts.Token);
            });
    }
}