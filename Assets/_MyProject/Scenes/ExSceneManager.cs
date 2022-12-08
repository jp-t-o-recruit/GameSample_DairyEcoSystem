using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;



/// <summary>
/// 
/// �̗p�����V�[���݌v�̊�{�\��
/// https://gamebiz.jp/news/218949
/// </summary>

public interface ISceneTransition
{
    public abstract UniTask<List<Scene>> LoadScenes();
    public abstract UniTask UnLoadScenes();
}

public abstract class LayerdSceneTransition<TParam> : ISceneTransition where TParam : new()
{
    public TParam Parameter { get; set; }
    internal Dictionary<SceneLayer, System.Type> _layer;

    /// <summary>
    /// ���̃V�[���܂Ƃ܂肪�����񃍁[�h����Ă��ǂ���
    /// 
    /// TODO �����񃍁[�h������������̃V�[�����擾��Q�Ƃ���̂����Ȃ����Ȃ�
    /// </summary>
    //public bool CanMultipleLoad = false;

    public async virtual UniTask<List<Scene>> LoadScenes()
    {
        List<Scene> scenes = new();
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        Scene scene = await LoadSceneByName(logicSceneName);
        scenes.Add(scene);

        string UISceneName = _layer[SceneLayer.UI].ToString();
        Scene UIScene = await LoadSceneByName(UISceneName);
        scenes.Add(UIScene);

        return scenes;
    }
    protected async UniTask<Scene> LoadSceneByName(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (null == scene)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            scene = SceneManager.GetSceneByName(sceneName);
        }
        return scene;
    }

    public async virtual UniTask UnLoadScenes()
    {
        foreach(System.Type types in _layer.Values)
        {
            await UnLoadSceneByName(types.ToString());
        }
    }

    public async UniTask UnLoadSceneByName(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
        await Resources.UnloadUnusedAssets().ToUniTask();
    }

    /// <summary>
    /// �V�[���N���X�i�X�N���v�g�j�Ƀp�����[�^���A�^�b�`����
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    protected SceneBase GetSceneBaseFromScene(Scene scene)
    {
        SceneBase sceneBase = null;

        // GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        // �܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase>();
            if (sceneBase != null)
            {
                break;
            }
        }

        return sceneBase;
    }
}

/// <summary>
/// �K�w�\���������Ȃ��V�[���i������Logic�w�Ƃ��Ĕ��肷��j
/// </summary>
/// <typeparam name="TParam"></typeparam>
public abstract class SoloLayerSceneTransition<TParam> : LayerdSceneTransition<TParam> where TParam : new()
{
    public async override UniTask<List<Scene>> LoadScenes()
    {
        List<Scene> scenes = new();
        string logicSceneName = _layer[SceneLayer.Logic].ToString();
        Scene scene = await LoadSceneByName(logicSceneName);
        scenes.Add(scene);
        return scenes;
    }
}


/// <summary>
/// ����V�[���}�l�[�W���[
/// 
/// ���@�\
/// �E�V�[���\����S��
/// �@��GameManagerScene�i�풓�j
/// �@�@�@��HogeScene
/// �@�@�@��HogeUIScene
/// �@�@�@��HogeFieldScene
/// �ERun���̃R���|�[�l���g�ݒ�𒲐��iAudioListener�̏d���Ƃ��j
/// �@�������̎���Ƃ̓X�g���e�W�[�p�^�[���ŕʓ���o����悤��
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    private Dictionary<SceneEnum, string> _typeToName;

    private Stack<ISceneTransition> _transitions;

    ///// <summary>
    ///// �R���X�g���N�^
    ///// </summary>
    public ExSceneManager()
    {
        _transitions = new Stack<ISceneTransition>();
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager �R���X�g���N�^�I");

        Scene scene = SceneManager.GetActiveScene();
        Logger.Debug($"ExSceneManager �V�[���F {null != scene}, �V�[����:{SceneManager.sceneCount}");
    }

    /////// <summary>
    /////// �}�l�[�W���[�Ƃ��Ď��g���쐬
    /////// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    // ExSceneManager�Ƃ������O��GameObject���쐬���AExSceneManager�Ƃ����N���X��Add����
    //    new GameObject("ExSceneManager", typeof(ExSceneManager));
    //    //// GameManagers�V�[�����풓������݌v�̏ꍇ
    //    ////ManagerScene���L���łȂ��Ƃ��ɒǉ����[�h
    //    //if (!SceneManager.GetSceneByName(managerSceneName).IsValid())
    //    //{
    //    //    SceneManager.LoadScene(managerSceneName, LoadSceneMode.Additive);
    //    //}
    //}

    // Start is called before the first frame update
    void Start()
    {
        Logger.Debug("ExSceneManager Start�I");
    }

    void OnDestroy()
    {
        Logger.Debug("ExSceneManager OnDestroy�I");
    }

    //public void ChangeScene(string sceneName)
    //{
    //    // File��BuildSettings �ł̓o�^���K�v
    //    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    //}

    async UniTask UnloadSceneAsync(ISceneTransition transition)
    {
        await transition.UnLoadScenes();
    }
    public async UniTask PushOrRetry(ISceneTransition transition)
    {
        // TODO ����m�F�����O
        Logger.SetEnableLogging(true);
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitions.Count);
        Logger.Debug("Retry transition:" + transition.GetType());
        if (_transitions.Count > 0)
        {
            Logger.Debug("Retry Last______:" + _transitions.Last().GetType());
            Logger.Debug("Retry bool______:" + (_transitions.Last().GetType() == transition.GetType()));
        }

        if (_transitions.Count > 0 && _transitions.Last().GetType() == transition.GetType())
        {
            var scenes = await transition.LoadScenes();
            Scene scene = scenes.First();
            Logger.Debug("Retry ���[�h�� �g�b�v�V�[��:" + scene.name);
            SceneManager.SetActiveScene(scene);
            Reflesh();
        }
        else
        {
            await PushAsync(transition);
        }
    }

    public async UniTask PushAsync(ISceneTransition transition)
    {
        _transitions.Push(transition);
        var scenes = await transition.LoadScenes();
        Scene scene = scenes.First();
        Logger.Debug("PushAsync ���[�h��");
        SceneManager.SetActiveScene(scene);
        Reflesh();
    }

    internal async UniTask Replace(ISceneTransition transition)
    {
        var removeTransition = _transitions.Pop();
        // TODO ���V�[���擾���ĂȂ����Ǒ�܂��ȋZ�p�I�ɂ̓X�^�b�N�Œʗp����͂�
        //var removeScene = SceneManager.GetActiveScene();

        Logger.Debug("Replace �X�^�b�N����");
        // �V���ȃV�[�����A�N�e�B�u�ɂ���
        await PushAsync(transition);
        Logger.Debug("Replace �A�����[�h�O");
        // �A�N�e�B�u�ȃV�[���������ԂňȑO�̃V�[���A�����[�h
        await UnloadSceneAsync(removeTransition);
    }

    internal async UniTask ReplaceAll(ISceneTransition transition)
    {
        // TODO GameManagerScene�͏�݂Ȃ�X�^�b�N�Ƃ͕ʂɂ��Đ����Ȃ��Ă��H�E�E�E
        while (_transitions.Count > 1)
        {
            var unloadTransition = _transitions.Pop();
            await UnloadSceneAsync(unloadTransition);
        }

        await PushAsync(transition);
    }

    /// <summary>
    /// �A�N�e�B�u�V�[����Pop
    /// </summary>
    /// <returns>�V�[�����A�����[�h���ꂽ��</returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async UniTask<bool> Pop()
    {
        bool result = await PageTopDelete();

        // TODO�@�X�^�b�N�Ōオ�X�V���ꂽ�̂ōČĂяo���̏���
        // ���b�p�[�I�ȕ����ŉ������Ȃ�����
        // �V�[���P�̂Ȃ���ʂ�邱�ƂȂ�
        //await page.Resume();

        Reflesh();
        return result;
    }

    private async UniTask<bool> PageTopDelete()
    {
        // TODO GameManagerScene�͏�݂Ȃ�X�^�b�N�Ƃ͕ʂɂ��Đ����Ȃ��Ă��H�E�E�E
        if (_transitions.Count > 1)
        {
            return false;
        }

        // �X�^�b�N�Ō���폜
        var deleteTransition = _transitions.Pop();
        // TODO�@�V�[�����̂��Q�Ƃ����ق������m�ł͂���
        //Scene scene = SceneManager.GetActiveScene();
        await UnloadSceneAsync(deleteTransition);

        return true;
    }

    void Reflesh()
    {
        // TODO �X�V���r���[�ɔ��f
    }

    private SceneBase GetSceneBaseForLast()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneBase sceneBase = null;

        //GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        //�܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase>();
            if (sceneBase != null)
            {
                break;
            }
        }
        return sceneBase;
    }

    public SceneLayer GetLayer(ILayeredScene layered)
    {
        if (layered is ILayeredSceneLogic) return SceneLayer.Logic;
        if (layered is ILayeredSceneUI) return SceneLayer.UI;
        if (layered is ILayeredSceneField) return SceneLayer.Field;

        throw new NotImplementedException($"{layered.GetType()}�͕s���ȃV�[���K�w�ł�");
    }

    public void TODOChange()
    {
        _typeToName = new()
        {
            [SceneEnum.GameManagersScene] = "GameManagersScene",
            [SceneEnum.TitleScene] = "TitleScene",
            [SceneEnum.HomeScene] = "HomeScene",
            [SceneEnum.TutorialScene] = "TutorialScene",
            [SceneEnum.CreditNotationScene] = "CreditNotationScene",
        };
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            _typeToName[SceneEnum.TitleScene]
        );
    }
}
