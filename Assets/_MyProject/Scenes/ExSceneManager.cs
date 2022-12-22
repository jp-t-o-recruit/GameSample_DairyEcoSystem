using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;

/// <summary>
/// 遷移でのシーンスタックタイプ
/// </summary>
public enum SceneStackType
{
    /// <summary>
    /// 指定シーンのプッシュまたは、現在シーンが指定シーンで不足している部分をロードする
    /// </summary>
    PushOrRetry,
    Push,
    Replace,
    ReplaceAll,
    /// <summary>
    /// "現在シーン"をPopする、ただしフェールセーフとして遷移先の指定シーンで遷移呼び出し
    /// </summary>
    PopTry,
    //Pop,
}

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
    }


    async UniTask UnloadSceneAsync(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        transition.Discard(cts);
        await transition.UnLoadScenes(cts);
    }

    async UniTask PushOrRetry(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        // TODO 動作確認中ログ
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitioners.Count);
        Logger.Debug("Retry class is:" + transitioner.GetSceneName());

        if (_transitioners.Count > 0 && _transitioners.Last().GetSceneName() == transitioner.GetSceneName())
        {
            var scenes = await transitioner.LoadScenes(cts);
            Scene scene = scenes.First();
            Logger.Debug("Retry ロード後 トップシーン:" + scene.name);
            SceneManager.SetActiveScene(scene);
        }
        else
        {
            await Push(transitioner, cts);
        }
    }

    /// <summary>
    /// 外からPushとして呼び出されるPush処理
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushGlobal(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        if (_transitioners.Count > 0)
        {
            var lastTrantion = _transitioners.Last();
            Logger.Debug("PushGlobal suspend実行 :" + lastTrantion.GetSceneName());
            lastTrantion.Suspend(cts);
        }
        Logger.Debug("PushGlobal push開始" + transition.GetSceneName());
        await Push(transition, cts);
    }

    /// <summary>
    /// 内部的にPushとして呼び出すPush処理
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    private async UniTask Push(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        _transitioners.Push(transition);
        Logger.Debug("Push transition：" + transition.GetType());
        var scenes = await transition.LoadScenes(cts);
        Scene scene = scenes.First();
        Logger.Debug("Push シーンロードした：" + scene.name);
        SceneManager.SetActiveScene(scene);
    }

    async UniTask Replace(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        var removeTransition = _transitioners.Count > 0 ? _transitioners.Pop() : default;

        Logger.Debug("Replace スタック抜き");
        // 新たなシーンをアクティブにする
        await Push(transition, cts);
        Logger.Debug("Replace アンロード前");
        // シーン数0が発生しないようアクティブなシーンがある状態で以前のシーンアンロード
        if (removeTransition != default)
        {
            await UnloadSceneAsync(removeTransition, cts);
        }
    }

    async UniTask ReplaceAll(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        while(_transitioners.Count > 0)
        {
            await UnloadSceneAsync(_transitioners.Pop(), cts);
        }

        await Push(transition, cts);
    }
    /// <summary>
    /// アクティブシーンをPop、出来ない場合フェールセーフとして指定シーンにReplace
    /// </summary>
    async UniTask TryPopDefaultReplace(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        if (_transitioners.Count > 1) {
            // Pop可能→つまり戻るシーンがある→スタックが2以上なら
            await Pop(cts);
        }
        else
        {
            Logger.Warning($"シーンをPopするつもりが出来なかった。シーンStack数：{_transitioners.Count}, 最上シーン名：{_transitioners.Last()?.GetSceneName()}");
            await Replace(transition, cts);
        }
    }

    /// <summary>
    /// アクティブシーンをPop
    /// </summary>
    async UniTask Pop(CancellationTokenSource cts)
    {
        // スタック最後を削除
        var unloadTransition = _transitioners.Pop();
        await UnloadSceneAsync(unloadTransition, cts);
        
        var lastTrantion = _transitioners.Last();
        SceneActivateFromTransition(lastTrantion);
        lastTrantion.Resume(cts);
    }

    public async UniTask Transition(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        switch (transition.StackType)
        {
            case SceneStackType.PushOrRetry:
                await PushOrRetry(transition, cts);
                break;
            case SceneStackType.Push:
                await PushGlobal(transition, cts);
                break;
            case SceneStackType.Replace:
                await Replace(transition, cts);
                break;
            case SceneStackType.ReplaceAll:
                await ReplaceAll(transition, cts);
                break;
            case SceneStackType.PopTry:
                await TryPopDefaultReplace(transition, cts);
                break;
            //case SceneStackType.Pop:
            //    await Pop(cts);
            //    break;
            default:
                throw new ArgumentException($"シーン遷移方法判定中に例外が発生しました。{transition.StackType} is not {typeof(SceneStackType).Name}.");
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
        //ReplenishLayeredSceneOnPlayMode();
        LoadGameManagersScene();

        ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ExSceneManager.Instance.Initialize();
        // TODO マネージャー常駐の意義があるなら使うかも
        // その時はシングルトン実装をやめる
        // CreateManagerInstance();
    }

    /// <summary>
    /// playmode起動でシーン階層がHierarchyに足りてないなら補充する
    /// </summary>
    //private async static void ReplenishLayeredSceneOnPlayMode(CancellationTokenSource cts)
    //{
    //    Scene scene = SceneManager.GetActiveScene();
    //    if (scene == default)
    //    {
    //        return;
    //    }

    //    if (EnumExt.TryParseToEnum(scene.name, out SceneEnum sceneEnum))
    //    {
    //        await ExSceneManager.Instance.PushOrRetry(sceneEnum,cts);
    //    }
    //    else
    //    {
    //        throw new FormatException($"シーン名変換中に例外が発生しました。{scene.name} is not SceneEnum.");
    //    }
    //}

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