using Cysharp.Threading.Tasks;
using PlasticGui.Configuration.CloudEdition.Welcome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Logger = MyLogger.MapBy<ISceneTransitioner>;

/// <summary>
/// �̗p�����V�[���݌v�̊�{�\��
/// https://gamebiz.jp/news/218949
/// </summary>


/// <summary>
/// �V�[���J�ڏ��
/// </summary>
public interface ISceneTransitioner : ITiming
{
    /// <summary>
    /// �J�ڂł̃V�[���X�^�b�N�^�C�v
    /// </summary>
    public SceneStackType StackType { get; set; }

    /// <summary>
    /// �V�[�����̃��[�h
    /// </summary>
    /// <returns></returns>
    public UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// �V�[�����̃A�����[�h
    /// </summary>
    /// <returns></returns>
    public UniTask UnLoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// ���̃V�[���J�ڏ��Ɋ�Â��V�[���J�ڎ��{
    /// </summary>
    /// <returns></returns>
    public UniTask Transition(CancellationTokenSource cts);
    /// <summary>
    /// �V�[���̖��O�擾
    /// </summary>
    /// <returns></returns>
    public string GetSceneName();
}

public class LayerList
{
    private Dictionary<SceneLayer, string> _layer;

    public LayerList()
    {
        _layer = new Dictionary<SceneLayer, string>();
    }

    public LayerList TryAdd<TType>(SceneLayer layer)
    {
        var type = typeof(TType);

        var errorMatch = new Dictionary<SceneLayer, Type>(){
            { SceneLayer.Logic, typeof(ILayeredSceneLogic)},
            { SceneLayer.UI, typeof(ILayeredSceneUI)},
            { SceneLayer.Field, typeof(ILayeredSceneField)},
        }.FirstOrDefault(pair => {
            return pair.Key == layer && !type.IsSubclassOf(pair.Value);
        });

        if (errorMatch.Value != default)
        {
            throw new ArgumentException($"{type.Name}�͎w�肳�ꂽ���C���[�Ƃ��Ă̏���:{errorMatch.Value.Name}���p�����Ă��܂���");
        }
        else
        {
            _layer.Add(layer, type.Name);
        }
            
        return this;
    }
}


public class LayerdSceneTransitioner : ISceneTransitioner
{
    public Dictionary<SceneLayer, System.Type> _layer;
    public SceneStackType StackType { get; set; }

    /// <summary>
    /// ���������n���h��
    /// </summary>
    public event TimingEventHandler InitializeHandler;
    /// <summary>
    /// �ꎞ��~���n���h��
    /// </summary>
    public event TimingEventHandler SuspendHandler;
    /// <summary>
    /// �ĊJ���n���h��
    /// </summary>
    public event TimingEventHandler ResumeHandler;
    /// <summary>
    /// �I�����n���h��
    /// </summary>
    public event TimingEventHandler DiscardHandler;


    /// <summary>
    /// TODO�@���̃V�[���܂Ƃ܂肪�����񃍁[�h����Ă��ǂ���
    /// 
    /// ItemDetailScene
    /// ��BattleSelectScene
    /// �@��ItemDetailScene
    /// �@ �̂悤�ɕ����񃍁[�h������������̃V�[�����擾��Q�Ƃ���̂����Ȃ����Ȃ�
    /// </summary>
    //public bool CanMultipleLoad = false;

    //protected ILayeredSceneDomain _domain;

    //[Inject]
    //public LayerdSceneTransitioner(ILayeredSceneDomain domain = default)
    //{
    //    _domain = domain ?? NullDomain.Create();
    //    SetupLayer();
    //}

    //public void SetupLayer()
    //{
    //    _layer = _domain.GetLayerMap();
    //}
    [Inject]
    public LayerdSceneTransitioner(Dictionary<SceneLayer, System.Type> layer = default)
    {
        _layer = layer ?? new Dictionary<SceneLayer, System.Type>();
        StackType = SceneStackType.Replace;
    }

    public async virtual UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts)
    {
        List<Scene> scenes = new();
        Logger.SetEnableLogging(true);
        SceneBase waitBySceneBase = default;

        await LoadScene<ILayeredSceneLogic>(cts, SceneLayer.Logic, (scene, sceneBase) =>
        {
            scenes.Add(scene);
            //    _domain.LogicLayer = sceneBase;
        });

        await LoadScene<ILayeredSceneUI>(cts, SceneLayer.UI, (UIScene, sceneBase) => {
            scenes.Add(UIScene);
            //    _domain.UILayer = sceneBase;
            waitBySceneBase = (SceneBase)sceneBase;
        });

        await LoadScene<ILayeredSceneField>(cts, SceneLayer.Field, (scene, sceneBase) => {
            scenes.Add(scene);
            //_domain.FieldLayer = sceneBase;
        });

        //TODO������SceneBase��Domain��R�Â������Ƃ����Ȃ��H
        LayerList hoge = new LayerList();
        hoge.TryAdd<ILayeredSceneLogic>(SceneLayer.Logic);
        hoge.TryAdd<ILayeredSceneUI>(SceneLayer.UI);
        hoge.TryAdd<ILayeredSceneField>(SceneLayer.Field);

        // �v���C���[���[�v�����܂Ȃ��ƃA�^�b�`�����ꂸ�Q�ƕs�S�Ȃ̂ő҂�
        await UniTask.WaitForEndOfFrame(waitBySceneBase);
        Initialize(cts);

        return scenes;
    }

    protected async UniTask<Scene> LoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Logger.Debug($"LoadSceneByName {sceneName} isLoaded:{scene.isLoaded},isDirty:{scene.isDirty},IsValid:{scene.IsValid()}");
        if (!scene.IsValid())
        {
            Logger.Debug($"LoadSceneByName {sceneName} �ǂݍ��݊J�n");
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).WithCancellation(cts.Token);
            scene = SceneManager.GetSceneByName(sceneName);
            Logger.Debug($"LoadSceneByName {sceneName} �ǂݍ��ݏI��? : " + scene.isLoaded);
        }
        return scene;
    }
    private async UniTask LoadScene<TComponent>(CancellationTokenSource cts,SceneLayer layer, Action<Scene, TComponent> callBack)
    {
        if (!_layer.ContainsKey(layer))
        {
            return;
        }

        string sceneName = _layer[layer].ToString();
        if (string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        Scene scene = await LoadSceneByName(sceneName, cts);
        if (scene.IsValid())
        {
            callBack(scene,
                     GetSceneBaseFromScene<TComponent>(scene));
        }
    }
    public async virtual UniTask UnLoadScenes(CancellationTokenSource cts)
    {
        foreach (System.Type types in _layer.Values)
        {
            await UnLoadSceneByName(types.ToString(), cts);
        }
    }

    private async UniTask UnLoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Logger.Debug("�A�����[�h�J�n:" + sceneName);
        await SceneManager.UnloadSceneAsync(sceneName).WithCancellation(cts.Token);
        await Resources.UnloadUnusedAssets().WithCancellation(cts.Token);
        Logger.Debug("�A�����[�h�I��:" + sceneName);
    }

    /// <summary>
    /// �V�[���N���X�i�X�N���v�g�j�Ƀp�����[�^���A�^�b�`����
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    public TComponent GetSceneBaseFromScene<TComponent>(Scene scene)
    {
        Logger.Debug("GetSceneBase�擾�J�n:" + scene.name);
        TComponent component = default;

        // GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        // �܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            component = gameObject.GetComponent<TComponent>();
            if (component != null)
            {
                break;
            }
        }

        Logger.Debug("GetSceneBase�擾�I��:" + scene.name);

        return component;
    }
    public async UniTask Transition(CancellationTokenSource cts)
    {
        await ExSceneManager.Instance.Transition(this, cts);

        // TODO
        //if (callback != default)
        //{
        //    var logic = GetSceneBaseFromScene<ILayeredSceneLogic>(_layer[SceneLayer.Logic].ToString());
        //    var ui = GetSceneBaseFromScene<ILayeredSceneUI>(_layer[SceneLayer.UI].ToString());
        //    var field = GetSceneBaseFromScene<ILayeredSceneField>(_layer[SceneLayer.Field].ToString());
        //    var list = new Dictionary<SceneLayer, ILayeredScene>(){
        //        { SceneLayer.Logic, logic },
        //        { SceneLayer.UI, ui },
        //        { SceneLayer.Field, field },
        //    };
        //    callback(list);
        //}
    }

    private TComponent GetSceneBaseFromScene<TComponent>(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return GetSceneBaseFromScene<TComponent>(scene);
    }

    public string GetSceneName()
    {
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        return logicSceneName;
    }

    /// <summary>
    /// �J�n���Ăяo��
    /// </summary>
    public void Initialize(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && InitializeHandler != null)
        {
            InitializeHandler(cts);
        }
        // TODO ���[�h�K�v�H
        //await LoadScenes();
    }
    /// <summary>
    /// ��~���Ăяo��
    /// </summary>
    public void Suspend(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && ResumeHandler != null)
        {
            SuspendHandler(cts);
        }
    }
    /// <summary>
    /// �ĊJ���Ăяo��
    /// </summary>
    public void Resume(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && ResumeHandler != null)
        {
            ResumeHandler(cts);
        }
    }
    /// <summary>
    /// �I�����Ăяo��
    /// </summary>
    public void Discard(CancellationTokenSource cts)
    {
        if (!cts.IsCancellationRequested && DiscardHandler != null)
        {
           DiscardHandler(cts);
        } 
        //await _domain.Discard();
    }
}

/// <summary>
/// �K�w�\���������Ȃ��V�[���i������Logic�w�Ƃ��Ĕ��肷��j
/// </summary>
/// <typeparam name="TParam"></typeparam>
public abstract class SoloLayerSceneTransitioner : LayerdSceneTransitioner
{
}
