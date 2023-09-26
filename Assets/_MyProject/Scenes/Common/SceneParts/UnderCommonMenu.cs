using System;
using UnityEngine.UIElements;

using Logger = MyLogger.MapBy<UnderCommonMenu>;

// Unity User Manual 2021.2���[�U�[�C���^�[�t�F�[�X (UI) �̍쐬UI Toolkit�R���g���[��  �R���g���[�����t�@�����X
// https://docs.unity3d.com/ja/2021.2/Manual/UIE-Controls.html

/// <summary>
/// ��ʉ����ʃ��j���[�̃R���g���[���[<br/>
/// <br/>
/// ���j���[���̂�UXML�̃e���v���[�g�@�\�Ƃ���UIDocument�Ɏg�p����Ă���z��B<br/>
/// .Q��UnderCommonMenu������VisualElement������������ێ�����B<br/>
/// </summary>
public class UnderCommonMenu
{
    /// <summary>
    /// �p�[�c��
    /// </summary>
    public static readonly string RootName = "underCommonMenu";

    public Button homeButton;
    public Button charButton;
    public Button storyButton;
    public Button questButton;
    public Button guildHouseButton;
    public Button gachaButton;
    public Button menuButton;

    public Action homeButtonClick;
    public Action charButtonClick;
    public Action storyButtonClick;
    public Action questButtonClick;
    public Action guildHouseButtonClick;
    public Action gachaButtonClick;
    public Action menuButtonClick;

    /// <summary>
    /// �N���b�N�A�N�V�������A�^�b�`���Ă��邩
    /// </summary>
    public bool IsAttachedClickAction { get ; protected set; }

    private UnderCommonMenu() {}

    private UnderCommonMenu(VisualElement selfSource)
    {
        SetupElements(selfSource);
    }
    ~UnderCommonMenu()
    {
        //Logger.SetEnableLogging(true);
        //Logger.Debug($"����@�f�X�g���N�^ {this}");

        // �N���b�N�A�N�V�������A�^�b�`����܂ł̓A�N�V�����������z��
        if (!IsAttachedClickAction)
        {
            Logger.Warning($"{typeof(UnderCommonMenu)}�� AttachClickAction �ŃA�^�b�`���邱�Ƃ�z�肵�Ă��܂����A���s����Ă��܂���B");
            return;
        }

        homeButton.clickable.clicked -= homeButtonClick;
        charButton.clickable.clicked -= charButtonClick;
        storyButton.clickable.clicked -= storyButtonClick;
        questButton.clickable.clicked -= questButtonClick;
        guildHouseButton.clickable.clicked -= guildHouseButtonClick;
        gachaButton.clickable.clicked -= gachaButtonClick;
        menuButton.clickable.clicked -= menuButtonClick;
    }


    void OnEnabele()
    {
        //SetupElements();
    }

    private void SetupElements(VisualElement selfSource)
    {
        homeButton = selfSource.Q<Button>("homeButton");
        charButton = selfSource.Q<Button>("charButton");
        storyButton = selfSource.Q<Button>("storyButton");
        questButton = selfSource.Q<Button>("questButton");
        guildHouseButton = selfSource.Q<Button>("guildHouseButton");
        gachaButton = selfSource.Q<Button>("gachaButton");
        menuButton = selfSource.Q<Button>("menuButton");
    }

    /// <summary>
    /// �N���b�N�A�N�V�������A�^�b�`����
    /// </summary>
    public void AttachClickAction()
    {
        IsAttachedClickAction = true;

        homeButton.clickable.clicked += homeButtonClick;
        charButton.clickable.clicked += charButtonClick;
        storyButton.clickable.clicked += storyButtonClick;
        questButton.clickable.clicked += questButtonClick;
        guildHouseButton.clickable.clicked += guildHouseButtonClick;
        gachaButton.clickable.clicked += gachaButtonClick;
        menuButton.clickable.clicked += menuButtonClick;
    }

    /// <summary>
    /// �R���g���[���𐶐�����<br/>
    /// VisualElement.Q<UnderCommonMenu>("UnderCommonMenu")����<br/>
    /// </summary>
    /// <param name="elm"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static UnderCommonMenu Q(VisualElement elm)
    {
        var selfSource = elm.Q<VisualElement>(RootName);
        if (selfSource == null)
        {
            throw new ArgumentException($"{typeof(UnderCommonMenu)}���擾���悤�Ƃ��܂������A�Q�ƌ���VisualElement {elm.name} �ɂ�UXML���o�^����Ă��܂���ł����B");
        }
        return new UnderCommonMenu(selfSource);
    }
}