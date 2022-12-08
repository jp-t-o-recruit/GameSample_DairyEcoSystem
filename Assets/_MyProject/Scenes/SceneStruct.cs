
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// 1シーン内の階層割り当てシーン
/// </summary>
public enum SceneLayer
{
    Logic, // AudioListenerやCanvasなど集約はここにする
    UI,
    Field,
}


/// <summary>
/// SceneLayer対応階層基盤
/// 階層割り当てシーンの階層情報をインターフェースとして持たせる
/// </summary>
public interface ILayeredScene { }
/// <summary>
/// SceneLayer対応階層 Logic
/// </summary>
public interface ILayeredSceneLogic : ILayeredScene { }
/// <summary>
/// SceneLayer対応階層 UI
/// </summary>
public interface ILayeredSceneUI : ILayeredScene { }
/// <summary>
/// SceneLayer対応階層 Field 
/// </summary>
public interface ILayeredSceneField : ILayeredScene { }

/// <summary>
/// 階層割り当て構成したシーンスクリプトのまとまり
/// </summary>
public class LayeredSceneDefinition
{
    private Dictionary<SceneLayer, MonoBehaviour> _layers;

    public class NullScene: MonoBehaviour
    {
    }

    [Inject]
    public LayeredSceneDefinition(Dictionary<SceneLayer, MonoBehaviour> layers)
    {
        _layers = layers;
        FillEmptyLayeredSceneForNullScene();
    }


    public MonoBehaviour GetSceneByLayer(SceneLayer layer)
    {
        return _layers[layer];
    }

    public void FillEmptyLayeredSceneForNullScene()
    {
        foreach (SceneLayer current in Enum.GetValues(typeof(SceneLayer)))
        {
            _layers[current] ??= new NullScene();
        }
    }
}

/// <summary>
/// ルートシーン列挙
/// </summary>
enum SceneEnum
{
    GameManagersScene,
    TitleScene,
    HomeScene,
    TutorialScene,
    CreditNotationScene,
}