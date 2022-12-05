using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


using LocalLogger = MyLogger.MapBy<HomeScene>;

public class HomeScene : SceneBase<HomeScene.CreateParameter>
{
    public class CreateParameter
    {
        public string ViewLabel = "HomeSceneスクリプトから設定！";
    }

    Button _nextSceneButton;
    Label _viewLabel;

    // Start is called before the first frame update
    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");

        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = CreateParam.ViewLabel;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        LocalLogger.UnloadEnableLogging();
    }

    async void OnButtonClicked()
    {
        _viewLabel.text = "HomeSceneボタンから設定！";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("ホームでボタン押下");

        TitleSceneTransition transition = new ();
        await ExSceneManager.Instance.Replace(transition);
        ////ロード済みのシーンであれば、名前で別シーンを取得できる
        //Scene scene = SceneManager.GetSceneByName("ManagerScene");

        //PageManager pageManager;

        ////GetRootGameObjectsで、そのシーンのルートGameObjects
        ////つまり、ヒエラルキーの最上位のオブジェクトが取得できる
        //foreach (var rootGameObject in scene.GetRootGameObjects())
        //{
        //    pageManager = rootGameObject.GetComponent<PageManager>();
        //    if (pageManager != null)
        //    {
        //        TitleScene.Transition transition = new TitleScene.Transition() { Parameter = { viewStr ="homeSceneで設定！！！" }};
        //        await pageManager.Replace(transition);
        //        break;
        //    }
        //}
    }
}
