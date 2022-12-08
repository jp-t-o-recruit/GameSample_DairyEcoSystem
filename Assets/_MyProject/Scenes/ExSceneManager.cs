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
    public abstract UniTask UnLoadScenes();
}

public abstract class LayerdSceneTransition<TParam> : ISceneTransition where TParam : new()
{
    public TParam Parameter { get; set; }
    internal Dictionary<SceneLayer, System.Type> _layer;

    /// <summary>
    /// このシーンまとまりが複数回ロードされても良いか
    /// 
    /// TODO 複数回ロードを許したら実体シーンを取得や参照するのがかなり難しくなる
    /// </summary>
    //public bool CanMultipleLoad = false;

    public async virtual UniTask<List<Scene>> LoadScenes()
    {
        List<Scene> scenes = new();
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        Scene scene = await LoadSceneByName(logicSceneName);
        scenes.Add(scene);

        string UISceneName = _layer[SceneLayer.UI].ToString();
        Scene UIScene = await LoadSceneByName(UISceneName);
        scenes.Add(UIScene);

        return scenes;
    }
    protected async UniTask<Scene> LoadSceneByName(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (null == scene)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            scene = SceneManager.GetSceneByName(sceneName);
        }
        return scene;
    }

    public async virtual UniTask UnLoadScenes()
    {
        foreach(System.Type types in _layer.Values)
        {
            await UnLoadSceneByName(types.ToString());
        }
    }

    public async UniTask UnLoadSceneByName(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
        await Resources.UnloadUnusedAssets().ToUniTask();
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

/// <summary>
/// 階層構造を持たないシーン（ただしLogic層として判定する）
/// </summary>
/// <typeparam name="TParam"></typeparam>
public abstract class SoloLayerSceneTransition<TParam> : LayerdSceneTransition<TParam> where TParam : new()
{
    public async override UniTask<List<Scene>> LoadScenes()
    {
        List<Scene> scenes = new();
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        Scene scene = await LoadSceneByName(logicSceneName);
        scenes.Add(scene);
        return scenes;
    }
}


/// <summary>
/// 自作シーンマネージャー
/// 
/// ■機能
/// ・シーン構成を担保
/// 　┗GameManagerScene（常駐）
/// 　　　┣HogeScene
/// 　　　┣HogeUIScene
/// 　　　┗HogeFieldScene
/// ・Run時のコンポーネント設定を調整（AudioListenerの重複とか）
/// 　┗調整の実作業はストラテジーパターンで別入れ出来るように
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    private Dictionary<SceneEnum, string> _typeToName;

    private Stack<ISceneTransition> _transitions;

    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public ExSceneManager()
    {
        _transitions = new Stack<ISceneTransition>();
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

    async UniTask UnloadSceneAsync(ISceneTransition transition)
    {
        await transition.UnLoadScenes();
    }
    public async UniTask PushOrRetry(ISceneTransition transition)
    {
        // TODO 動作確認中ログ
        Logger.SetEnableLogging(true);
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitions.Count);
        Logger.Debug("Retry transition:" + transition.GetType());
        if (_transitions.Count > 0)
        {
            Logger.Debug("Retry Last______:" + _transitions.Last().GetType());
            Logger.Debug("Retry bool______:" + (_transitions.Last().GetType() == transition.GetType()));
        }

        if (_transitions.Count > 0 && _transitions.Last().GetType() == transition.GetType())
        {
            var scenes = await transition.LoadScenes();
            Scene scene = scenes.First();
            Logger.Debug("Retry ロード後 トップシーン:" + scene.name);
            SceneManager.SetActiveScene(scene);
            Reflesh();
        }
        else
        {
            await PushAsync(transition);
        }
    }

    public async UniTask PushAsync(ISceneTransition transition)
    {
        _transitions.Push(transition);
        var scenes = await transition.LoadScenes();
        Scene scene = scenes.First();
        Logger.Debug("PushAsync ロード後");
        SceneManager.SetActiveScene(scene);
        Reflesh();
    }

    internal async UniTask Replace(ISceneTransition transition)
    {
        var removeTransition = _transitions.Pop();
        // TODO 実シーン取得してないけど大まかな技術的にはスタックで通用するはず
        //var removeScene = SceneManager.GetActiveScene();

        Logger.Debug("Replace スタック抜き");
        // 新たなシーンをアクティブにする
        await PushAsync(transition);
        Logger.Debug("Replace アンロード前");
        // アクティブなシーンがある状態で以前のシーンアンロード
        await UnloadSceneAsync(removeTransition);
    }

    internal async UniTask ReplaceAll(ISceneTransition transition)
    {
        // TODO GameManagerSceneは常設ならスタックとは別にして数えなくても？・・・
        while (_transitions.Count > 1)
        {
            var unloadTransition = _transitions.Pop();
            await UnloadSceneAsync(unloadTransition);
        }

        await PushAsync(transition);
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
        // TODO GameManagerSceneは常設ならスタックとは別にして数えなくても？・・・
        if (_transitions.Count > 1)
        {
            return false;
        }

        // スタック最後を削除
        var deleteTransition = _transitions.Pop();
        // TODO　シーン実体を参照したほうが正確ではある
        //Scene scene = SceneManager.GetActiveScene();
        await UnloadSceneAsync(deleteTransition);

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

    public SceneLayer GetLayer(ILayeredScene layered)
    {
        if (layered is ILayeredSceneLogic) return SceneLayer.Logic;
        if (layered is ILayeredSceneUI) return SceneLayer.UI;
        if (layered is ILayeredSceneField) return SceneLayer.Field;

        throw new NotImplementedException($"{layered.GetType()}は不明なシーン階層です");
    }

    public void TODOChange()
    {
        _typeToName = new()
        {
            [SceneEnum.GameManagersScene] = "GameManagersScene",
            [SceneEnum.TitleScene] = "TitleScene",
            [SceneEnum.HomeScene] = "HomeScene",
            [SceneEnum.TutorialScene] = "TutorialScene",
            [SceneEnum.CreditNotationScene] = "CreditNotationScene",
        };
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            _typeToName[SceneEnum.TitleScene]
        );
    }
}
