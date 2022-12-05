using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using UnityEngine;
using VContainer;
using VContainer.Unity;



[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PageAssetAttribute : Attribute
{
    public string PrefabName { get; }

    public PageAssetAttribute(string prefabName)
    {
        this.PrefabName = prefabName;
    }
}

[PageAsset("BarPage.prefab")]
public class BarPage : BasePage
{
    public class Transition : BasePageTransition<BarPage> { }
    PageManager _pageManager;
    BarPage()
    {
        // TODO�@�����ƃV���O���g���I�Ɏ�荞��
        _pageManager = new PageManager();
    }

    async void LoadPage()
    {
        var parameter = new FooPage.CreateParameter() { Bar = "bar" };
        await _pageManager.PushAsync(new FooPage.Transition() { Parameter = parameter }); // �ǉ�
        await _pageManager.Pop(); // �ЂƂO�ɖ߂�
        await _pageManager.Replace(new FooPage.Transition() { Parameter = parameter }); // ���݂̃y�[�W��j�����Ēǉ�����
        await _pageManager.ReplaceAll(new FooPage.Transition() { Parameter = parameter }); // �X�^�b�N�����y�[�W�����ׂĔj�����Ēǉ�����

#pragma warning disable CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
        _pageManager.PushAsync(new FooPage.Transition() { Parameter = parameter }); // �񓯊��Œǉ�
#pragma warning restore CS4014 // ���̌Ăяo���͑ҋ@����Ȃ��������߁A���݂̃��\�b�h�̎��s�͌Ăяo���̊�����҂����ɑ��s����܂�
    }
}

[PageAsset("FooPage.prefab")]
public class FooPage : BasePage<FooPage.CreateParameter>
{
    public class CreateParameter
    {
        public string Bar;
    }
    public class Transition : BasePageTransition<FooPage, CreateParameter> { }
}
public class FooLifetimeScope : LifetimeScopeWithParameter<FooPage.CreateParameter>
{
    //[SerializeField] private FooView _fooView;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.Register<FooPage>(Lifetime.Scoped);
        // TODO
        //builder.Register<FooPressenter>(Lifetime.Scoped);
        //builder.Register<FooModel>(Lifetime.Scoped);
        //builder.RegisterComponent(_fooView);
    }
}