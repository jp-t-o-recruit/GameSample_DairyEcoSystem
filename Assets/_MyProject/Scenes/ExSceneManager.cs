using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;


/// <summary>
/// 自作シーンマネージャー
/// 
/// ■機能
/// ・シーン構成を担保
/// 　┣GameManagerScene（常駐）
/// 　┣HogeScene
/// 　┣HogeUIScene
/// 　┗HogeFieldScene
/// ・Run時のコンポーネント設定を調整（AudioListenerの重複とか）
/// 　┗調整の実作業はストラテジーパターンで別入れ出来るように
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    private Stack<ISceneTransitioner> _transitioners;

    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public ExSceneManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        _transitioners = new Stack<ISceneTransitioner>();
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager イニシャライズ！");

        //Scene scene = SceneManager.GetActiveScene();
        //Logger.Debug($"ExSceneManager シーン： {null != scene}, シーン数:{SceneManager.sceneCount}");
    }


    async UniTask UnloadSceneAsync(ISceneTransitioner transition)
    {
        await transition.Discard();
        await transition.UnLoadScenes();
    }

    public async UniTask PushOrRetry(SceneEnum sceneEnum)
    {
        string sceneName = Enum.GetName(typeof(SceneEnum), sceneEnum);
        // TODO 動作確認中ログ
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitioners.Count);
        Logger.Debug("Retry class is:" + sceneName);
        if (_transitioners.Count > 0)
        {
            Logger.Debug("Retry Last______:" + _transitioners.Last().GetType());
            Logger.Debug("Retry bool______:" + (_transitioners.Last().GetType().Name == sceneName));
        }

        if (_transitioners.Count > 0 && _transitioners.Last().GetType().Name == sceneName)
        {
            ISceneTransitioner transition = SceneRelationService.GetSceneTransitionerByEnum(sceneEnum);
            var scenes = await transition.LoadScenes();
            Scene scene = scenes.First();
            Logger.Debug("Retry ロード後 トップシーン:" + scene.name);
            SceneManager.SetActiveScene(scene);

            await transition.Initialize();
        }
        else
        {
            ISceneTransitioner transition = SceneRelationService.GetSceneTransitionerByEnum(sceneEnum);
            await Push(transition);
        }
    }

    public async UniTask PushAsync(ISceneTransitioner transition)
    {
        Logger.Debug("PushAsync ラストは？" + _transitioners.Count);
        var lastTrantion = _transitioners.Last();
        await lastTrantion.Suspend();

        await Push(transition);
    }

    private async UniTask Push(ISceneTransitioner transition)
    {
        _transitioners.Push(transition);
        Logger.SetEnableLogging(true);
        Logger.Debug("Push transition：" + transition.GetType());
        var scenes = await transition.LoadScenes();
        Scene scene = scenes.First();
        Logger.Debug("Push シーンロードした：" + scene.name);
        SceneManager.SetActiveScene(scene);

        await transition.Initialize();
    }

    internal async UniTask Replace(ISceneTransitioner transition)
    {
        var removeTransition = _transitioners.Pop();

        Logger.Debug("Replace スタック抜き");
        // 新たなシーンをアクティブにする
        await Push(transition);
        Logger.Debug("Replace アンロード前");
        // アクティブなシーンがある状態で以前のシーンアンロード
        await UnloadSceneAsync(removeTransition);
    }

    internal async UniTask ReplaceAll(ISceneTransitioner transition)
    {
        while(_transitioners.Count > 0)
        {
            var unloadTransition = _transitioners.Pop();
            await UnloadSceneAsync(unloadTransition);
        }

        await Push(transition);
    }

    /// <summary>
    /// アクティブシーンをPop
    /// </summary>
    internal async UniTask Pop()
    {
        // スタック最後を削除
        var unloadTransition = _transitioners.Pop();
        await UnloadSceneAsync(unloadTransition);
        
        var lastTrantion = _transitioners.Last();
        SceneActivateFromTransition(lastTrantion);
        await lastTrantion.Resume();
    }

    public async UniTask Transition(ISceneTransitioner transition)
    {
        switch (transition.NextRelation)
        {
            case SceneRelation.Free:
                if (new List<SceneRelation>(){ SceneRelation.StartLink }.Contains(transition.PrevRelation) )
                {
                    await ReplaceAll(transition);
                } else
                {
                    await Replace(transition);
                }
                break;
            case SceneRelation.None:
                await Pop();
                break;
            case SceneRelation.ChainLink:
            case SceneRelation.StartLink:
            case SceneRelation.HookLink:
                await PushAsync(transition);
                break;
            default:
                throw new ArgumentException($"シーン遷移方法判定中に例外が発生しました。{transition.NextRelation} is not {typeof(SceneRelation).Name}.");
        }        
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

    private bool SceneActivateFromTransition(ISceneTransitioner trantion)
    {
        string sceneName = trantion.GetSceneName();
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
            return true;
        }
        return false;
    }

    public SceneLayer GetLayer(ILayeredScene layered)
    {
        if (layered is ILayeredSceneLogic) return SceneLayer.Logic;
        if (layered is ILayeredSceneUI) return SceneLayer.UI;
        if (layered is ILayeredSceneField) return SceneLayer.Field;

        throw new NotImplementedException($"{layered.GetType()}は不明なシーン階層です");
    }

    /// <summary>
    /// 指定シーンからコンポーネントの取得
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <param name="scene"></param>
    /// <returns></returns>
    public static TComponent GetComponentFromScene<TComponent>(Scene scene) where TComponent : MonoBehaviour
    {
        TComponent component = default;

        // GetRootGameObjectsで、そのシーンのルートGameObjects
        // つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            if (rootGameObject.TryGetComponent<TComponent>(out component))
            {
                break;
            }
        }
        return component;
    }

    /// <summary>
    /// TODO ReplenishLayeredSceneOnPlayModeの方で実現しているので用済み
    /// </summary>
    /// <param name="factory"></param>
    //internal async void NoticeDefaultTransition(Func<ISceneTransitioner> factory)
    //{
    //    Logger.Debug("NoticeDefaultTransition Count: " + _transitioners.Count);
    //    if (_transitioners == default || _transitioners.Count == 0)
    //    {
    //        Instance.Initialize();

    //        var transition = factory();
    //        Logger.Debug("NoticeDefaultTransition: 初期化してプッシュした" + transition.GetSceneName());

    //        await Instance.ReplaceAll(transition);
    //    }
    //    else
    //    {
    //        // TODO _transitionsを格納していてもデバッグで保持が続いてるだけで初期化がそのデバッグで呼ばれていないルートもある
    //        // GameManagersSceneAutoLoaderで初期化呼ぶことでそのルートを断つ？
    //        Logger.Debug($"NoticeDefaultTransition 初期化しない。 transitions数：{_transitioners.Count}  ：{_transitioners.Last().GetSceneName()}");
    //    }
    //}

    //internal async UniTask TrySetup()
    //{
    //    // GameManagerSceneを生成
    //    // GameManagerSceneをヒエラルキートップに
    //    // アクティブシーンは変更せず
    //    var sceneName = Enum.GetName(typeof(SceneEnum), SceneEnum.GameManagersScene);
    //    Logger.SetEnableLogging(true);
    //    Scene scene = SceneManager.GetSceneByName(sceneName);
    //    if (scene.isLoaded)
    //    {
    //        Logger.Debug("GameManagerSceneを生成　しない: " + scene.buildIndex);
    //        return;
    //    }

    //    // GameManagersSceneは常駐させる
    //    await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    //    scene = SceneManager.GetSceneByName(sceneName);
    //    Logger.Debug("GameManagerSceneを生成　した？: " + scene.buildIndex);
    //}

}


#if DEVELOPMENT_BUILD || UNITY_EDITOR
/// <summary>
/// Awake前にManagerSceneを自動でロードするクラス
/// 
/// コピペ元
/// 全てのシーンに存在し、かつ、一つしか存在してはいけないマネージャー的存在の実装方法【Unity】 - (:3[kanのメモ帳]
/// https://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
/// </summary>
public class GameManagersSceneAutoLoader
{
    //ゲーム開始時(シーン読み込み前)に実行される
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        ReplenishLayeredSceneOnPlayMode();
        LoadGameManagersScene();

        //ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ExSceneManager.Instance.Initialize();
        // TODO マネージャー常駐の意義があるなら使うかも
        // その時はシングルトン実装をやめる
        // CreateManagerInstance();
    }
    private static void CreateScenario(SceneEnum sceneEnum)
    {
        // TODO
        //var hoge = new HomeScenario();
        //var sev = new TitleScenario();
    }

    /// <summary>
    /// playmode起動でシーン階層がHierarchyに足りてないなら補充する
    /// </summary>
    private async static void ReplenishLayeredSceneOnPlayMode()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene == default)
        {
            return;
        }

        if (EnumExt.TryParseToEnum(scene.name, out SceneEnum sceneEnum))
        {
            await ExSceneManager.Instance.PushOrRetry(sceneEnum);
            CreateScenario(sceneEnum);
        }
        else
        {
            throw new FormatException($"シーン名変換中に例外が発生しました。{scene.name} is not SceneEnum.");
        }
    }

    /// <summary>
    /// 常駐させるGameManagersSceneを起動する
    /// </summary>
    private static void LoadGameManagersScene()
    {
        string sceneName = Enum.GetName(typeof(SceneEnum), SceneEnum.GameManagersScene);

        // ManagerSceneが有効でない時(まだ読み込んでいない時)だけ追加ロードするように
        if (!SceneManager.GetSceneByName(sceneName).IsValid())
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// 常時シーンにゲームオブジェクトとして生成する前提で常駐サービスとする
    /// </summary>
    private static void CreateManagerInstance()
    {
        //Managerという名前のGameObjectを作成し、ManagerというクラスをAddする
        new GameObject("ExSceneManager", typeof(ExSceneManager));
    }
}
#endif