using System;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// DIコンテナまとめ
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] public TitleView titleView;

    //[SerializeField] public TitleScene titleScene;
    //[SerializeField] public TitleUIScene titleUIScene;
    


    protected override void Configure(IContainerBuilder builder)
    {
        // ここでDIコンテナにどのインスタンスをどの型で保存しておくかを書く

        // インスタンスを注入するクラスを指定する
        builder.RegisterEntryPoint<TitleScenePresenter>(Lifetime.Singleton);

        // TitleModelのインスタンスをITitleModelの型でDIコンテナに登録する
        builder.Register<ITitleModel, TitleModel>(Lifetime.Singleton);

        // MonoBehaviorを継承しているクラスはこのようにDIコンテナに登録する
        // builder.RegisterComponentInHierarchy<TitleView>(); と記述するとヒエラルキーから探してきてくれる
        builder.RegisterComponent(titleView);

        
        //builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        //builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        //builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

        ////_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();

        //builder.RegisterComponent(titleScene);
        //builder.RegisterComponent(titleUIScene);
    }
}