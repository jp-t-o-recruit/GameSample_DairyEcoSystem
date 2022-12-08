using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;



/// <summary>
/// 
/// 採用したシーン設計の基本構造
/// https://gamebiz.jp/news/218949
/// </summary>

public interface ISceneTransition
{
    public abstract UniTask<List<Scene>> LoadScenes();
}
public abstract class SceneTransition<TParam>: ISceneTransition where TParam : new()
{
    public string SceneName { get; protected set; }
    public TParam Parameter { get; set; }
    public async virtual UniTask<List<Scene>> LoadScenes()
    {
        await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByName(SceneName);
        GetSceneBaseFromScene(scene);
        return new List<Scene> { scene };
    }

    /// <summary>
    /// シーンクラス（スクリプト）にパラメータをアタッチする
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    protected SceneBase GetSceneBaseFromScene(Scene scene)
    {
        SceneBase sceneBase = null;

        // GetRootGameObjectsで、そのシーンのルートGameObjects
        // つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase>();
            if (sceneBase != null)
            {
                break;
            }
        }

        return sceneBase;
    }
}


public abstract class LayerdSceneTransition<TParam> : SceneTransition<TParam> where TParam : new()
{
    internal Dictionary<SceneLayer, System.Type> _layer;

    public async override UniTask<List<Scene>> LoadScenes()
    {
        await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByName(SceneName);

        string UISceneName = _layer[SceneLayer.UI].ToString();
        await SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive);

        Scene UIScene = SceneManager.GetSceneByName(UISceneName);
        GetSceneBaseFromScene(UIScene);

        return new List<Scene>() { scene, UIScene };
    }
}

/// <summary>
/// 自作シーンマネージャー
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    private Dictionary<SceneEnum, string> _typeToName;

    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public ExSceneManager()
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager コンストラクタ！");

        Scene scene = SceneManager.GetActiveScene();
        Logger.Debug($"ExSceneManager シーン： {null != scene}, シーン数:{SceneManager.sceneCount}");
    }

    /////// <summary>
    /////// マネージャーとして自身を作成
    /////// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    // ExSceneManagerという名前のGameObjectを作成し、ExSceneManagerというクラスをAddする
    //    new GameObject("ExSceneManager", typeof(ExSceneManager));
    //    //// GameManagersシーンを常駐させる設計の場合
    //    ////ManagerSceneが有効でないときに追加ロード
    //    //if (!SceneManager.GetSceneByName(managerSceneName).IsValid())
    //    //{
    //    //    SceneManager.LoadScene(managerSceneName, LoadSceneMode.Additive);
    //    //}
    //}

    // Start is called before the first frame update
    void Start()
    {
        Logger.Debug("ExSceneManager Start！");
    }

    void OnDestroy()
    {
        Logger.Debug("ExSceneManager OnDestroy！");
    }

    //public void ChangeScene(string sceneName)
    //{
    //    // File→BuildSettings での登録が必要
    //    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    //}

    async UniTask UnloadSceneAsync(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
        await Resources.UnloadUnusedAssets().ToUniTask();
    }

    public async UniTask PushAsync(ISceneTransition transition)
    {
        var scenes = await transition.LoadScenes();
        Scene scene = scenes.First();
        Logger.Debug("PushAsync ロード後");
        SceneManager.SetActiveScene(scene);
        Reflesh();
    }

    internal async UniTask Replace(ISceneTransition transition)
    {
        var removeScene = SceneManager.GetActiveScene();
        Logger.Debug("Replace スタック抜き");
        // 新たなシーンをアクティブにする
        await PushAsync(transition);
        Logger.Debug("Replace アンロード前");
        // アクティブなシーンがある状態で以前のシーンアンロード
        await UnloadSceneAsync(removeScene.name);
    }

    internal async UniTask ReplaceAll(ISceneTransition transition)
    {
        // 現在のシーン名をすべて取得
        // スタック的クリアなので逆順で取得
        List<string> unloadScenes = new();
        foreach (var index in Enumerable.Range(0, SceneManager.sceneCount).Reverse())
        {
            unloadScenes.Add(SceneManager.GetSceneAt(index).name);
        }

        // 先に追加してから
        await PushAsync(transition);

        // 以前のシーンをアンロード
        foreach (var name in unloadScenes)
        {
            await UnloadSceneAsync(name);
        };
    }

    /// <summary>
    /// アクティブシーンをPop
    /// </summary>
    /// <returns>シーンがアンロードされたか</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async UniTask<bool> Pop()
    {
        bool result = await PageTopDelete();

        // TODO　スタック最後が更新されたので再呼び出しの処理
        // ラッパー的な部分で何かやらない限り
        // シーン単体なら特別やることない
        //await page.Resume();

        Reflesh();
        return result;
    }

    private async UniTask<bool> PageTopDelete()
    {
        //if (EditorSceneManager.loadedSceneCount > 1)
        // TODO　Unityバージョン2021.3まではロード済みの数
        // 2022.2からはロード中やアンロード中を含む
        if (SceneManager.sceneCount > 1)
        {
            return false;
        }

        // スタック最後を削除
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        await UnloadSceneAsync(sceneName);

        return true;
    }

    void Reflesh()
    {
        // TODO 更新をビューに反映
    }

    private SceneBase GetSceneBaseForLast()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneBase sceneBase = null;

        //GetRootGameObjectsで、そのシーンのルートGameObjects
        //つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase>();
            if (sceneBase != null)
            {
                break;
            }
        }
        return sceneBase;
    }




    public void TODOChange()
    {
        _typeToName = new()
        {
            [SceneEnum.GameManagersScene] = "GameManagersScene",
            [SceneEnum.TitleScene] = "TitleScene",
            [SceneEnum.HomeScene] = "HomeScene",
            [SceneEnum.TutorialScene] = "TutorialScene",
        };
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            _typeToName[SceneEnum.TitleScene]
        );
    }
}
