using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

using Logger = MyLogger.MapBy<TitleScene>;

/// <summary>
/// タイトルシーンの内ロジックレイヤーシーン
/// 
/// クラス名はシーンオブジェクトと同一名にする
/// </summary>
[PageAsset("TitleScenePage.prefab")]
public class TitleScene : MonoBehaviour, ILayeredSceneLogic
{
    TitleSceneDomain _domain;

    [Inject]
    public void Construct(TitleSceneDomain domain)
    {
        _domain = domain;
        Logger.SetEnableLogging(true);
        Logger.Debug("Construct 実体ある？：" + (domain != null));
        _domain.SetLayerScene(this);
    }

    void Start()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        // 非同期メソッド実行
        DelayAsync(ct).Forget();
    }
    // TODOテスト用非同期メソッド
    private async UniTask DelayAsync(CancellationToken token)
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1),
            DelayType.UnscaledDeltaTime,
            PlayerLoopTiming.Update, token);

        Logger.SetEnableLogging(true);
        Logger.Debug("TryStart自動発火");
        await _domain.TryStart();
        Logger.Debug("TryStart自動発火 おわり -------------------");
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        Logger.UnloadEnableLogging();
    }

    //async void OnButtonClicked()
    //{
    //    // TODO イベントハンドリングでドメイン呼び出すか
    //    // そもそもドメイン自体がハンドリングするか
    //    //var domain = new TitleSceneDomain();
    //    //await domain.Login();
    //}
}

public class TitleSceneTransition : LayerdSceneTransition<TitleUIScene.CreateParameter>
{
    public TitleSceneTransition()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(TitleScene) },
            { SceneLayer.UI, typeof(TitleUIScene) },
            //{ SceneLayer.Field, typeof(TitleFieldScene) },
        };
    }
}

