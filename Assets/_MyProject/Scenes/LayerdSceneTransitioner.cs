using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;


using Logger = MyLogger.MapBy<ISceneTransitioner>;

/// <summary>
/// 
/// 採用したシーン設計の基本構造
/// https://gamebiz.jp/news/218949
/// </summary>

public enum SceneRelation
{
    /// <summary>
    /// 無し
    /// </summary>
    None,
    /// <summary>
    /// 決まっていない
    /// </summary>
    Free,
    /// <summary>
    /// 破棄できない連結
    /// </summary>
    ChainLink,
    /// <summary>
    /// 破棄できる連結
    /// </summary>
    HookLink,
    /// <summary>
    /// 連結の起点
    /// </summary>
    StartLink,


}
public interface ISceneTransitioner : ITiming
{
    public SceneRelation PrevRelation { get; set; }
    public SceneRelation SelfRelation { get; set; }
    public SceneRelation NextRelation { get; set; }
    public abstract UniTask<List<Scene>> LoadScenes();
    public abstract UniTask UnLoadScenes();
    public UniTask Transition();
    public string GetSceneName();
}


public abstract class LayerdSceneTransitioner<TParam> : ISceneTransitioner where TParam : IDomainBaseParam, new()
{
    public TParam Parameter { get; set; }
    internal protected Dictionary<SceneLayer, System.Type> _layer;

    public SceneRelation PrevRelation { get; set; }
    public SceneRelation SelfRelation { get; set; }
    public SceneRelation NextRelation { get; set; }

    /// <summary>
    /// TODO　このシーンまとまりが複数回ロードされても良いか
    /// 
    /// ItemDetailScene
    /// ┗BattleSelectScene
    /// 　┗ItemDetailScene
    /// 　 のように複数回ロードを許したら実体シーンを取得や参照するのがかなり難しくなる
    /// </summary>
    //public bool CanMultipleLoad = false;

    protected ILayeredSceneDomain _domain;

    protected LayerdSceneTransitioner(ILayeredSceneDomain domain = default)
    {
        _domain = domain ?? NullDomain.Create();
    }

    public async virtual UniTask<List<Scene>> LoadScenes()
    {
        List<Scene> scenes = new();

        await LoadScene<ILayeredSceneLogic>(SceneLayer.Logic, (scene, sceneBase) => {
            scenes.Add(scene);
            _domain.LogicLayer = sceneBase;
        });

        await LoadScene<ILayeredSceneUI>(SceneLayer.UI, (UIScene, sceneBase) => {
            scenes.Add(UIScene);
            _domain.UILayer = sceneBase;
        });

        await LoadScene<ILayeredSceneField>(SceneLayer.Field, (scene, sceneBase) => {
            scenes.Add(scene);
            _domain.FieldLayer = sceneBase;
        });

        PresentParameter();

        return scenes;
    }

    protected async UniTask<Scene> LoadSceneByName(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Logger.Debug($"LoadSceneByName {sceneName} isLoaded:{scene.isLoaded},isDirty:{scene.isDirty},IsValid:{scene.IsValid()}");
        if (!scene.isLoaded)
        {
            Logger.Debug($"LoadSceneByName {sceneName} 読み込み開始");
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            scene = SceneManager.GetSceneByName(sceneName);
            Logger.Debug($"LoadSceneByName {sceneName} 読み込み終了" + scene.isLoaded);
        }
        return scene;
    }
    private async UniTask LoadScene<TComponent>(SceneLayer layer, Action<Scene, TComponent> callBack)
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

        Scene scene = await LoadSceneByName(sceneName);
        if (scene != null && scene.IsValid())
        {
            callBack(scene,
                     GetSceneBaseFromScene<TComponent>(scene));
        }
    }
    public async virtual UniTask UnLoadScenes()
    {
        foreach (System.Type types in _layer.Values)
        {
            await UnLoadSceneByName(types.ToString());
        }
    }

    private async UniTask UnLoadSceneByName(string sceneName)
    {
        Logger.Debug("アンロード開始:" + sceneName);
        await SceneManager.UnloadSceneAsync(sceneName);
        await Resources.UnloadUnusedAssets().ToUniTask();
        Logger.Debug("アンロード終了:" + sceneName);
    }

    /// <summary>
    /// シーンクラス（スクリプト）にパラメータをアタッチする
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    protected TComponent GetSceneBaseFromScene<TComponent>(Scene scene)
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
    public async UniTask Transition()
    {
        await ExSceneManager.Instance.Transition(this);
    }

    public string GetSceneName()
    {
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        return logicSceneName;
    }

    /// <summary>
    /// 開始時呼び出し
    /// </summary>
    public async UniTask Initialize()
    {
        await LoadScene<ILayeredSceneLogic>(SceneLayer.Logic, (scene, sceneBase) => {
            _domain.LogicLayer = sceneBase;
        });
        await LoadScene<ILayeredSceneUI>(SceneLayer.UI, (scene, sceneBase) => {
            _domain.UILayer = sceneBase;
        });
        await LoadScene<ILayeredSceneField>(SceneLayer.Field, (scene, sceneBase) => {
            _domain.FieldLayer = sceneBase;
        });

        PresentParameter();

        await _domain.Initialize();
    }
    /// <summary>
    /// 停止時呼び出し
    /// </summary>
    public async UniTask Suspend()
    {
        await _domain.Suspend();
    }
    /// <summary>
    /// 再開時呼び出し
    /// </summary>
    public async UniTask Resume()
    {
        await _domain.Resume();
    }
    /// <summary>
    /// 終了時呼び出し
    /// </summary>
    public async UniTask Discard()
    {
        await _domain.Discard();
    }
    private void PresentParameter()
    {
        _domain.InitialParam = Parameter ?? new TParam();
        _domain.Param = Parameter ?? new TParam();
    }

    public void SetSceneRelation(SceneRelation next = SceneRelation.Free,
                                 SceneRelation prev = SceneRelation.Free)
    {
        NextRelation = next;
        PrevRelation = prev;
    }
}

/// <summary>
/// 階層構造を持たないシーン（ただしLogic層として判定する）
/// </summary>
/// <typeparam name="TParam"></typeparam>
public abstract class SoloLayerSceneTransitioner<TParam> : LayerdSceneTransitioner<TParam> where TParam : IDomainBaseParam, new()
{
}