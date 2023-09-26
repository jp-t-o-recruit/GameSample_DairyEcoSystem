using Cysharp.Threading.Tasks;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

using Logger = MyLogger.MapBy<LayeredSceneDefinition>;

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
public interface IDomainParamBase
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
    public void Initialize(CancellationTokenSource cts);

    /// <summary>
    /// 停止時呼び出し
    /// </summary>
    public void Suspend(CancellationTokenSource cts);

    /// <summary>
    /// 再開時呼び出し
    /// </summary>
    public void Resume(CancellationTokenSource cts);
    /// <summary>
    /// 終了時呼び出し
    /// </summary>
    public void Discard(CancellationTokenSource cts);
}
/// <summary>
/// ITimingのデリゲート
/// </summary>
/// <param name="cts"></param>
public delegate void TimingEventHandler(CancellationTokenSource cts);

public interface ISceneDomain : ITiming
{
    IDomainParamBase Param { get; set; }
    IDomainParamBase InitialParam { get; set; }

    string GetSceneName();
    ISceneTransitioner CreateTransitioner();
    UniTask SceneTransition(CancellationTokenSource cts, Action<ISceneTransitioner> editCallback = default);
}

public interface ILayeredSceneDomain: ISceneDomain
{
    ILayeredSceneLogic LogicLayer { get; set; }
    ILayeredSceneUI UILayer { get; set; }
    ILayeredSceneField FieldLayer { get; set; }

    Dictionary<SceneLayer, string> GetLayerNames();
}

/// <summary>
/// ドメインの基盤
/// 
/// 
/// UniTaskで待つ
/// https://baba-s.hatenablog.com/entry/2019/09/11/083000
/// </summary>
/// <typeparam name="TLogic"></typeparam>
/// <typeparam name="TUI"></typeparam>
/// <typeparam name="TField"></typeparam>
/// <typeparam name="TParam"></typeparam>
public abstract class LayeredSceneDomainBase<TLogic, TUI, TField, TParam> : ILayeredSceneDomain
    where TLogic : MonoBehaviour, ILayeredSceneLogic
    where TUI    : MonoBehaviour, ILayeredSceneUI
    where TField : ILayeredSceneField
    where TParam : IDomainParamBase, new()
{
    public IDomainParamBase Param {
        get => _param;
        set { if (value is TParam param) _param = param; }
    }
    protected TParam _param;

    public IDomainParamBase InitialParam  {
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

    //protected List<Button> _buttons;

    protected LayeredSceneDomainBase()
    {
        Param = new TParam();
        InitialParam = new TParam();
    }

    public virtual void Initialize(CancellationTokenSource cts)
    {
        Logger.Debug($"{this.GetType()} Initialize");
        //_buttons = new List<Button>();
    }
    public virtual void Suspend(CancellationTokenSource cts)
    {
        //_buttons.ForEach(b => {
        //    b.pickingMode = PickingMode.Ignore;
        //});
    }
    public virtual void Resume(CancellationTokenSource cts) {
        //_buttons.ForEach(b => {
        //    b.pickingMode = PickingMode.Position;
        //});
    }
    public virtual void Discard(CancellationTokenSource cts) {
        Logger.Debug($"{this.GetType()} Discard");
        //_buttons.Clear();
        //_buttons = null;
        _logicLayer = default;
        _uiLayer = default;
        _fieldLayer = default;
        Param = default;
        InitialParam = default;
    }
    /// <summary>
    /// ロジックシーン（階層シーンのメイン）名を返す
    /// </summary>
    /// <returns>シーン名</returns>
    public string GetSceneName()
    {
        return typeof(TLogic).Name;
    }

    public Dictionary<SceneLayer, string> GetLayerNames()
    {
        return new Dictionary<SceneLayer, string>() {
            { SceneLayer.Logic, typeof(TLogic).Name},
            { SceneLayer.UI, typeof(TUI).Name},
            { SceneLayer.Field, typeof(TField).Name },
        };
    }
    /// <summary>
    /// シーン遷移実施
    /// </summary>
    /// <param name="cts"></param>
    /// <param name="editCallback">遷移処理編集コールバック</param>
    /// <returns></returns>
    public async UniTask SceneTransition(CancellationTokenSource cts = default,
                                         Action<ISceneTransitioner> editCallback = default)
    {
        cts ??= new CancellationTokenSource();
        ISceneTransitioner transitioner = CreateTransitioner(); 

        if (editCallback != default)
        {
            editCallback(transitioner);
        }

        await transitioner.Transition(cts);
    }

    /// <summary>
    /// シーン遷移処理クラスインスタンスの作成
    /// シーン用オブジェクトにアタッチするスクリプトの読み取りをコールバックで設定
    /// </summary>
    /// <returns></returns>
    public virtual ISceneTransitioner CreateTransitioner()
    {
        return new LayeredSceneTransitioner(this, async (outer, cts) => {
            _logicLayer = await outer.GetAsyncComponentByScene<TLogic>(cts);
            _uiLayer    = await outer.GetAsyncComponentByScene<TUI>(cts);
            _fieldLayer = await outer.GetAsyncComponentByScene<TField>(cts);
        });
    }
}




/// <summary>
/// Nullオブジェクト
/// </summary>
public sealed class NullDomain : LayeredSceneDomainBase<
    NullDomain.NullLayeredSceneLogic,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    NullDomain.DomainParam>
{
    public class NullLayeredSceneLogic : MonoBehaviour, ILayeredSceneLogic
    { }
    public class NullLayeredSceneUI : MonoBehaviour,ILayeredSceneUI
    {}
    public class NullLayeredSceneField : ILayeredSceneField
    {}

    public class DomainParam : IDomainParamBase
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