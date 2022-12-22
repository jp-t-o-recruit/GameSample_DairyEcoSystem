using Cysharp.Threading.Tasks;
using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Logger = MyLogger.MapBy<ISceneTransitioner>;

/// <summary>
/// 採用したシーン設計の基本構造
/// https://gamebiz.jp/news/218949
/// </summary>


/// <summary>
/// シーン遷移情報
/// </summary>
public interface ISceneTransitioner : ITiming
{
    /// <summary>
    /// 遷移でのシーンスタックタイプ
    /// </summary>
    public SceneStackType StackType { get; set; }

    /// <summary>
    /// シーン実体ロード
    /// </summary>
    /// <returns></returns>
    public UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// シーン実体アンロード
    /// </summary>
    /// <returns></returns>
    public UniTask UnLoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// このシーン遷移情報に基づきシーン遷移実施
    /// </summary>
    /// <returns></returns>
    public UniTask Transition(CancellationTokenSource cts);
    /// <summary>
    /// シーンの名前取得
    /// </summary>
    /// <returns></returns>
    public string GetSceneName();
}

public class LayerList
{
    private Dictionary<SceneLayer, string> _layer;

    public LayerList()
    {
        _layer = new Dictionary<SceneLayer, string>();
    }

    public LayerList TryAdd<TType>(SceneLayer layer)
    {
        var type = typeof(TType);

        var errorMatch = new Dictionary<SceneLayer, Type>(){
            { SceneLayer.Logic, typeof(ILayeredSceneLogic)},
            { SceneLayer.UI, typeof(ILayeredSceneUI)},
            { SceneLayer.Field, typeof(ILayeredSceneField)},
        }.FirstOrDefault(pair => {
            return pair.Key == layer && !type.IsSubclassOf(pair.Value);
        });

        if (errorMatch.Value != default)
        {
            throw new ArgumentException($"{type.Name}は指定されたレイヤーとしての条件:{errorMatch.Value.Name}を継承していません");
        }
        else
        {
            _layer.Add(layer, type.Name);
        }
            
        return this;
    }
}


public class LayerdSceneTransitioner : ISceneTransitioner
{
    public Dictionary<SceneLayer, System.Type> _layer;
    public SceneStackType StackType { get; set; }

    /// <summary>
    /// 初期化時ハンドラ
    /// </summary>
    public event TimingEventHandler InitializeHandler;
    /// <summary>
    /// 一時停止時ハンドラ
    /// </summary>
    public event TimingEventHandler SuspendHandler;
    /// <summary>
    /// 再開時ハンドラ
    /// </summary>
    public event TimingEventHandler ResumeHandler;
    /// <summary>
    /// 終了時ハンドラ
    /// </summary>
    public event TimingEventHandler DiscardHandler;


    /// <summary>
    /// TODO　このシーンまとまりが複数回ロードされても良いか
    /// 
    /// ItemDetailScene
    /// ┗BattleSelectScene
    /// 　┗ItemDetailScene
    /// 　 のように複数回ロードを許したら実体シーンを取得や参照するのがかなり難しくなる
    /// </summary>
    //public bool CanMultipleLoad = false;

    //protected ILayeredSceneDomain _domain;

    //[Inject]
    //public LayerdSceneTransitioner(ILayeredSceneDomain domain = default)
    //{
    //    _domain = domain ?? NullDomain.Create();
    //    SetupLayer();
    //}

    //public void SetupLayer()
    //{
    //    _layer = _domain.GetLayerMap();
    //}
    [Inject]
    public LayerdSceneTransitioner(Dictionary<SceneLayer, System.Type> layer = default)
    {
        _layer = layer ?? new Dictionary<SceneLayer, System.Type>();
        StackType = SceneStackType.Replace;
    }

    public async virtual UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts)
    {
        List<Scene> scenes = new();
        Logger.SetEnableLogging(true);
        SceneBase waitBySceneBase = default;

        await LoadScene<ILayeredSceneLogic>(cts, SceneLayer.Logic, (scene, sceneBase) =>
        {
            scenes.Add(scene);
            //    _domain.LogicLayer = sceneBase;
        });

        await LoadScene<ILayeredSceneUI>(cts, SceneLayer.UI, (UIScene, sceneBase) => {
            scenes.Add(UIScene);
            //    _domain.UILayer = sceneBase;
            waitBySceneBase = (SceneBase)sceneBase;
        });

        await LoadScene<ILayeredSceneField>(cts, SceneLayer.Field, (scene, sceneBase) => {
            scenes.Add(scene);
            //_domain.FieldLayer = sceneBase;
        });

        //TODOここでSceneBaseとDomainを紐づけ無いといけない？
        LayerList hoge = new LayerList();
        hoge.TryAdd<ILayeredSceneLogic>(SceneLayer.Logic);
        hoge.TryAdd<ILayeredSceneUI>(SceneLayer.UI);
        hoge.TryAdd<ILayeredSceneField>(SceneLayer.Field);

        // プレイヤーループを挟まないとアタッチがされず参照不全なので待つ
        await UniTask.WaitForEndOfFrame(waitBySceneBase);
        Initialize(cts);

        return scenes;
    }

    protected async UniTask<Scene> LoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Logger.Debug($"LoadSceneByName {sceneName} isLoaded:{scene.isLoaded},isDirty:{scene.isDirty},IsValid:{scene.IsValid()}");
        if (!scene.IsValid())
        {
            Logger.Debug($"LoadSceneByName {sceneName} 読み込み開始");
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).WithCancellation(cts.Token);
            scene = SceneManager.GetSceneByName(sceneName);
            Logger.Debug($"LoadSceneByName {sceneName} 読み込み終了? : " + scene.isLoaded);
        }
        return scene;
    }
    private async UniTask LoadScene<TComponent>(CancellationTokenSource cts,SceneLayer layer, Action<Scene, TComponent> callBack)
    {
        if (!_layer.ContainsKey(layer))
        {
            return;
        }

        string sceneName = _layer[layer].ToString();
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        Scene scene = await LoadSceneByName(sceneName, cts);
        if (scene.IsValid())
        {
            callBack(scene,
                     GetSceneBaseFromScene<TComponent>(scene));
        }
    }
    public async virtual UniTask UnLoadScenes(CancellationTokenSource cts)
    {
        foreach (System.Type types in _layer.Values)
        {
            await UnLoadSceneByName(types.ToString(), cts);
        }
    }

    private async UniTask UnLoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Logger.Debug("アンロード開始:" + sceneName);
        await SceneManager.UnloadSceneAsync(sceneName).WithCancellation(cts.Token);
        await Resources.UnloadUnusedAssets().WithCancellation(cts.Token);
        Logger.Debug("アンロード終了:" + sceneName);
    }

    /// <summary>
    /// シーンクラス（スクリプト）にパラメータをアタッチする
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public TComponent GetSceneBaseFromScene<TComponent>(Scene scene)
    {
        Logger.Debug("GetSceneBase取得開始:" + scene.name);
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

        Logger.Debug("GetSceneBase取得終了:" + scene.name);

        return component;
    }
    public async UniTask Transition(CancellationTokenSource cts)
    {
        await ExSceneManager.Instance.Transition(this, cts);

        // TODO
        //if (callback != default)
        //{
        //    var logic = GetSceneBaseFromScene<ILayeredSceneLogic>(_layer[SceneLayer.Logic].ToString());
        //    var ui = GetSceneBaseFromScene<ILayeredSceneUI>(_layer[SceneLayer.UI].ToString());
        //    var field = GetSceneBaseFromScene<ILayeredSceneField>(_layer[SceneLayer.Field].ToString());
        //    var list = new Dictionary<SceneLayer, ILayeredScene>(){
        //        { SceneLayer.Logic, logic },
        //        { SceneLayer.UI, ui },
        //        { SceneLayer.Field, field },
        //    };
        //    callback(list);
        //}
    }

    private TComponent GetSceneBaseFromScene<TComponent>(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return GetSceneBaseFromScene<TComponent>(scene);
    }

    public string GetSceneName()
    {
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        return logicSceneName;
    }

    /// <summary>
    /// 開始時呼び出し
    /// </summary>
    public void Initialize(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && InitializeHandler != null)
        {
            InitializeHandler(cts);
        }
        // TODO ロード必要？
        //await LoadScenes();
    }
    /// <summary>
    /// 停止時呼び出し
    /// </summary>
    public void Suspend(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && ResumeHandler != null)
        {
            SuspendHandler(cts);
        }
    }
    /// <summary>
    /// 再開時呼び出し
    /// </summary>
    public void Resume(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && ResumeHandler != null)
        {
            ResumeHandler(cts);
        }
    }
    /// <summary>
    /// 終了時呼び出し
    /// </summary>
    public void Discard(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && DiscardHandler != null)
        {
           DiscardHandler(cts);
        } 
        //await _domain.Discard();
    }
}

/// <summary>
/// 階層構造を持たないシーン（ただしLogic層として判定する）
/// </summary>
/// <typeparam name="TParam"></typeparam>
public abstract class SoloLayerSceneTransitioner : LayerdSceneTransitioner
{
}
