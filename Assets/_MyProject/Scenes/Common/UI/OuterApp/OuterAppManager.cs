using UnityEngine;
using System;
using UnityEngine.UIElements;

public class OuterAppManager : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    Label DayInfo;
    Button ResetButton;
    Button NextButton;
    Button PrevButton;

    /// <summary>
    /// 現在日付
    /// </summary>
    public DateTime CurrentDay => _currentDay;
    private DateTime _currentDay;
    public DateTime StartDay => _startDay;
    private DateTime _startDay;

    /// <summary>
    /// 現在通算日数
    /// </summary>
    public int CurrentDayCount
    {
        get { return CurrentDay.Subtract(StartDay).Days; }
    }

    // Start is called before the first frame update
    void Start()
    {
        var rootElement = _uiDocument.rootVisualElement;
        DayInfo = rootElement.Q<Label>("DayInfo");
        ResetButton = rootElement.Q<Button>("resetButton");
        PrevButton = rootElement.Q<Button>("prevButton");
        NextButton = rootElement.Q<Button>("nextButton");

        ResetButton.clickable.clicked += ClickResetButton;
        PrevButton.clickable.clicked += ClickPrevButton;
        NextButton.clickable.clicked += ClickNextButton;

        ResetDay(DateTime.Now);
    }

    // Update is called once per frame
    void Update()
    {
    }
       
    private void OnDestroy()
    {
        ResetButton.clickable.clicked -= ClickResetButton;
        PrevButton.clickable.clicked -= ClickPrevButton;
        NextButton.clickable.clicked -= ClickNextButton;
    }

    /// <summary>
    /// 表示リフレッシュ
    /// </summary>
    private void Refresh()
    {
        DayInfo.text = "日付:" + CurrentDay.ToShortDateString() + "\n日数：" + CurrentDayCount.ToString();
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
}
