using UnityEditor;
using UnityEngine.UIElements;

using Logger = MyLogger.MapBy<UIToolKitTemplateTips>;

namespace My
{ }



/// <summary>
/// UIToolkit�̃J�X�^�����i���͎��̒ʂ�B
/// 
/// 1.UXML��template�Ƃ��ĕ��i���B
/// ��UXML�̃e�L�X�g�x�[�X�ŊǗ��B
/// ��UIBuilder�ŕ\�����Ȃ��璲���ł��A�\����ڎ����₷���B
/// 
/// 2.C#Script��UxmlFactory���������Ď����B
/// ��UIBuilder�ŃJ�X�^�����i�Ƃ��đI���ł���B
/// �����i�A�N�V���������̂܂܎����ł���
/// 
/// 
/// ����������i���Ƃ��Ă͖��_������B
/// 
/// 1.C#��class���R���g���[���[�Ƃ��ĕR�Â�����Ȃ��BUIBuilder�ŃJ�X�^�����i�Ƃ��ĕ\������Ȃ��B
/// element = RootElement.Q<CostomItem>("elementName"); �̂悤�ɕW����Button�Ȃǂ̂悤�Ɏ擾���������ł��Ȃ��B
/// Script��UxmlFactory�����܂������ł���Ή\��������Ȃ���API�ɏڍׂ��L�ڂ���Ă��Ȃ��B
/// 
/// 2.UXML���R�[�h�I�ɑg�ݗ��Ă�̂�UIBuilder�ŃO���t�B�J���ɒ����ł��郁���b�g����������B
/// 
/// 
/// 
/// 
/// �`���[�g���A������
/// LIGHT11�@UIElements �̌�������
/// https://light11.hatenadiary.com/search?q=UIElements
/// 
/// </summary>
public class UIToolKitTemplateTips
{}

public static class UIToolkitTemplateSetupper
{
    /// <summary>
    /// https://technote.qualiarts.jp/article/19#%E6%A7%8B%E7%AF%89%E3%81%97%E3%81%9Fui%E3%81%AE%E4%BD%BF%E7%94%A8%EF%BC%88uxml%EF%BC%89
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static VisualTreeAsset MyLoad(string path)
    {
        // TODO Resources�t�H���_���ł���΂������ł��擾�ł���炵��
        //var uxml = Resources.Load <VisualTreeAsset>(path);
        //VisualElement tree = uxml.Instantiate();

        var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        Logger.SetEnableLogging(true);
        if (treeAsset == null)
        {
            Logger.Warning($"UnderCommonMenu VisualTreeAsset ���Ȃ��@Path : {path}");
        }
        else
        {
            Logger.Debug($"UnderCommonMenu VisualTreeAsset �͂��� Path : {path} name : {treeAsset.name}");
        }
        return treeAsset;
    }

    private static VisualTreeAsset SetupRoot(UIDocument uiDocument)
    {
        Logger.SetEnableLogging(true);
        Logger.Debug($"UnderCommonMenu _uiDocument �͂��邩? : " + (uiDocument != null ? "����" : "�Ȃ�"));
        VisualElement rootElement = uiDocument.rootVisualElement;
        return rootElement.visualTreeAssetSource;
    }

    private static TemplateContainer Instantiate(VisualElement elm, VisualTreeAsset treeAsset)
    {
        var container = treeAsset.CloneTree();
        //var container = treeAsset.Instantiate();

        // Add�ɂ���Ēǉ�����鑤���e�c���[�����菜�����̂ŁA���߂Đe��Add���邱�Ƃō\�����L�[�v����
        // TODO ������USS�\��������
        elm.hierarchy.Add(container);
        return container;
    }

    public static TemplateContainer Setup(VisualElement elm, string path)
    {
        // UXML�̃��[�h
        var treeAsset = MyLoad(path);
        return Instantiate(elm, treeAsset);
    }

    public static TemplateContainer Setup(VisualElement elm, UIDocument uiDocument)
    {
        // UXML�̃��[�h
        var treeAsset = SetupRoot(uiDocument);
        return Instantiate(elm, treeAsset);
    }
}

public abstract class TipsBase : VisualElement
{
    /// <summary>
    /// �e���v���[�g�̃p�X
    /// </summary>
    public static readonly string UXMLPath = "Assets/_MyProject/Scenes/Common/SceneParts/Resources/UnderCommonMenu.uxml";
    public Button InButton;

    protected void SetupElements()
    {
        InButton = this.Q<Button>("homeButton");
    }

    public void CreateGUI() { }

    void OnEnabele()
    {
        //UIToolkitTemplateSetupper.Setup(this, UXMLPath);
        //SetupElements();
    }
    void OnDisable()
    {
    }
}

/// <summary>
/// Tips1�p�^�[��
/// </summary>
public class UnderCommonMenu1 : TipsBase
{
    /// <summary>
    /// ���p���iMonoBehaviour)��UIDocument��n���Ă��炢�A�Y���e���v���[�g��ǂݍ���
    /// </summary>
    /// <param name="uiDocument"></param>
    public UnderCommonMenu1(UIDocument uiDocument)
    {
        UIToolkitTemplateSetupper.Setup(this, uiDocument);
        SetupElements();
    }

    /// ���p���iMonoBehaviour)��rootElement��n���Ă��炢�A�Y���e���v���[�g��ǂݍ���
    //public UnderCommonMenu1(VisualElement template)
    //{
    //    this.hierarchy.Add(template);
    //    SetupElements();
    //}
}

/// <summary>
/// Tips�Q�p�^�[��
/// </summary>
public class UnderCommonMenu2 : TipsBase
{
    public UnderCommonMenu2()
    {
        UIToolkitTemplateSetupper.Setup(this, UXMLPath);
        SetupElements();
    }

    /// <summary>
    /// UIBuilder��ł��z�u�ł���悤�ɂ���
    /// https://www.docswell.com/s/UnityJapan/ZLDPGK-sync2022_day1_track2_2050#p25
    /// </summary>
    public new class UxmlFactory : UxmlFactory<UnderCommonMenu2, UxmlTraits>
    {
        // �����s��
        //public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        //{
        //    //VisualElement elm = base.Create(bag, cc) as UnderCommonMenu;
        //    UnderCommonMenu2 menu = base.Create(bag, cc) as UnderCommonMenu2;
        //    //UnderCommonMenu menu = new UnderCommonMenu();
        //    return UIToolkitTemplateSetupper.Setup(menu, UnderCommonMenu2.UXMLPath);
        //}
    }

    /// <summary>
    /// �T�v:
    ///     Defines UxmlTraits for the UnderCommonMenu.
    /// </summary>
    public new class UxmlTraits : TextElement.UxmlTraits
    {
        ///// <summary>
        ///// �q�v�f�������Ȃ����Ƃ�������`
        ///// </summary>
        //public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        //{
        //    get { yield break; }
        //}

        //
        // �T�v:
        //     Constructor.
        public UxmlTraits()
        {
            base.focusable.defaultValue = false;
        }
    }
}

