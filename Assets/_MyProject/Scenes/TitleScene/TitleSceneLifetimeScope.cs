using System.Threading;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TitleSceneLifetimeScope : LifetimeScope
{
    [SerializeField] public TitleScene titleScene;
    [SerializeField] public TitleUIScene titleUIScene;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);

        //builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        //builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        //builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

        //builder.Register<CancellationTokenSource>(Lifetime.Transient).AsImplementedInterfaces().AsSelf();
        //builder.Register<TitleSceneDomain>(Lifetime.Singleton);
        //var cts = new CancellationTokenSource();
        //builder.RegisterInstance(cts);
        //var domain = new TitleSceneDomain(cts);
        //builder.RegisterInstance(domain);

        //builder.RegisterComponentInHierarchy<TitleScene>();

        if (null != titleScene)
        {
            builder.RegisterComponent(titleScene);
        }
        if (null != titleUIScene)
        {
            builder.RegisterComponent(titleUIScene);
        }
    }
}