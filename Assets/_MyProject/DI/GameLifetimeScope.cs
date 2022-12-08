using System;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// DI�R���e�i�܂Ƃ�
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] public TitleView titleView;

    //[SerializeField] public TitleScene titleScene;
    //[SerializeField] public TitleUIScene titleUIScene;
    


    protected override void Configure(IContainerBuilder builder)
    {
        // ������DI�R���e�i�ɂǂ̃C���X�^���X���ǂ̌^�ŕۑ����Ă�����������

        // �C���X�^���X�𒍓�����N���X���w�肷��
        builder.RegisterEntryPoint<TitleScenePresenter>(Lifetime.Singleton);

        // TitleModel�̃C���X�^���X��ITitleModel�̌^��DI�R���e�i�ɓo�^����
        builder.Register<ITitleModel, TitleModel>(Lifetime.Singleton);

        // MonoBehavior���p�����Ă���N���X�͂��̂悤��DI�R���e�i�ɓo�^����
        // builder.RegisterComponentInHierarchy<TitleView>(); �ƋL�q����ƃq�G�����L�[����T���Ă��Ă����
        builder.RegisterComponent(titleView);

        
        //builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        //builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        //builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

        ////_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();

        //builder.RegisterComponent(titleScene);
        //builder.RegisterComponent(titleUIScene);
    }
}