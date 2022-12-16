using Cysharp.Threading.Tasks;
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
/// SceneLayer��Ή��K�w�i�풓�V�[���j
/// </summary>
public interface ILayeredSceneResident : ILayeredScene { } 

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

// TODO���Q��
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
/// ���ɋ@�\�������Ȃ����^���ʂ̂��߂̃C���^�[�t�F�[�X
/// </summary>
public interface IDomainBaseParam
{
}

/// <summary>
/// ���O�̃T�C�N���^�C�~���O����
/// </summary>
public interface ITiming
{
    /// <summary>
    /// �J�n���Ăяo��
    /// </summary>
    public UniTask Initialize();

    /// <summary>
    /// ��~���Ăяo��
    /// </summary>
    public UniTask Suspend();

    /// <summary>
    /// �ĊJ���Ăяo��
    /// </summary>
    public UniTask Resume();
    /// <summary>
    /// �I�����Ăяo��
    /// </summary>
    public UniTask Discard();
}

public interface ILayeredSceneDomain: ITiming
{
    public IDomainBaseParam Param { get; set; }
    public IDomainBaseParam InitialParam { get; set; }

    public ILayeredSceneLogic LogicLayer { get; set; }
    public ILayeredSceneUI UILayer { get; set; }
    public ILayeredSceneField FieldLayer { get; set; }
}

public abstract class DomainBase<TLogic, TUI, TField, TParam> : ILayeredSceneDomain
    where TLogic : ILayeredSceneLogic
    where TUI    : ILayeredSceneUI
    where TField : ILayeredSceneField
    where TParam : IDomainBaseParam
{

    public IDomainBaseParam Param {
        get => _param;
        set { if (value is TParam param) _param = param; }
    }
    protected TParam _param;

    public IDomainBaseParam InitialParam  {
        get => _initialParam;
        set { if (value is TParam param) _initialParam = param; }
    }
    protected TParam _initialParam;

    public ILayeredSceneLogic LogicLayer {
        get => _logicLayer;
        set { if (value is TLogic param) _logicLayer = param; }
    }
    protected TLogic _logicLayer;

    public ILayeredSceneUI UILayer {
        get => _uiLayer;
        set { if (value is TUI param) _uiLayer = param; }
    }
    public TUI _uiLayer;

    public ILayeredSceneField FieldLayer {
        get => _fieldLayer;
        set { if (value is TField param) _fieldLayer = param; }
    }
    protected TField _fieldLayer;

    public virtual UniTask Initialize() { return UniTask.Delay(0); }
    public virtual UniTask Suspend() { return UniTask.Delay(0); }
    public virtual UniTask Resume() { return UniTask.Delay(0); }
    public virtual UniTask Discard() { return UniTask.Delay(0); }
}




/// <summary>
/// Null�I�u�W�F�N�g
/// </summary>
public sealed class NullDomain : DomainBase<
    NullDomain.NullLayeredSceneLogic,
    NullDomain.NullLayeredSceneUI,
    NullDomain.NullLayeredSceneField,
    NullDomain.DomainParam>
{
    public class NullLayeredSceneLogic : ILayeredSceneLogic
    {}
    public class NullLayeredSceneUI : ILayeredSceneUI
    {}
    public class NullLayeredSceneField : ILayeredSceneField
    {}

    public class DomainParam : IDomainBaseParam
    {}
    
    private NullDomain()
    {
        LogicLayer = new NullLayeredSceneLogic();
        UILayer = new NullLayeredSceneUI();
        FieldLayer = new NullLayeredSceneField();
    }

    public static NullDomain Create()
    {
        return new NullDomain();
    }
}