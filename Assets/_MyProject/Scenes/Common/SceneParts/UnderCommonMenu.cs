using System;
using UnityEngine.UIElements;

using Logger = MyLogger.MapBy<UnderCommonMenu>;

// Unity User Manual 2021.2ユーザーインターフェース (UI) の作成UI Toolkitコントロール  コントロールリファレンス
// https://docs.unity3d.com/ja/2021.2/Manual/UIE-Controls.html

/// <summary>
/// 画面下共通メニューのコントローラー<br/>
/// <br/>
/// メニュー自体はUXMLのテンプレート機能としてUIDocumentに使用されている想定。<br/>
/// .QでUnderCommonMenu相当のVisualElementを内部生成し保持する。<br/>
/// </summary>
public class UnderCommonMenu
{
    /// <summary>
    /// パーツ名
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
    /// クリックアクションをアタッチしているか
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
        //Logger.Debug($"解放　デストラクタ {this}");

        // クリックアクションをアタッチするまではアクションが無い想定
        if (!IsAttachedClickAction)
        {
            Logger.Warning($"{typeof(UnderCommonMenu)}は AttachClickAction でアタッチすることを想定していますが、実行されていません。");
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
    /// クリックアクションをアタッチする
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
    /// コントローラを生成する<br/>
    /// VisualElement.Q<UnderCommonMenu>("UnderCommonMenu")相当<br/>
    /// </summary>
    /// <param name="elm"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static UnderCommonMenu Q(VisualElement elm)
    {
        var selfSource = elm.Q<VisualElement>(RootName);
        if (selfSource == null)
        {
            throw new ArgumentException($"{typeof(UnderCommonMenu)}を取得しようとしましたが、参照元のVisualElement {elm.name} にはUXMLが登録されていませんでした。");
        }
        return new UnderCommonMenu(selfSource);
    }
}