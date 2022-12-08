using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO現状廃棄予定

/// <summary>
/// 特に機能を持たないが型判別のためのインターフェース
/// </summary>
public interface IDomain<TParam> where TParam : new()
{
    public TParam Param { get; set; }
    public TParam InitialParam { get; set; }
    public TParam CreateParam();
}

/// <summary>
/// 特に機能を持たないが型判別のためのインターフェース
/// </summary>
public interface IDomain2
{
    public IDomainBaseParam Param { get; set; }
    public IDomainBaseParam InitialParam { get; set; }
    public IDomainBaseParam CreateParam();
}

public abstract class DomainBase2 : IDomain2
{
    public virtual IDomainBaseParam Param { get; set; }
    public virtual IDomainBaseParam InitialParam { get; set; }

    public virtual IDomainBaseParam CreateParam()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 特に機能を持たないが型判別のためのインターフェース
/// </summary>
public interface IDomainBaseParam
{
}

//public abstract class DomainBaseParamBase : IDomainBaseParam
//{

//}

public abstract class DomainBase<TParam> : IDomain<TParam> where TParam: new()
{
    public TParam Param { get; set; }
    public TParam InitialParam { get; set; }

    public abstract TParam CreateParam();
}

/// <summary>
/// TODO 現在未使用
/// </summary>
public class DomainManager
{
    /// <summary>
    /// ログ出力有効の型をキーにしたマッピング
    /// </summary>
    //protected Dictionary<Type, DomainBase<IDomainBaseParam>> Domains
    //{
    //    get
    //    {
    //        if (null == _domains)
    //        {
    //            _domains = new Dictionary<Type, DomainBase<IDomainBaseParam>>();
    //        }
    //        return _domains;
    //    }
    //}
    //private static Dictionary<Type, object>  _d12omains;
    private static Dictionary<Type, object> _d12omains;

    public static TDomain GetDomain<TDomain, TParam>()
        where TDomain : DomainBase<TParam>, new()
        where TParam : new()
    {
        TDomain domain = new();
        domain.Param = domain.CreateParam();
        return domain;
    }
}
