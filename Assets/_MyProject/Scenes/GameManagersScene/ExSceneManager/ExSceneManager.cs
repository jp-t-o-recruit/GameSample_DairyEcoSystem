using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;


public interface ISceneTransition
{
    public abstract UniTask<Scene> LoadScene();
}
public abstract class SceneTransition<TParam>: ISceneTransition where TParam : new()
{
    public string SceneName { get; protected set; }
    public TParam Parameter { get; set; }
    public async UniTask<Scene> LoadScene()
    {
        await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByName(SceneName);
        AttachParameter(scene);
        return scene;
    }

    /// <summary>
    /// シーンクラス（スクリプト）にパラメータをアタッチする
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneBase<TParam> AttachParameter(Scene scene)
    {
        SceneBase<TParam> sceneBase = null;

        // GetRootGameObjectsで、そのシーンのルートGameObjects
        // つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase<TParam>>();
            if (sceneBase != null)
            {
                sceneBase.CreateParam = Parameter ?? new TParam();
                break;
            }
        }
        return sceneBase;
    }
}


/// <summary>
/// 自作シーンマネージャー
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public ExSceneManager()
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager コンストラクタ！");

        Scene scene = SceneManager.GetActiveScene();
        Logger.Debug($"ExSceneManager シーン： {null != scene}, シーン数:{SceneManager.sceneCount }");
    }

    ///// <summary>
    ///// マネージャーとして自身を作成
    ///// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    // ExSceneManagerという名前のGameObjectを作成し、ExSceneManagerというクラスをAddする
    //    new GameObject("ExSceneManager", typeof(ExSceneManager));
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
        //UniTask.Delay(5000);
        await Resources.UnloadUnusedAssets().ToUniTask();
    }

    public async UniTask PushAsync(ISceneTransition transition)
    {
        Scene scene = await transition.LoadScene();
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
        foreach(var index in Enumerable.Range(0, SceneManager.sceneCount).Reverse())
        {
            unloadScenes.Add(SceneManager.GetSceneAt(index).name);
        }

        // 先に追加してから
        await PushAsync(transition);

        // 以前のシーンをアンロード
        foreach (var name in unloadScenes){
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

    private SceneBase<TParam> GetSceneBaseForLast<TParam>()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneBase<TParam> sceneBase = null;

        //GetRootGameObjectsで、そのシーンのルートGameObjects
        //つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase<TParam>>();
            if (sceneBase != null)
            {
                break;
            }
        }
        return sceneBase;
    }
}
