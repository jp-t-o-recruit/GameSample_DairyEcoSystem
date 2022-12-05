using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//using Logger = MyLogger.MapBy<CustomButton>;

public class CustomButton : MonoBehaviour,
    IPointerClickHandler, //..�{�^����������A���̌�h���b�O���삪���邱�ƂȂ��{�^�����������
    IPointerDownHandler, //..�{�^�����������
    IPointerUpHandler, //..�{�^�����������
    IPointerEnterHandler, //..�{�^���͈̔͂Ƀ}�E�X�J�[�\��������
    IPointerExitHandler //..�{�^���͈̔͂���}�E�X�J�[�\�����o��
{
    ///// <summary>
    ///// ���K�[
    ///// </summary>
    //[SerializeField] private bool IsLogging
    //{
    //    get => Logger.GetEnableLogging();
    //    set => Logger.SetEnableLogging(value);
    //}

    /// <summary>
    /// Pointer�̏��
    /// </summary>
    public enum PointerState
    {
        Default = 1,
        Hover = 2,
        Press = 3
    }

    /// <summary>
    /// �{�^���̗L���������
    /// </summary>
    public enum ButtonEnableState
    {
        Enabled = 1,
        Disabled = 0
    }

    /// <summary>
    /// �{�^���̃t�H�[�J�X���
    /// </summary>
    public enum ButtonFocusState
    {
        InActive = 0,
        Focused = 1
    }

    /// <summary>
    /// �g�O�����
    /// </summary>
    public enum ButtonToggleState
    {
        Inactive = 1,
        Active = 2
    }

    /// <summary>
    /// �A�Ŗh�~�@�\
    /// </summary>
    public bool IsPromiseOneClick = false;

    public System.Action onPointerClickCallback;
    public System.Action onPointerDownCallback;
    public System.Action onPointerUpCallback;
    public System.Action onPointerEnterCallback;
    public System.Action onPointerExitCallback;

    class ButtonColorAssets
    {
        public Color32 DefaultColor;
        public Color32 HighlightedColor;
        public Color32 PressedColor;
        public Color32 SelectedColor;
        public Color32 DisabledColor;
    }
    class ButtonColorManager
    {
        public Color32 DefaultColor = Color.white;
        public Color32 HighlightedColor = Color.white;
        public Color32 PressedColor = Color.white;
        public Color32 SelectedColor = Color.white;
        public Color32 DisabledColor = Color.white;
        public Color32 DisabledTextColor = Color.black;
    }

    [SerializeField] private Image Image;
    private ButtonColorManager _bcm;

    /// <summary>
    /// �{�^���A�j���[�V��������
    /// </summary>
    private ButtonAnimation _ba;

    private ButtonFocusState _focusState = ButtonFocusState.InActive;
    private PointerState _pointerState = PointerState.Default;

    /// <summary>
    /// �{�^���L�����
    /// </summary>
    [SerializeField] public bool IsDisabled
    {
        get => _IsDisabled == ButtonEnableState.Disabled;
        set
        {
            if (value != IsDisabled)
            {
                if (value)
                    Inactivate();
                else
                    Activate();
            }
        }
    }
    ButtonEnableState _IsDisabled = ButtonEnableState.Enabled;

    void Start()
    {
        _ba = new ButtonAnimation(transform);
        _bcm = new ButtonColorManager();
        BuildupBtnColorAssets();
    }

    void Update()
    {
        _ba.Update();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ButtonActionTemplate(onPointerClickCallback , () => {
            _pointerState = PointerState.Default;
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonActionTemplate(onPointerDownCallback, () => {
            _ba.StartAnim(0.05f,
                          new Vector3(0.9f, 0.9f, 0f));
            _pointerState = PointerState.Press;
        });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ButtonActionTemplate(onPointerUpCallback, () => {
            _ba.StartAnim(0.2f,
                          new Vector3(1.0f, 1.0f, 0f));
            _pointerState = PointerState.Default;
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ButtonActionTemplate(onPointerEnterCallback, () => {
            _pointerState = PointerState.Hover;
        });
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonActionTemplate(onPointerExitCallback, () => {
            _pointerState = PointerState.Default;
        });
    }

    public void OnFocus()
    {
        // TODO:�t�H�[�J�X�̎d�g�݂�������Ȃ��̂ō��̂Ƃ���Ă΂�Ȃ�
        _focusState = ButtonFocusState.Focused;
        Refresh();
    }
    public void OnUnFocus()
    {
        // TODO:�t�H�[�J�X�̎d�g�݂�������Ȃ��̂ō��̂Ƃ���Ă΂�Ȃ�
        _focusState = ButtonFocusState.InActive;
        Refresh();
    }

    private void ButtonActionTemplate(Action buttonCallback, Action templateBehavior)
    {
        if (!IsDisabled)
        {
            if (buttonCallback?.GetInvocationList() != null && IsPromiseOneClick)
            {
                Inactivate();
            }
            buttonCallback?.Invoke();
            templateBehavior();
        }
        Refresh();
    }

    public void Activate()
    {
        _IsDisabled = ButtonEnableState.Enabled;
        Refresh();
    }
    private void Inactivate()
    {
        _IsDisabled = ButtonEnableState.Disabled;
        Refresh();
    }


    /// <summary>
    /// ��Ԃɂ��r�W���A���X�V
    /// </summary>
    void Refresh()
    {
        Color32 newColor = Color.white;
        // �J���[�ݒ�
        if (IsDisabled)
        {
            newColor = _bcm.DisabledColor;
        } 
        else
        {
            switch (_pointerState)
            {
                case PointerState.Default:
                    if (_focusState == ButtonFocusState.Focused)
                    {
                        newColor = _bcm.SelectedColor;
                    }
                    else
                    {
                        newColor = _bcm.DefaultColor;
                    }
                    break;
                case PointerState.Press:
                    newColor = _bcm.PressedColor;
                    break;
                case PointerState.Hover:
                    newColor = _bcm.HighlightedColor;
                    break;
            }
        }
        ChangeColor(newColor);
    }

    private void BuildupBtnColorAssets()
    {
        if (Image == null)
        {
            return;
        }

        //new Color32(80, 80, 80, 255)
        _bcm.DefaultColor = Image.GetComponent<Image>().color;

        _bcm.DisabledColor = Color32.LerpUnclamped(_bcm.DefaultColor, Color.gray, 0.5f);
        _bcm.HighlightedColor = Color32.LerpUnclamped(_bcm.DefaultColor, Color.blue, 0.5f);
        _bcm.PressedColor = Color32.LerpUnclamped(_bcm.DefaultColor, Color.gray, 0.8f);
        _bcm.SelectedColor = Color32.LerpUnclamped(_bcm.DefaultColor, Color.blue, 0.8f);
    }

    private void ChangeColor(Color32 newColor)
    {
        if (Image != null)
        {
            if (Image.GetComponent<Image>().color != newColor)
            {
                Image.GetComponent<Image>().color = newColor;
            }
        }
    }
}
