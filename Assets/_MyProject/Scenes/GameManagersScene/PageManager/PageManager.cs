using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalLogger = MyLogger.MapBy<PageManager>;

/// <summary>
/// ページマネージャー
/// https://learning.unity3d.jp/6688/
/// 【Unity】DIコンテナVContainerの使い方まとめ
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// VContainer入門(1) - IContainerBuilderとIObjectResolver
/// https://qiita.com/sakano/items/3a009019e279024fda19
/// </summary>
public class PageManager : SingletonBase<PageManager>
{
    List<BasePage> _pages;

    /// <summary>
    /// シングルトン実装
    /// </summary>
    //public static PageManager Instance {
    //    get {
    //        if (null == _instance)
    //        {
    //            _instance = new PageManager();
    //        }
    //        return _instance;
    //    }
    //}
    //private static PageManager _instance;

    ///// <summary>
    ///// コンストラクタ
    ///// </summary>
    public PageManager()
    {
        LocalLogger.SetEnableLogging(true);
        LocalLogger.Debug("PageManager コンストラクタ！");
        _pages = new List<BasePage>();
    }

    ///// <summary>
    ///// マネージャーとして自身を作成
    ///// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    //Managerという名前のGameObjectを作成し、SceneManagerというクラスをAddする
    //    new GameObject("Manager", typeof(PageManager));
    //}

    // Start is called before the first frame update
    void Start()
    {
        //MyLogger = new MyDebugLogger() { IsLogging = true };
        LocalLogger.Debug("PageManager Start！");

        //Invoke("ChangeScene", 3f);
        //Invoke("EndScene", 3f + 5f);


        //ChangeScene("SampleScene");
        //EndSceneAsync("SampleScene");
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy()
    {
        LocalLogger.Debug("Page Manager OnDestroy！");
    }

    public void ChangeScene(string sceneName)
    {
        // File→BuildSettings での登録が必要
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    async void EndSceneAsync(string sceneName)
    {
        // File→BuildSettings での登録が必要
        await SceneManager.UnloadSceneAsync(sceneName);
        await UniTask.Delay(5000);
        await Resources.UnloadUnusedAssets();
    }

    public async UniTask PushAsync(IPageTransition transition)
    {
        var page = await transition.LoadPage();
        await page.Initialize();
        _pages.Add(page);
        Reflesh();
    }

    internal async UniTask Replace(IPageTransition transition)
    {
        await PageTopDelete();
        await PushAsync(transition);
    }

    internal async UniTask ReplaceAll(IPageTransition transition)
    {
        // スタック的クリアなので逆順呼び出し(一応リストの破壊しない逆順呼び出し)
        foreach (var page in _pages.AsEnumerable().Reverse())
        {
            await page.Discard();
        }
        _pages.Clear();

        await PushAsync(transition);
    }

    internal async UniTask Pop()
    {
        await PageTopDelete();

        // スタック最後が更新されたので再呼び出し
        BasePage page = _pages.Last();
        if (page == null)
        {
            // TODO例外処理？
            // シーンリセット
            throw new InvalidOperationException("仕様としておかしいページが全部なくなった時");
        }
        else
        {
            await page.Resume();
        }
        Reflesh();
    }

    private async UniTask PageTopDelete()
    {
        // スタック最後を削除
        BasePage popPage = _pages.Last();
        if (popPage == null)
        {
            // TODO例外処理？
            // シーンリセット
            throw new InvalidOperationException("仕様としておかしいページが無かった時");
        }
        else
        {
            await popPage.Discard();
            _pages.RemoveAt(_pages.Count - 1);
        }
    }

    void Reflesh()
    {
        // TODO 更新をビューに反映
    }
}
