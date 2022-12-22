using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
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

public interface ILayeredSceneDomain: ITiming
{
    public IDomainParamBase Param { get; set; }
    public IDomainParamBase InitialParam { get; set; }

    public ILayeredSceneLogic LogicLayer { get; set; }
    public ILayeredSceneUI UILayer { get; set; }
    public ILayeredSceneField FieldLayer { get; set; }

    public Dictionary<SceneLayer, System.Type> GetLayerMap();
    public UniTask<LayerdSceneTransitioner> SceneTransition(CancellationTokenSource cts, Action<LayerdSceneTransitioner> transitionerEditor = default);
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
public abstract class DomainBase<TLogic, TUI, TField, TParam> : ILayeredSceneDomain
    where TLogic : ILayeredSceneLogic
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

    protected LayerdSceneTransitioner Transitioner;

    //protected List<Button> _buttons;

    protected DomainBase()
    {
        Param = new TParam();
        InitialParam = new TParam();
    }

    public virtual void Initialize(CancellationTokenSource cts)
    {
        Logger.Debug($"{this.GetType()} Initialize");
        //_buttons = new List<Button>();
        //await UniTask.WaitForFixedUpdate();
        //await UniTask.Yield();
    }
    public virtual void Suspend(CancellationTokenSource cts)
    {
        //_buttons.ForEach(b => {
        //    b.pickingMode = PickingMode.Ignore;
        //});
        //await UniTask.WaitForFixedUpdate();
        //await UniTask.Yield();
    }
    public virtual void Resume(CancellationTokenSource cts) {
        //_buttons.ForEach(b => {
        //    b.pickingMode = PickingMode.Position;
        //});
        //await UniTask.WaitForFixedUpdate();
        //await UniTask.Yield();
    }
    public virtual void Discard(CancellationTokenSource cts) {
        Logger.Debug($"{this.GetType()} Discard");
        //_buttons.Clear();
        //_buttons = null;
        //await UniTask.Yield();
        _logicLayer = default;
        _uiLayer = default;
        _fieldLayer = default;
        Transitioner.InitializeHandler -= this.Initialize;
        Transitioner.SuspendHandler -= this.Suspend;
        Transitioner.ResumeHandler -= this.Resume;
        Transitioner.DiscardHandler -= this.Discard;
        Transitioner = default;
        Param = default;
        InitialParam = default;
    }

    public Dictionary<SceneLayer, System.Type> GetLayerMap()
    {
        return new Dictionary<SceneLayer, System.Type>() {
            { SceneLayer.Logic, typeof(TLogic)},
            { SceneLayer.UI, typeof(TUI)},
            { SceneLayer.Field, typeof(TField) },
        };
    }
    /// <summary>
    /// シーン遷移実施
    /// </summary>
    /// <param name="cts"></param>
    /// <param name="transitionerEditor">遷移処理編集コールバック</param>
    /// <returns></returns>
    public async UniTask<LayerdSceneTransitioner> SceneTransition(CancellationTokenSource cts,
                                                                  Action<LayerdSceneTransitioner> transitionerEditor = default)
    {
        Transitioner = new (this.GetLayerMap());
        Transitioner.InitializeHandler += this.Initialize;
        Transitioner.SuspendHandler += this.Suspend;
        Transitioner.ResumeHandler += this.Resume;
        Transitioner.DiscardHandler += this.Discard;

        if (transitionerEditor != default)
        {
            transitionerEditor(Transitioner);
        }
        await Transitioner.Transition(cts);
        //, (list) =>
        //{
        //    LogicLayer = list[SceneLayer.Logic];
        //    UILayer = list[SceneLayer.UI];
        //    FieldLayer = list[SceneLayer.Field];
        //});

        return Transitioner;
    }
    // TODO 疎結合的によろしくない
    public void SetupSceneBase()
    {
        _logicLayer = GetSceneBaseFromScene<TLogic>();
        _uiLayer = GetSceneBaseFromScene<TUI>();
        _fieldLayer = GetSceneBaseFromScene<TField>();
    }

    /// <summary>
    /// シーンクラス（スクリプト）にパラメータをアタッチする
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private TComponent GetSceneBaseFromScene<TComponent>()
    {
        Scene scene = SceneManager.GetSceneByName($"{typeof(TComponent)}");
        TComponent component = default;

        // GetRootGameObjectsで、そのシーンのルートGameObjects
        // つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            component = gameObject.GetComponent<TComponent>();
            if (component != null)
            {
                break;
            }
        }

        return component;
    }
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