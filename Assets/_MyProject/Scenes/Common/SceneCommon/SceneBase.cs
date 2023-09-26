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
    protected VisualElement RootElement => _uiDocument?.rootVisualElement;

    /// <summary>
    /// ���d���͖h�~�t���O
    /// </summary>
    internal protected bool IsInputLock = false;

    /// <summary>
    /// UIDocument�̗v�f�擾
    /// </summary>
    /// <typeparam name="TVisualElement"></typeparam>
    /// <param name="elementName">�v�f��</param>
    /// <returns></returns>
    protected TVisualElement Pull<TVisualElement>(string elementName) where TVisualElement : VisualElement
    {
        return RootElement.Q<TVisualElement>(elementName);
    }
}