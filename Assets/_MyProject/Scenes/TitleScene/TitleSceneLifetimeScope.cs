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
        builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

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