
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// 1�V�[�����̊K�w���蓖�ăV�[��
/// </summary>
public enum SceneLayer
{
    Logic, // AudioListener��Canvas�ȂǏW��͂����ɂ���
    UI,
    Field,
}


/// <summary>
/// SceneLayer�Ή��K�w���
/// �K�w���蓖�ăV�[���̊K�w�����C���^�[�t�F�[�X�Ƃ��Ď�������
/// </summary>
public interface ILayeredScene { }
/// <summary>
/// SceneLayer�Ή��K�w Logic
/// </summary>
public interface ILayeredSceneLogic : ILayeredScene { }
/// <summary>
/// SceneLayer�Ή��K�w UI
/// </summary>
public interface ILayeredSceneUI : ILayeredScene { }
/// <summary>
/// SceneLayer�Ή��K�w Field 
/// </summary>
public interface ILayeredSceneField : ILayeredScene { }

/// <summary>
/// �K�w���蓖�č\�������V�[���X�N���v�g�̂܂Ƃ܂�
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
/// ���[�g�V�[����
/// </summary>
enum SceneEnum
{
    GameManagersScene,
    TitleScene,
    HomeScene,
    TutorialScene,
    CreditNotationScene,
}