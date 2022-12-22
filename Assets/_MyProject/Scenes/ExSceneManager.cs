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
    private Stack<ILayeredSceneDomain> _domains;

    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public ExSceneManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_domains != default)
        {
            ClearAll(new CancellationTokenSource()).Forget();
        }
        _domains = new Stack<ILayeredSceneDomain>();
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager イニシャライズ！");
    }


    async UniTask UnloadSceneAsync(ILayeredSceneDomain domain, CancellationTokenSource cts)
    {
        domain.Discard(cts);
        ISceneTransitioner transitioner = domain.CreateTransitioner();
        await transitioner.UnLoadScenes(cts);
        transitioner.Dispose();
    }

    /// <summary>
    /// 階層シーンの不足を補い完全性を整えるシーンロード
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushOrRetry(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 0 && _domains.Last().GetSceneName() == transitioner.GetSceneName())
        {
            var scenes = await transitioner.LoadScenes(cts);
            Scene scene = scenes.First();
            Logger.Debug("シーン遷移　Retry ロード後 トップシーン:" + scene.name);
            SceneManager.SetActiveScene(scene);
        }
        else
        {
            Logger.Debug("シーン遷移　Retry で　Push　呼び出し:" + transitioner.GetSceneName());
            await Push(transitioner, cts);
        }
    }

    /// <summary>
    /// クラス外からPushとして呼び出されるPush処理
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushGlobal(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 0)
        {
            var lastDomain = _domains.Last();
            Logger.Debug("PushGlobal suspend実行 :" + lastDomain.GetSceneName());
            lastDomain.Suspend(cts);
        }
        Logger.Debug("PushGlobal push開始" + transitioner.GetSceneName());
        await Push(transitioner, cts);
    }

    /// <summary>
    /// 内部的にPushとして呼び出すPush処理
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    private async UniTask Push(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        _domains.Push(transitioner.Domain);
        Logger.Debug("Push transition：" + transitioner.Domain.GetSceneName());
        var scenes = await transitioner.LoadScenes(cts);
        Scene scene = scenes.First();
        Logger.Debug("Push シーンロードした：" + scene.name);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// 指定シーンをスタックピークとして交換するシーン読み込み
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask Replace(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        var removeDomain = _domains.Count > 0 ? _domains.Pop(): default;

        Logger.Debug("Replace スタック抜き");
        // 新たなシーンをアクティブにする
        await Push(transitioner, cts);
        Logger.Debug("Replace アンロード前");
        // シーン数0が発生しないようアクティブなシーンがある状態で以前のシーンアンロード
        if (removeDomain != default)
        {
            await UnloadSceneAsync(removeDomain, cts);
        }
    }
    /// <summary>
    /// 指定シーンのみのスタックに更新するシーン読み込み
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask ReplaceAll(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        await ClearAll(cts);
        await Push(transitioner, cts);
    }

    private async UniTask ClearAll(CancellationTokenSource cts)
    {
        while (_domains.Count > 0)
        {
            await UnloadSceneAsync(_domains.Pop(), cts);
        }
    }


    /// <summary>
    /// アクティブシーンをPop、出来ない場合フェールセーフとして指定シーンにReplace
    /// </summary>
    async UniTask TryPopDefaultReplace(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 1) {
            // Pop可能→戻るシーンがある→スタックが2以上なら
            await Pop(cts);
        }
        else
        {
            Logger.Warning($"シーンをPopするつもりが出来なかった。シーンStack数：{_domains.Count}, 最上シーン名：{_domains.Last()?.GetSceneName()}");
            await Replace(transitioner, cts);
        }
    }

    /// <summary>
    /// アクティブシーンをPop
    /// </summary>
    async UniTask Pop(CancellationTokenSource cts)
    {
        // スタック最後を削除
        var unloadDomain = _domains.Pop();
        await UnloadSceneAsync(unloadDomain, cts);
        
        var lastDomain = _domains.Last();
        ActivateSceneFromDomain(lastDomain);
        lastDomain.Resume(cts);
    }

    public async UniTask Transition(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        switch (transitioner.StackType)
        {
            case SceneStackType.PushOrRetry:
                await PushOrRetry(transitioner, cts);
                break;
            case SceneStackType.Push:
                await PushGlobal(transitioner, cts);
                break;
            case SceneStackType.Replace:
                await Replace(transitioner, cts);
                break;
            case SceneStackType.ReplaceAll:
                await ReplaceAll(transitioner, cts);
                break;
            case SceneStackType.PopTry:
                await TryPopDefaultReplace(transitioner, cts);
                break;
            default:
                throw new ArgumentException($"シーン遷移方法判定中に例外が発生しました。{transitioner.StackType} is not {typeof(SceneStackType).Name}.");
        }

        transitioner.Dispose();
    }

    private bool ActivateSceneFromDomain(ILayeredSceneDomain domain)
    {
        string sceneName = domain.GetSceneName();
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
            return true;
        }
        return false;
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

        if ( component == null) Logger.Warning($"シーン読み込み　GetSceneBase　でシーンコンポーネント：{typeof(TComponent).Name} の取得ができなかった。");

        return component;
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