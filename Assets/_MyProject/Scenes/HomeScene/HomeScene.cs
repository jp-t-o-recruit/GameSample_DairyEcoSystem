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
        public string ViewLabel = "HomeScene�X�N���v�g����ݒ�I";
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
        _viewLabel.text = "HomeScene�{�^������ݒ�I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("�z�[���Ń{�^������");

        TitleSceneTransition transition = new ();
        await ExSceneManager.Instance.Replace(transition);
        ////���[�h�ς݂̃V�[���ł���΁A���O�ŕʃV�[�����擾�ł���
        //Scene scene = SceneManager.GetSceneByName("ManagerScene");

        //PageManager pageManager;

        ////GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        ////�܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        //foreach (var rootGameObject in scene.GetRootGameObjects())
        //{
        //    pageManager = rootGameObject.GetComponent<PageManager>();
        //    if (pageManager != null)
        //    {
        //        TitleScene.Transition transition = new TitleScene.Transition() { Parameter = { viewStr ="homeScene�Őݒ�I�I�I" }};
        //        await pageManager.Replace(transition);
        //        break;
        //    }
        //}
    }
}
