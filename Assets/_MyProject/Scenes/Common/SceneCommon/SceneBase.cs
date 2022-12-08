using UnityEngine;
using UnityEngine.UIElements;

public abstract class SceneBase : MonoBehaviour
{
    /// <summary>
    /// UIDocument�A�^�b�`�K�{
    /// </summary>
    [SerializeField] protected UIDocument _uiDocument;

    /// <summary>
    /// UIDocument��Root�v�f(�q�G�����L�[�ŏ��)��Ԃ�
    /// </summary>
    protected VisualElement RootElement { get => _uiDocument?.rootVisualElement; }
}