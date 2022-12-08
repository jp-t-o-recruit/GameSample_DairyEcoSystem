using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class HomeSceneTransition : LayerdSceneTransition<HomeUIScene.CreateParameter>
{
    public HomeSceneTransition()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(HomeScene) },
            { SceneLayer.UI, typeof(HomeUIScene) },
            { SceneLayer.Field, typeof(HomeFieldScene) },
        };
        SceneName = _layer[SceneLayer.Logic].ToString();
    }
}

//[PageAsset("HomeScenePage.prefab")]
//public class HomeScenePage : BasePage<HomeScenePage.CreateParameter>

//    //async void OnButtonClicked()
//    //{
//    //    ////ロード済みのシーンであれば、名前で別シーンを取得できる
//    //    //Scene scene = SceneManager.GetSceneByName("GameManagersScene");

//    //    //PageManager pageManager;

//    //    ////GetRootGameObjectsで、そのシーンのルートGameObjects
//    //    ////つまり、ヒエラルキーの最上位のオブジェクトが取得できる
//    //    //foreach (var rootGameObject in scene.GetRootGameObjects())
//    //    //{
//    //    //    pageManager = rootGameObject.GetComponent<PageManager>();
//    //    //    if (pageManager != null)
//    //    //    {
//    //    //        await pageManager.Replace(new FooPage.Transition() { Parameter = { Bar = "homeで指定" } }); // 現在のページを破棄して追加する
//    //    //        break;
//    //    //    }
//    //    //}
//    //}
//}

