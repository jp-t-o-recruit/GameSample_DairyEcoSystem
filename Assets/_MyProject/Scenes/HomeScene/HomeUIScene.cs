using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


using Logger = MyLogger.MapBy<HomeUIScene>;

/// <summary>
/// 
/// 
/// �Q�lUI�V�[���\��
/// https://engineering.enish.jp/?p=1115
/// </summary>
public class HomeUIScene : SceneBase
{
    public class CreateParameter
    {
        public string ViewLabel = "HomeUIScene�f�t�H���g�̃��x��";
    }

    Button _nextSceneButton;
    Label _viewLabel;

    void Awake()
    {
        // �e�V�[��(Root)�̃��[�g�L�����o�X���擾����
        var rootCanvasParentScene = SceneManager.GetSceneByName(typeof(HomeScene).ToString()).GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X���擾����
        var rootCanvas = GetComponent<Canvas>();

        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X��UI�J�������폜����
        if (rootCanvas.worldCamera != null)
        {
            Object.Destroy(rootCanvas.worldCamera.gameObject);
            rootCanvas.worldCamera = null;
        }


        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X��UI�J������e�V�[��(Root)�̃J�����ɒu��������
        rootCanvas.worldCamera = rootCanvasParentScene.worldCamera;
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");

        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = new CreateParameter().ViewLabel;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        Logger.UnloadEnableLogging();
    }

    async void OnButtonClicked()
    {
        _viewLabel.text = "HomeUIScene�{�^������ݒ�I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        Logger.SetEnableLogging(false);
        Logger.Debug("�z�[���Ń{�^������");

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
        //        TitleScene.Transition transition = new TitleScene.Transition() { Parameter = { viewStr ="HomeUIScene�Őݒ�I�I�I" }};
        //        await pageManager.Replace(transition);
        //        break;
        //    }
        //}
    }
}
