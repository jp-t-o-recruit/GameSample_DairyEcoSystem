using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ルートDIコンテナ
/// 
/// DIコンテナまとめ
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    //[SerializeField] public GameManagerView titleView;

    //[SerializeField] public TitleScene titleScene;
    //[SerializeField] public TitleUIScene titleUIScene;
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);


        if (EditorApplication.isPlaying)
        {
            //ExSceneManager.Instance.NoticeDefaultTransition(() => new TitleSceneTransitioner());
        }

        // ここでDIコンテナにどのインスタンスをどの型で保存しておくかを書く

        // インスタンスを注入するクラスを指定する
        //builder.RegisterEntryPoint<TitleScenePresenter>(Lifetime.Singleton);

        // TitleModelのインスタンスをITitleModelの型でDIコンテナに登録する
        //builder.Register<ITitleModel, TitleModel>(Lifetime.Singleton);

        // MonoBehaviorを継承しているクラスはこのようにDIコンテナに登録する
        // builder.RegisterComponentInHierarchy<TitleView>(); と記述するとヒエラルキーから探してきてくれる
        //builder.RegisterComponent(titleView);


        //builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        //builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        //builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

        ////_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();

        //builder.RegisterComponent(titleScene);
        //builder.RegisterComponent(titleUIScene);
    }
}



public interface ITitleModel
{
    public string GetRandomText();
}
public class TitleModel : ITitleModel
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
    private readonly GameManagerView _view;
    private readonly ITitleModel _model;

    [Inject]
    public TitleScenePresenter(GameManagerView view, ITitleModel model)
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