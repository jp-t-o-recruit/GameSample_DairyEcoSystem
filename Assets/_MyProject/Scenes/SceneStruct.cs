using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// 1シーン内の階層割り当てシーン
/// </summary>
public enum SceneLayer
{
    Logic, // AudioListenerやCanvasなど集約はここにする
    UI,
    Field,
}


/// <summary>
/// SceneLayer対応階層基盤
/// 階層割り当てシーンの階層情報をインターフェースとして持たせる
/// </summary>
public interface ILayeredScene { }

/// <summary>
/// SceneLayer非対応階層（常駐シーン）
/// </summary>
public interface ILayeredSceneResident : ILayeredScene { } 

/// <summary>
/// SceneLayer対応階層 Logic
/// </summary>
public interface ILayeredSceneLogic : ILayeredScene { }
/// <summary>
/// SceneLayer対応階層 UI
/// </summary>
public interface ILayeredSceneUI : ILayeredScene { }
/// <summary>
/// SceneLayer対応階層 Field 
/// </summary>
public interface ILayeredSceneField : ILayeredScene { }

// TODO無参照
/// <summary>
/// 階層割り当て構成したシーンスクリプトのまとまり
/// </summary>
public class LayeredSceneDefinition
{
    private Dictionary<SceneLayer, MonoBehaviour> _layers;

    public class NullScene: MonoBehaviour
    {
    }

    [Inject]
    public LayeredSceneDefinition(Dictionary<SceneLayer, MonoBehaviour> layers)
    {
        _layers = layers;
        FillEmptyLayeredSceneForNullScene();
    }


    public MonoBehaviour GetSceneByLayer(SceneLayer layer)
    {
        return _layers[layer];
    }

    public void FillEmptyLayeredSceneForNullScene()
    {
        foreach (SceneLayer current in Enum.GetValues(typeof(SceneLayer)))
        {
            _layers[current] ??= new NullScene();
        }
    }
}


/// <summary>
/// 特に機能を持たないが型判別のためのインターフェース
/// </summary>
public interface IDomainBaseParam
{
}

/// <summary>
/// 自前のサイクルタイミング処理
/// </summary>
public interface ITiming
{
    /// <summary>
    /// 開始時呼び出し
    /// </summary>
    public UniTask Initialize();

    /// <summary>
    /// 停止時呼び出し
    /// </summary>
    public UniTask Suspend();

    /// <summary>
    /// 再開時呼び出し
    /// </summary>
    public UniTask Resume();
    /// <summary>
    /// 終了時呼び出し
    /// </summary>
    public UniTask Discard();
}

public interface ILayeredSceneDomain: ITiming
{
    public IDomainBaseParam Param { get; set; }
    public IDomainBaseParam InitialParam { get; set; }

    public ILayeredSceneLogic LogicLayer { get; set; }
    public ILayeredSceneUI UILayer { get; set; }
    public ILayeredSceneField FieldLayer { get; set; }
}

public abstract class DomainBase<TLogic, TUI, TField, TParam> : ILayeredSceneDomain
    where TLogic : ILayeredSceneLogic
    where TUI    : ILayeredSceneUI
    where TField : ILayeredSceneField
    where TParam : IDomainBaseParam
{

    public IDomainBaseParam Param {
        get => _param;
        set { if (value is TParam param) _param = param; }
    }
    protected TParam _param;

    public IDomainBaseParam InitialParam  {
        get => _initialParam;
        set { if (value is TParam param) _initialParam = param; }
    }
    protected TParam _initialParam;

    public ILayeredSceneLogic LogicLayer {
        get => _logicLayer;
        set { if (value is TLogic param) _logicLayer = param; }
    }
    protected TLogic _logicLayer;

    public ILayeredSceneUI UILayer {
        get => _uiLayer;
        set { if (value is TUI param) _uiLayer = param; }
    }
    public TUI _uiLayer;

    public ILayeredSceneField FieldLayer {
        get => _fieldLayer;
        set { if (value is TField param) _fieldLayer = param; }
    }
    protected TField _fieldLayer;

    public virtual UniTask Initialize() { return UniTask.Delay(0); }
    public virtual UniTask Suspend() { return UniTask.Delay(0); }
    public virtual UniTask Resume() { return UniTask.Delay(0); }
    public virtual UniTask Discard() { return UniTask.Delay(0); }
}




/// <summary>
/// Nullオブジェクト
/// </summary>
public sealed class NullDomain : DomainBase<
    NullDomain.NullLayeredSceneLogic,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    NullDomain.DomainParam>
{
    public class NullLayeredSceneLogic : ILayeredSceneLogic
    {}
    public class NullLayeredSceneUI : ILayeredSceneUI
    {}
    public class NullLayeredSceneField : ILayeredSceneField
    {}

    public class DomainParam : IDomainBaseParam
    {}
    
    private NullDomain()
    {
        LogicLayer = new NullLayeredSceneLogic();
        UILayer = new NullLayeredSceneUI();
        FieldLayer = new NullLayeredSceneField();
    }

    public static NullDomain Create()
    {
        return new NullDomain();
    }
}