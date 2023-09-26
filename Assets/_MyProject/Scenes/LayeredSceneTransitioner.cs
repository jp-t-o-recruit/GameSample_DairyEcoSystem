using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
public interface ISceneTransitioner: IDisposable
{
    /// <summary>
    /// 遷移でのシーンスタックタイプ
    /// </summary>
    SceneStackType StackType { get; set; }
    ILayeredSceneDomain Domain { get; set; }

    /// <summary>
    /// シーン実体ロード
    /// </summary>
    /// <returns></returns>
    UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// シーン実体アンロード
    /// </summary>
    /// <returns></returns>
    UniTask UnLoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// このシーン遷移情報に基づきシーン遷移実施
    /// </summary>
    /// <returns></returns>
    UniTask Transition(CancellationTokenSource cts);
    /// <summary>
    /// シーンの名前取得
    /// </summary>
    /// <returns></returns>
    string GetSceneName();
}

/// <summary>
/// 階層構造シーン遷移処理
/// </summary>
public class LayeredSceneTransitioner : ISceneTransitioner
{
    public SceneStackType StackType { get; set; }
    public ILayeredSceneDomain Domain { get; set; }

    /// <summary>
    /// TODO　このシーンまとまりが複数回ロードされても良いか
    /// 
    /// ItemDetailScene
    /// ┗BattleSelectScene
    /// 　┗ItemDetailScene
    /// 　 のように複数回ロードを許したら実体シーンを取得や参照するのがかなり難しくなる
    /// </summary>
    //public bool CanMultipleLoad = false;

    Func<OuterLoader, CancellationTokenSource, UniTask> _preSetCallback;


    [Inject]
    public LayeredSceneTransitioner(ILayeredSceneDomain domain, Func<OuterLoader, CancellationTokenSource, UniTask> callback)
    {
        Domain = domain;
        _preSetCallback = callback;
        StackType = SceneStackType.ReplaceAll;
        Logger.SetEnableLogging(true);
    }
    public void Dispose()
    {
        _preSetCallback = null;
        Domain = null;
    }

    /// <summary>
    /// シーンスクリプト読み込みの外だし用クラス
    /// 必要処理だけに絞って外に出す
    /// </summary>
    public class OuterLoader
    {
        public List<Scene> Scenes;
        public List<MonoBehaviour> SceneBases;
        LayeredSceneTransitioner _owner;
        public OuterLoader(LayeredSceneTransitioner owner)
        {
            Scenes = new();
            SceneBases = new();
            _owner = owner;
        }

        public async UniTask<TType> GetAsyncComponentByScene<TType>(CancellationTokenSource cts)
        {
            TType result = default;
            await _owner.LoadScene<TType>(cts, (scene, sceneBase) => {
                Scenes.Add(scene);
                result = sceneBase;
                if (sceneBase is MonoBehaviour)
                {
                    SceneBases.Add(sceneBase as MonoBehaviour);
                }
            });

            return result;
        }
    }

    public async virtual UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts)
    {
        OuterLoader outer = new (this);
        await _preSetCallback(outer, cts);

        if (outer.SceneBases.Count == 0)
        {
            Logger.Warning($"{GetSceneName()}にシーンのコンポーネントがScene内Hierarchyに設定されていない。");
        }

        // プレイヤーループを挟まないとアタッチがされず参照不全になるので待つ
        await UniTask.WhenAll(outer.SceneBases.Select(async v => await UniTask.WaitForEndOfFrame(v, cts.Token)));
        Domain.Initialize(cts);

        return outer.Scenes;
    }

    public async UniTask LoadScene<TComponent>(CancellationTokenSource cts, Action<Scene, TComponent> callBack)
    {
        string sceneName = typeof(TComponent).Name;

        Scene scene = await LoadSceneByName(sceneName, cts);
        if (scene.IsValid())
        {
            callBack(scene,
                     ExSceneManager.Instance.GetSceneBaseFromScene<TComponent>(scene));
        }
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

    public async virtual UniTask UnLoadScenes(CancellationTokenSource cts)
    {
        foreach (var pair in Domain.GetLayerNames())
        {
            await UnLoadSceneByName(pair.Value, cts);
        }
    }

    private async UniTask UnLoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Logger.Debug("アンロード開始:" + sceneName);
        await SceneManager.UnloadSceneAsync(sceneName).WithCancellation(cts.Token);
        await Resources.UnloadUnusedAssets().WithCancellation(cts.Token);
        Logger.Debug("アンロード終了:" + sceneName);
    }

    public async UniTask Transition(CancellationTokenSource cts)
    {
        await ExSceneManager.Instance.Transition(this, cts);
    }

    public string GetSceneName()
    {
        return Domain.GetSceneName();
    }

}

///// <summary>
///// 階層構造を持たないシーン（ただしLogic層として判定する）
///// </summary>
///// <typeparam name="TParam"></typeparam>
//public abstract class SoloLayerSceneTransitioner : LayeredSceneTransitioner
//{
//    protected SoloLayerSceneTransitioner(ILayeredSceneDomain domain) : base(domain)
//    {

//    }
//}
