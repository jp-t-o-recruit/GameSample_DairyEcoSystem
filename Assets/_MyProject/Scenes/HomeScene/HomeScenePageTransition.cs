using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class HomeSceneTransition : SceneTransition<HomeScene.CreateParameter>
{
    public HomeSceneTransition()
    {
        SceneName = "HomeScene";
    }
}

//[PageAsset("HomeScenePage.prefab")]
//public class HomeScenePage : BasePage<HomeScenePage.CreateParameter>
//{
//    [SerializeField] private UIDocument _uiDocument;
//    Button _nextSceneButton;
//    Label _viewLabel;

//    public class CreateParameter
//    {
//        public string viewStr;
//    }
//    public class Transition : BasePageTransition<HomeScenePage, CreateParameter> {

//    }

//    void Start()
//    {
//        var rootElement = _uiDocument.rootVisualElement;
//        _nextSceneButton = rootElement.Q<Button>("nextSceneButton");
//        _viewLabel = rootElement.Q<Label>("homeSceneLabel");

//        //_nextSceneButton.clickable.clicked += OnButtonClicked;
//        _viewLabel.text = Parameter.viewStr ?? "HomeSceneスクリプトから設定！";
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    private void OnDestroy()
//    {
//        //_nextSceneButton.clickable.clicked -= OnButtonClicked;
//    }

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

