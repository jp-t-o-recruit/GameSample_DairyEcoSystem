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
        // TODO　ちゃんとシングルトン的に取り込む
        _pageManager = new PageManager();
    }

    async void LoadPage()
    {
        var parameter = new FooPage.CreateParameter() { Bar = "bar" };
        await _pageManager.PushAsync(new FooPage.Transition() { Parameter = parameter }); // 追加
        await _pageManager.Pop(); // ひとつ前に戻る
        await _pageManager.Replace(new FooPage.Transition() { Parameter = parameter }); // 現在のページを破棄して追加する
        await _pageManager.ReplaceAll(new FooPage.Transition() { Parameter = parameter }); // スタックしたページをすべて破棄して追加する

#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
        _pageManager.PushAsync(new FooPage.Transition() { Parameter = parameter }); // 非同期で追加
#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
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