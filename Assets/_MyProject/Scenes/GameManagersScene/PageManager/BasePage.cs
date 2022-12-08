using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;
using System;

public abstract class BasePage
{
    //abstract ����Ȃ���virtual���������ǃG���[�o�Ă�
    // TODO virtual ����Ȃ��ɂ��낿���Ǝ���
    public virtual UniTask Initialize() { return UniTask.Delay(1); }
    public virtual UniTask Suspend() { return UniTask.Delay(1); }
    public virtual UniTask Resume() { return UniTask.Delay(1); }
    public virtual UniTask Discard() { return UniTask.Delay(1); }
}

public abstract class BasePage<TParam> : BasePage
{
    protected TParam Parameter { get; }
}

public interface IPageTransition
{
    public abstract UniTask<BasePage> LoadPage();
}

/// <summary>
/// �y�[�W�̌Ăяo������m���Ă�z
/// </summary>
/// <typeparam name="TPage"></typeparam>
public class BasePageTransition<TPage> : IPageTransition where TPage : BasePage
{
    GameObject _pagePrefab;

    UniTask<GameObject> DoPrefabLoad(string n)
    {
        // TODO
        var utcs = new UniTaskCompletionSource<GameObject>();
        utcs.TrySetResult(new GameObject());

        //Instantiate(_pagePrefab, pos, Quaternion.identity);

        return utcs.Task;
    }

    public virtual async UniTask<BasePage> LoadPage()
    {
        // pageAssetName���ǂ��n�����˃J�X�^���A�g���r���[�g�Œ��ڕR�Â��Ă��܂�
        //var pageInstance = await DoPrefabLoad(pageAssetName);
        var pageNameAttr = Attribute.GetCustomAttribute(typeof(TPage),
            typeof(PageAssetAttribute)) as PageAssetAttribute;
        //TODO
        var pageInstance = await DoPrefabLoad(pageNameAttr.PrefabName);
        //var pageInstance = new GameObject();


        var pageLifetimeScope = pageInstance.GetComponent<LifetimeScope>();
        InitializePageParameter(pageLifetimeScope);
        pageLifetimeScope.Build(); // Page���̈ˑ��֌W����
        var page = pageLifetimeScope.Container.Resolve<TPage>();
        return page;
    }
    protected virtual void InitializePageParameter(LifetimeScope scope) { }
}

public class LifetimeScopeWithParameter<TParam> : LifetimeScope
{
    public TParam Param;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(Param);
    }
}

public class BasePageTransition<TPage, TParam> : BasePageTransition<TPage> where TPage : BasePage<TParam>
{
    public TParam Parameter { get; set; }

    protected override void InitializePageParameter(LifetimeScope scope)
    {
        if (scope is LifetimeScopeWithParameter<TParam> s)
        {
            s.Param = Parameter;
        }
    }
}
