using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting.YamlDotNet.Core;

using Logger = MyLogger.MapBy<OuterAppManager>;
using System.Linq;

public class OuterAppManager : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    public static readonly string CloseClass = "outer-app-manager--close";
    public static readonly string ErrorInputClass = "add-days-input--error";

    VisualElement RootElement;
    VisualElement FirstElement;
    Label DayInfo;
    Button ResetButton;
    Button NextButton;
    Button PrevButton;
    Button AddDaysButton;
    TextField AddDayText;
    Button ViewToggleButton;
    /// <summary>
    /// 現在日付
    /// </summary>
    public DateTime CurrentDay => _currentDay;
    private DateTime _currentDay;
    public DateTime StartDay => _startDay;
    private DateTime _startDay;

    public bool IsOpen { get => _isOpen; set => ToggleView(value); }
    private bool _isOpen = false;

    /// <summary>
    /// 現在通算日数
    /// </summary>
    public int CurrentDayCount
    {
        get => CurrentDay.Subtract(StartDay).Days;
    }

    void OnEnable()
    {
        Logger.SetEnableLogging(true);
        RootElement = _uiDocument.rootVisualElement;
        FirstElement = RootElement.Q<VisualElement>("OuterAppManager");
        DayInfo = RootElement.Q<Label>("DayInfo");
        ResetButton = RootElement.Q<Button>("resetButton");
        PrevButton = RootElement.Q<Button>("prevButton");
        NextButton = RootElement.Q<Button>("nextButton");
        AddDayText = RootElement.Q<TextField>("addDayInput");
        AddDaysButton = RootElement.Q<Button>("addDaysButton");
        ViewToggleButton = RootElement.Q<Button>("outerViewToggle");

        ResetButton.clickable.clicked += ClickResetButton;
        PrevButton.clickable.clicked += ClickPrevButton;
        NextButton.clickable.clicked += ClickNextButton;
        AddDaysButton.clickable.clicked += ClickAddDaysButton;
        ViewToggleButton.clickable.clicked += ClickViewToggleButton;

        AddDayText.RegisterValueChangedCallback(ChangeAddDayTextValidateInt);
        ValidateAddDayText(AddDayText.text);
        ToggleView(IsOpen);

        ResetDay(DateTime.Now);
    }
       
    private void OnDestroy()
    {
        ResetButton.clickable.clicked -= ClickResetButton;
        PrevButton.clickable.clicked -= ClickPrevButton;
        NextButton.clickable.clicked -= ClickNextButton;
        AddDaysButton.clickable.clicked -= ClickAddDaysButton;
        ViewToggleButton.clickable.clicked -= ClickViewToggleButton;
    }

    /// <summary>
    /// 表示リフレッシュ
    /// </summary>
    private void Refresh()
    {
        DayInfo.text = "日数：" + CurrentDayCount.ToString() + (IsOpen ? "\n" : "  ") + "日付:" + CurrentDay.ToShortDateString();
    }

    /// <summary>
    /// 初日からにリセット
    /// </summary>
    private void ResetDay(DateTime startDay)
    {
        _startDay = startDay;
        _currentDay = startDay;
    }

    /// <summary>
    /// 日を追加
    /// </summary>
    /// <param name="addDays"></param>
    private void AddDay(int addDays = 1)
    {
        _currentDay = CurrentDay + new TimeSpan(addDays, 0, 0, 0);
    }

    private void ClickResetButton()
    {
        ResetDay(StartDay);
        Refresh();
    }

    private void ClickNextButton()
    {
        AddDay(1);
        Refresh();
    }

    private void ClickPrevButton()
    {
        AddDay(-1);
        Refresh();
    }

    private void ChangeAddDayTextValidateInt(ChangeEvent<string> args)
    {
        ValidateAddDayText(args.newValue);
    }

    /// <summary>
    /// 数値バリデーションと結果によるUI制御
    /// </summary>
    /// <param name="value"></param>
    private void ValidateAddDayText(string value)
    {
        bool valid = true;
        try
        {
            int intParsed = int.Parse(value);
            valid = intParsed >= 0;
        }
        catch
        {
            valid = false;
        }

        if (valid)
        {
            AddDaysButton.pickingMode = PickingMode.Position;
            AddDayText.RemoveFromClassList(ErrorInputClass);
        }
        else
        {
            AddDaysButton.pickingMode = PickingMode.Ignore;
            AddDayText.AddToClassList(ErrorInputClass);
        }
    }

    private void ClickAddDaysButton()
    {
        int addDay = int.Parse(AddDayText.text);
        AddDay(addDay);
        Refresh();
    }
    private void ClickViewToggleButton()
    {
        ToggleView(!_isOpen);
        Refresh();
    }

    private void ToggleView(bool isOpen)
    {
        if (isOpen)
        {
            FirstElement.RemoveFromClassList(CloseClass);
        }
        else
        {
            FirstElement.AddToClassList(CloseClass);
        }
        _isOpen = isOpen;
    }
}
