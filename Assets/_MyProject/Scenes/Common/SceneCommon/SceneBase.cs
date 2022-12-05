using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SceneBase<TParam> : MonoBehaviour
{
    public TParam CreateParam { get; set; }

    /// <summary>
    /// UIDocumentアタッチ必須
    /// </summary>
    [SerializeField] protected UIDocument _uiDocument;

    /// <summary>
    /// UIDocumentのRoot要素(ヒエラルキー最上位)を返す
    /// </summary>
    protected VisualElement RootElement { get => _uiDocument.rootVisualElement; }
}