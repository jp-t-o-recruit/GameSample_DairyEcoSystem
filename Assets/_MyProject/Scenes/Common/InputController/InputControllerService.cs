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
        // EventSystem��PS��Enable��HomeScenePS��PanelRaycaster��ON��OFF��ON�ɂ���ƃA�N�e�B�u�V�[���łȂ�GameManager�̎q�̕��i���N���b�N�ł���
        // �����悤�Ȏ�
        // https://hacchi-man.hatenablog.com/entry/2021/12/03/220000
        // PanelSettings��SortOrder�Ńp�l����UIToolKit�̑O��֌W��ݒ�i���̐��Ŏ�O�j���đO�ɂ���΂��Ƃ��ƐG���

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
