using UnityEngine;
using UnityEngine.UIElements;

public abstract class SceneBase : MonoBehaviour
{
    /// <summary>
    /// UIDocumentアタッチ必須
    /// </summary>
    [SerializeField] protected UIDocument _uiDocument;

    /// <summary>
    /// UIDocumentのRoot要素(ヒエラルキー最上位)を返す
    /// </summary>
    protected VisualElement RootElement => _uiDocument?.rootVisualElement;

    /// <summary>
    /// 多重入力防止フラグ
    /// </summary>
    internal protected bool IsInputLock = false;

    /// <summary>
    /// UIDocumentの要素取得
    /// </summary>
    /// <typeparam name="TVisualElement"></typeparam>
    /// <param name="elementName">要素名</param>
    /// <returns></returns>
    protected TVisualElement Pull<TVisualElement>(string elementName) where TVisualElement : VisualElement
    {
        return RootElement.Q<TVisualElement>(elementName);
    }
}