using UnityEditor;
using UnityEngine.UIElements;

using Logger = MyLogger.MapBy<UIToolKitTemplateTips>;

namespace My
{ }



/// <summary>
/// UIToolkitのカスタム部品化は次の通り。
/// 
/// 1.UXMLのtemplateとして部品化。
/// ┣UXMLのテキストベースで管理。
/// ┗UIBuilderで表示しながら調整でき、構造を目視しやすい。
/// 
/// 2.C#ScriptでUxmlFactoryを実装して実現。
/// ┣UIBuilderでカスタム部品として選択できる。
/// ┗部品アクションをそのまま実装できる
/// 
/// 
/// いずれも部品化としては問題点がある。
/// 
/// 1.C#のclassをコントローラーとして紐づけされない。UIBuilderでカスタム部品として表示されない。
/// element = RootElement.Q<CostomItem>("elementName"); のように標準のButtonなどのように取得したいができない。
/// ScriptのUxmlFactoryをうまく実装できれば可能かもしれないがAPIに詳細が記載されていない。
/// 
/// 2.UXMLをコード的に組み立てるのでUIBuilderでグラフィカルに調整できるメリットが激減する。
/// 
/// 
/// 
/// 
/// チュートリアル文献
/// LIGHT11　UIElements の検索結果
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
        // TODO Resourcesフォルダ内であればこっちでも取得できるらしい
        //var uxml = Resources.Load <VisualTreeAsset>(path);
        //VisualElement tree = uxml.Instantiate();

        var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        Logger.SetEnableLogging(true);
        if (treeAsset == null)
        {
            Logger.Warning($"UnderCommonMenu VisualTreeAsset がない　Path : {path}");
        }
        else
        {
            Logger.Debug($"UnderCommonMenu VisualTreeAsset はある Path : {path} name : {treeAsset.name}");
        }
        return treeAsset;
    }

    private static VisualTreeAsset SetupRoot(UIDocument uiDocument)
    {
        Logger.SetEnableLogging(true);
        Logger.Debug($"UnderCommonMenu _uiDocument はあるか? : " + (uiDocument != null ? "ある" : "ない"));
        VisualElement rootElement = uiDocument.rootVisualElement;
        return rootElement.visualTreeAssetSource;
    }

    private static TemplateContainer Instantiate(VisualElement elm, VisualTreeAsset treeAsset)
    {
        var container = treeAsset.CloneTree();
        //var container = treeAsset.Instantiate();

        // Addによって追加される側が親ツリーから取り除かれるので、改めて親にAddすることで構造をキープする
        // TODO ただしUSS構造が壊れる
        elm.hierarchy.Add(container);
        return container;
    }

    public static TemplateContainer Setup(VisualElement elm, string path)
    {
        // UXMLのロード
        var treeAsset = MyLoad(path);
        return Instantiate(elm, treeAsset);
    }

    public static TemplateContainer Setup(VisualElement elm, UIDocument uiDocument)
    {
        // UXMLのロード
        var treeAsset = SetupRoot(uiDocument);
        return Instantiate(elm, treeAsset);
    }
}

public abstract class TipsBase : VisualElement
{
    /// <summary>
    /// テンプレートのパス
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
/// Tips1パターン
/// </summary>
public class UnderCommonMenu1 : TipsBase
{
    /// <summary>
    /// 利用側（MonoBehaviour)でUIDocumentを渡してもらい、該当テンプレートを読み込む
    /// </summary>
    /// <param name="uiDocument"></param>
    public UnderCommonMenu1(UIDocument uiDocument)
    {
        UIToolkitTemplateSetupper.Setup(this, uiDocument);
        SetupElements();
    }

    /// 利用側（MonoBehaviour)でrootElementを渡してもらい、該当テンプレートを読み込む
    //public UnderCommonMenu1(VisualElement template)
    //{
    //    this.hierarchy.Add(template);
    //    SetupElements();
    //}
}

/// <summary>
/// Tips２パターン
/// </summary>
public class UnderCommonMenu2 : TipsBase
{
    public UnderCommonMenu2()
    {
        UIToolkitTemplateSetupper.Setup(this, UXMLPath);
        SetupElements();
    }

    /// <summary>
    /// UIBuilder上でも配置できるようにする
    /// https://www.docswell.com/s/UnityJapan/ZLDPGK-sync2022_day1_track2_2050#p25
    /// </summary>
    public new class UxmlFactory : UxmlFactory<UnderCommonMenu2, UxmlTraits>
    {
        // やり方不明
        //public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
        //{
        //    //VisualElement elm = base.Create(bag, cc) as UnderCommonMenu;
        //    UnderCommonMenu2 menu = base.Create(bag, cc) as UnderCommonMenu2;
        //    //UnderCommonMenu menu = new UnderCommonMenu();
        //    return UIToolkitTemplateSetupper.Setup(menu, UnderCommonMenu2.UXMLPath);
        //}
    }

    /// <summary>
    /// 概要:
    ///     Defines UxmlTraits for the UnderCommonMenu.
    /// </summary>
    public new class UxmlTraits : TextElement.UxmlTraits
    {
        ///// <summary>
        ///// 子要素を持たないことを示す定義
        ///// </summary>
        //public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        //{
        //    get { yield break; }
        //}

        //
        // 概要:
        //     Constructor.
        public UxmlTraits()
        {
            base.focusable.defaultValue = false;
        }
    }
}

