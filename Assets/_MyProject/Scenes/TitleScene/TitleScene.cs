using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

using LocalLogger = MyLogger.MapBy<TitleScene>;

/// <summary>
/// タイトルシーンの内ロジックレイヤーシーン
/// 
/// クラス名はシーンオブジェクトと同一名にする
/// </summary>
[PageAsset("TitleScenePage.prefab")]
public class TitleScene : MonoBehaviour
{
    TitleSceneDomain _domain;

    [Inject]
    public TitleScene(TitleSceneDomain domain)
    {
        _domain = domain;
        //_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();
    }

    void Start()
    {
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        LocalLogger.UnloadEnableLogging();
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
        SceneName = _layer[SceneLayer.Logic].ToString();
    }
}


public interface ITitleModel
{
    public string GetRandomText();
}
public class TitleModel: ITitleModel
{

    public string GetRandomText()
    {
        return "123";
    }
}

public class TitleModelMock : ITitleModel
{
    public string GetRandomText()
        => "Test";
}

// Presenterクラス
public class TitleScenePresenter : IStartable
{
    // TODO シーン変更でOnDestroy（Disposable）されるのか？
    private readonly TitleView _view;
    private readonly ITitleModel _model;

    [Inject]
    public TitleScenePresenter(TitleView view, ITitleModel model)
    {
        _view = view;
        _model = model;
    }

    public void Start()
    {
        // MonoBehaviorのStartメソッドが呼ばれるタイミングで実行される(IStartableのおかげ)
        var text = _model.GetRandomText();
        _view.DrawText(text);
    }
}