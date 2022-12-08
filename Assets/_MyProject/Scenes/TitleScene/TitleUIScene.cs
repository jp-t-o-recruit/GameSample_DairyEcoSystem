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
public class TitleUIScene : SceneBase
{
    public class CreateParameter
    {
        public string ViewLabel = "TitleUIScene�f�t�H���g�̃��x��";
    }

    Button _nextSceneButton;
    Label _viewLabel;

    TitleSceneDomain _domain;

    [Inject]
    public TitleUIScene(TitleSceneDomain domain)
    {
        _domain = domain;
    }

    void Awake()
    {
        // �e�V�[��(Root)�̃��[�g�L�����o�X���擾����
        var rootCanvasParentScene = SceneManager.GetSceneByName($"{typeof(TitleScene)}").GetRootGameObjects()
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
    }

    private void OnDestroy()
    {
        _nextSceneButton.clickable.clicked -= OnButtonClicked;
        LocalLogger.UnloadEnableLogging();
    }

    async void OnButtonClicked()
    {
        _viewLabel.text = "TitleUIScene�{�^������ݒ�I";
        _nextSceneButton.pickingMode = PickingMode.Ignore;
        LocalLogger.SetEnableLogging(false);
        LocalLogger.Debug("�^�C�g���Ń{�^������");

        // TODO
        await _domain.Login();
        _nextSceneButton.pickingMode = PickingMode.Position;
    }
}
