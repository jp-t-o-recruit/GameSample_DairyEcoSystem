using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
public interface ISceneTransitioner: IDisposable
{
    /// <summary>
    /// �J�ڂł̃V�[���X�^�b�N�^�C�v
    /// </summary>
    SceneStackType StackType { get; set; }
    ILayeredSceneDomain Domain { get; set; }

    /// <summary>
    /// �V�[�����̃��[�h
    /// </summary>
    /// <returns></returns>
    UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// �V�[�����̃A�����[�h
    /// </summary>
    /// <returns></returns>
    UniTask UnLoadScenes(CancellationTokenSource cts);
    /// <summary>
    /// ���̃V�[���J�ڏ��Ɋ�Â��V�[���J�ڎ��{
    /// </summary>
    /// <returns></returns>
    UniTask Transition(CancellationTokenSource cts);
    /// <summary>
    /// �V�[���̖��O�擾
    /// </summary>
    /// <returns></returns>
    string GetSceneName();
}

/// <summary>
/// �K�w�\���V�[���J�ڏ���
/// </summary>
public class LayeredSceneTransitioner : ISceneTransitioner
{
    public SceneStackType StackType { get; set; }
    public ILayeredSceneDomain Domain { get; set; }

    /// <summary>
    /// TODO�@���̃V�[���܂Ƃ܂肪�����񃍁[�h����Ă��ǂ���
    /// 
    /// ItemDetailScene
    /// ��BattleSelectScene
    /// �@��ItemDetailScene
    /// �@ �̂悤�ɕ����񃍁[�h������������̃V�[�����擾��Q�Ƃ���̂����Ȃ����Ȃ�
    /// </summary>
    //public bool CanMultipleLoad = false;

    Func<OuterLoader, CancellationTokenSource, UniTask> _preSetCallback;


    [Inject]
    public LayeredSceneTransitioner(ILayeredSceneDomain domain, Func<OuterLoader, CancellationTokenSource, UniTask> callback)
    {
        Domain = domain;
        _preSetCallback = callback;
        StackType = SceneStackType.ReplaceAll;
        Logger.SetEnableLogging(true);
    }
    public void Dispose()
    {
        Domain = null;
    }

    public class OuterLoader
    {
        public List<Scene> scenes;
        public MonoBehaviour SceneBase;

        LayeredSceneTransitioner _owner;
        public OuterLoader(LayeredSceneTransitioner owner)
        {
            scenes = new();
            _owner = owner;
        }

        public async UniTask<TType> GetAsyncComponentByScene<TType>(CancellationTokenSource cts)
        {
            TType result = default;
            await _owner.LoadScene<TType>(cts, (scene, sceneBase) => {
                scenes.Add(scene);
                result = sceneBase;
                if (sceneBase is MonoBehaviour)
                {
                    SceneBase = sceneBase as MonoBehaviour;
                }
            });
            return result;
        }
    }

    public async virtual UniTask<List<Scene>> LoadScenes(CancellationTokenSource cts)
    {
        OuterLoader outer = new (this);
        await _preSetCallback(outer, cts);

        List<Scene> scenes = outer.scenes;

        if (outer.SceneBase == default || outer.SceneBase == null)
        {
            Logger.Warning($"{GetSceneName()}�ɃV�[���̃R���|�[�l���g��Hierarchy�ɐݒ肳��Ă��Ȃ��B");
        }

        // �v���C���[���[�v�����܂Ȃ��ƃA�^�b�`�����ꂸ�Q�ƕs�S�ɂȂ�̂ő҂�
        await UniTask.WaitForEndOfFrame(outer.SceneBase);

        Domain.Initialize(cts);

        return scenes;
    }

    public async UniTask LoadScene<TComponent>(CancellationTokenSource cts, Action<Scene, TComponent> callBack)
    {
        string sceneName = typeof(TComponent).Name;

        Scene scene = await LoadSceneByName(sceneName, cts);
        if (scene.IsValid())
        {
            callBack(scene,
                     ExSceneManager.Instance.GetSceneBaseFromScene<TComponent>(scene));
        }
    }
    protected async UniTask<Scene> LoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        Logger.Debug($"LoadSceneByName {sceneName} isLoaded:{scene.isLoaded},isDirty:{scene.isDirty},IsValid:{scene.IsValid()}");
        if (!scene.IsValid())
        {
            Logger.Debug($"LoadSceneByName {sceneName} �ǂݍ��݊J�n");
            // TODO �Ȃ񂩃G���[�ł�H
            // InvalidOperationException: This can only be used during play mode, please use EditorSceneManager.OpenScene() instead.
            //await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).WithCancellation(cts.Token);
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            scene = SceneManager.GetSceneByName(sceneName);
            Logger.Debug($"LoadSceneByName {sceneName} �ǂݍ��ݏI��? : " + scene.isLoaded);
        }
        return scene;
    }

    public async virtual UniTask UnLoadScenes(CancellationTokenSource cts)
    {
        foreach (var pair in Domain.GetLayerNames())
        {
            await UnLoadSceneByName(pair.Value, cts);
        }
    }

    private async UniTask UnLoadSceneByName(string sceneName, CancellationTokenSource cts)
    {
        Logger.Debug("�A�����[�h�J�n:" + sceneName);
        await SceneManager.UnloadSceneAsync(sceneName).WithCancellation(cts.Token);
        await Resources.UnloadUnusedAssets().WithCancellation(cts.Token);
        Logger.Debug("�A�����[�h�I��:" + sceneName);
    }

    public async UniTask Transition(CancellationTokenSource cts)
    {
        await ExSceneManager.Instance.Transition(this, cts);
    }

    public string GetSceneName()
    {
        return Domain.GetSceneName();
    }

}

///// <summary>
///// �K�w�\���������Ȃ��V�[���i������Logic�w�Ƃ��Ĕ��肷��j
///// </summary>
///// <typeparam name="TParam"></typeparam>
//public abstract class SoloLayerSceneTransitioner : LayeredSceneTransitioner
//{
//    protected SoloLayerSceneTransitioner(ILayeredSceneDomain domain) : base(domain)
//    {

//    }
//}
