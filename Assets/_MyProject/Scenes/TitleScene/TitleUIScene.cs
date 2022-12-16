using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;
using LocalLogger = MyLogger.MapBy<TitleUIScene>;

/// <summary>
/// �^�C�g���V�[���̓�UI���C���[�V�[��
/// 
/// �N���X���̓V�[���I�u�W�F�N�g�Ɠ��ꖼ�ɂ���
/// 
/// �Q�lUI�V�[���\��
/// https://engineering.enish.jp/?p=1115
/// </summary>
public class TitleUIScene : SceneBase, ILayeredSceneUI
{
    public class CreateParameter
    {
        public string ViewLabel = "TitleUIScene�f�t�H���g�̃��x��";
    }

    public Button _nextSceneButton;
    Label _viewLabel;

    [Inject]
    public void Construct()
    {
    }

    void Awake()
    {
        // �e�V�[��(Root)�̃��[�g�L�����o�X���擾����
        var rootCanvasParentScene = SceneManager.GetSceneByName($"{typeof(TitleScene)}").GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X���擾����
        var mySceneCanvas = SceneManager.GetSceneByName($"{typeof(TitleUIScene)}").GetRootGameObjects()
        .First(obj => obj.GetComponent<Canvas>() != null)
        .GetComponent<Canvas>();

        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X��UI�J�������폜����
        if (mySceneCanvas.worldCamera != null)
        {
            UnityEngine.Object.Destroy(mySceneCanvas.worldCamera.gameObject);
            mySceneCanvas.worldCamera = null;
        }


        // ���g�̃V�[��(Additive)�̃��[�g�L�����o�X��UI�J������e�V�[��(Root)�̃J�����ɒu��������
        mySceneCanvas.worldCamera = rootCanvasParentScene.worldCamera;
    }

    // Start is called before the first frame update
    void Start()
    {
        _nextSceneButton = RootElement.Q<Button>("nextSceneButton");
        _viewLabel = RootElement.Q<Label>("titleSceneLabel");

        _nextSceneButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = new TitleUIScene.CreateParameter().ViewLabel;
    }

    // Update is called once per frame
    void Update()
    {
        _viewLabel.text = TitleSceneParamsSO.Entity.ViewLabel.ToString();
        _nextSceneButton.text = $"title: {TimeSpan.FromSeconds(DateTime.Now.Second).TotalSeconds % 60}";
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        LocalLogger.UnloadEnableLogging();
    }

    void OnButtonClicked()
    {
        if (IsInputLock) return;
        IsInputLock = true;

        _viewLabel.text = "TitleUIScene�{�^������ݒ�I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("�^�C�g���Ń{�^������");

        _nextSceneButton.pickingMode = PickingMode.Position;
        IsInputLock = false;
    }
}
