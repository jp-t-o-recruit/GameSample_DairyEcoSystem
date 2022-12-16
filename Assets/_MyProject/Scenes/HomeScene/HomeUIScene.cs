using System;
using System.Linq;
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
public class HomeUIScene : SceneBase, ILayeredSceneUI
{
    public Label _viewLabel;
    public Button _nextSceneButton;
    public Button _toSaveDataBuilderSceneButton;
    public Button _toTitleSceneButton;

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
            UnityEngine.Object.Destroy(rootCanvas.worldCamera.gameObject);
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
        _viewLabel = RootElement.Q<Label>("homeSceneLabel");
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _toSaveDataBuilderSceneButton = RootElement.Q<Button>("toSaveDataBuilderSceneButton");
        _toTitleSceneButton = RootElement.Q<Button>("toTitleSceneButton");

        //_viewLabel.text = ViewLabel. ;
        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _toSaveDataBuilderSceneButton.clickable.clicked += OnToSaveDataBuilderSceneButtonClicked;
        _toTitleSceneButton.clickable.clicked += OnToTitleSceneButtonClicked;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // TODO
        _nextSceneButton.text = $"home: {TimeSpan.FromSeconds(DateTime.Now.Second).TotalSeconds % 60}";
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        Logger.UnloadEnableLogging();
    }

    void OnButtonClicked()
    {
        if (IsInputLock) return;
        IsInputLock = true;

        _viewLabel.text = "HomeUIScene�{�^������ݒ�I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        Logger.SetEnableLogging(false);
        Logger.Debug("�z�[���Ń{�^������");
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
        IsInputLock = false;
    }
    

    private void OnToSaveDataBuilderSceneButtonClicked()
    {
        if (IsInputLock) return;
        IsInputLock = true;
        IsInputLock = false;
    }

    private void OnToTitleSceneButtonClicked()
    {
        if (IsInputLock)�@return;
        IsInputLock = true;
        IsInputLock = false;
    }
}
