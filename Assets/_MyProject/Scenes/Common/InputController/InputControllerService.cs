using UnityEngine;

public enum ScaleMode
{
    Up,
    Even,
    Down,
}
public static class InputControllerService
{
    private static ScaleMode _mode;
    public static void CheckInput()
    {
        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    _mode = ScaleMode.Up;
        //}
        //else if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    _mode = ScaleMode.Down;
        //}
        //else
        //{
        //    _mode = ScaleMode.Even;
        //}
    }
    public static void UpdateScale()
    {
        // EventSystemでPSのEnableをHomeScenePS内PanelRaycasterをON→OFF→ONにするとアクティブシーンでないGameManagerの子の部品もクリックできる
        // 似たような事
        // https://hacchi-man.hatenablog.com/entry/2021/12/03/220000
        // PanelSettingsのSortOrderでパネル≒UIToolKitの前後関係を設定（正の数で手前）して前にすればもともと触れる

        if (_mode == ScaleMode.Up)
        {
        }
        else if (_mode == ScaleMode.Down)
        {
        }
        else if (_mode == ScaleMode.Even)
        {
        }
    }
}
