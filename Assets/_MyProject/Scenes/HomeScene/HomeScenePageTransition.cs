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
//        _viewLabel.text = Parameter.viewStr ?? "HomeScene�X�N���v�g����ݒ�I";
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
//    //    ////���[�h�ς݂̃V�[���ł���΁A���O�ŕʃV�[�����擾�ł���
//    //    //Scene scene = SceneManager.GetSceneByName("GameManagersScene");

//    //    //PageManager pageManager;

//    //    ////GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
//    //    ////�܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
//    //    //foreach (var rootGameObject in scene.GetRootGameObjects())
//    //    //{
//    //    //    pageManager = rootGameObject.GetComponent<PageManager>();
//    //    //    if (pageManager != null)
//    //    //    {
//    //    //        await pageManager.Replace(new FooPage.Transition() { Parameter = { Bar = "home�Ŏw��" } }); // ���݂̃y�[�W��j�����Ēǉ�����
//    //    //        break;
//    //    //    }
//    //    //}
//    //}
//}

