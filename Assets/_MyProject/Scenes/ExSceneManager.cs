using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;

/// <summary>
/// �J�ڂł̃V�[���X�^�b�N�^�C�v
/// </summary>
public enum SceneStackType
{
    /// <summary>
    /// �w��V�[���̃v�b�V���܂��́A���݃V�[�����w��V�[���ŕs�����Ă��镔�������[�h����
    /// </summary>
    PushOrRetry,
    Push,
    Replace,
    ReplaceAll,
    /// <summary>
    /// "���݃V�[��"��Pop����A�������t�F�[���Z�[�t�Ƃ��đJ�ڐ�̎w��V�[���őJ�ڌĂяo��
    /// </summary>
    PopTry,
    //Pop,
}

/// <summary>
/// ����V�[���}�l�[�W���[
/// 
/// ���@�\
/// �E�V�[���\����S��
/// �@��GameManagerScene�i�풓�j
/// �@��HogeScene
/// �@��HogeUIScene
/// �@��HogeFieldScene
/// �ERun���̃R���|�[�l���g�ݒ�𒲐��iAudioListener�̏d���Ƃ��j
/// �@�������̎���Ƃ̓X�g���e�W�[�p�^�[���ŕʓ���o����悤��
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    private Stack<ISceneTransitioner> _transitioners;

    ///// <summary>
    ///// �R���X�g���N�^
    ///// </summary>
    public ExSceneManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        _transitioners = new Stack<ISceneTransitioner>();
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager �C�j�V�����C�Y�I");
    }


    async UniTask UnloadSceneAsync(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        transition.Discard(cts);
        await transition.UnLoadScenes(cts);
    }

    async UniTask PushOrRetry(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        // TODO ����m�F�����O
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitioners.Count);
        Logger.Debug("Retry class is:" + transitioner.GetSceneName());

        if (_transitioners.Count > 0 && _transitioners.Last().GetSceneName() == transitioner.GetSceneName())
        {
            var scenes = await transitioner.LoadScenes(cts);
            Scene scene = scenes.First();
            Logger.Debug("Retry ���[�h�� �g�b�v�V�[��:" + scene.name);
            SceneManager.SetActiveScene(scene);
        }
        else
        {
            await Push(transitioner, cts);
        }
    }

    /// <summary>
    /// �O����Push�Ƃ��ČĂяo�����Push����
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushGlobal(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        if (_transitioners.Count > 0)
        {
            var lastTrantion = _transitioners.Last();
            Logger.Debug("PushGlobal suspend���s :" + lastTrantion.GetSceneName());
            lastTrantion.Suspend(cts);
        }
        Logger.Debug("PushGlobal push�J�n" + transition.GetSceneName());
        await Push(transition, cts);
    }

    /// <summary>
    /// �����I��Push�Ƃ��ČĂяo��Push����
    /// </summary>
    /// <param name="transition"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    private async UniTask Push(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        _transitioners.Push(transition);
        Logger.Debug("Push transition�F" + transition.GetType());
        var scenes = await transition.LoadScenes(cts);
        Scene scene = scenes.First();
        Logger.Debug("Push �V�[�����[�h�����F" + scene.name);
        SceneManager.SetActiveScene(scene);
    }

    async UniTask Replace(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        var removeTransition = _transitioners.Count > 0 ? _transitioners.Pop() : default;

        Logger.Debug("Replace �X�^�b�N����");
        // �V���ȃV�[�����A�N�e�B�u�ɂ���
        await Push(transition, cts);
        Logger.Debug("Replace �A�����[�h�O");
        // �V�[����0���������Ȃ��悤�A�N�e�B�u�ȃV�[���������ԂňȑO�̃V�[���A�����[�h
        if (removeTransition != default)
        {
            await UnloadSceneAsync(removeTransition, cts);
        }
    }

    async UniTask ReplaceAll(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        while(_transitioners.Count > 0)
        {
            await UnloadSceneAsync(_transitioners.Pop(), cts);
        }

        await Push(transition, cts);
    }
    /// <summary>
    /// �A�N�e�B�u�V�[����Pop�A�o���Ȃ��ꍇ�t�F�[���Z�[�t�Ƃ��Ďw��V�[����Replace
    /// </summary>
    async UniTask TryPopDefaultReplace(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        if (_transitioners.Count > 1) {
            // Pop�\���܂�߂�V�[�������遨�X�^�b�N��2�ȏ�Ȃ�
            await Pop(cts);
        }
        else
        {
            Logger.Warning($"�V�[����Pop������肪�o���Ȃ������B�V�[��Stack���F{_transitioners.Count}, �ŏ�V�[�����F{_transitioners.Last()?.GetSceneName()}");
            await Replace(transition, cts);
        }
    }

    /// <summary>
    /// �A�N�e�B�u�V�[����Pop
    /// </summary>
    async UniTask Pop(CancellationTokenSource cts)
    {
        // �X�^�b�N�Ō���폜
        var unloadTransition = _transitioners.Pop();
        await UnloadSceneAsync(unloadTransition, cts);
        
        var lastTrantion = _transitioners.Last();
        SceneActivateFromTransition(lastTrantion);
        lastTrantion.Resume(cts);
    }

    public async UniTask Transition(ISceneTransitioner transition, CancellationTokenSource cts)
    {
        switch (transition.StackType)
        {
            case SceneStackType.PushOrRetry:
                await PushOrRetry(transition, cts);
                break;
            case SceneStackType.Push:
                await PushGlobal(transition, cts);
                break;
            case SceneStackType.Replace:
                await Replace(transition, cts);
                break;
            case SceneStackType.ReplaceAll:
                await ReplaceAll(transition, cts);
                break;
            case SceneStackType.PopTry:
                await TryPopDefaultReplace(transition, cts);
                break;
            //case SceneStackType.Pop:
            //    await Pop(cts);
            //    break;
            default:
                throw new ArgumentException($"�V�[���J�ڕ��@���蒆�ɗ�O���������܂����B{transition.StackType} is not {typeof(SceneStackType).Name}.");
        }        
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

    private bool SceneActivateFromTransition(ISceneTransitioner trantion)
    {
        string sceneName = trantion.GetSceneName();
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
            return true;
        }
        return false;
    }

    public SceneLayer GetLayer(ILayeredScene layered)
    {
        if (layered is ILayeredSceneLogic) return SceneLayer.Logic;
        if (layered is ILayeredSceneUI) return SceneLayer.UI;
        if (layered is ILayeredSceneField) return SceneLayer.Field;

        throw new NotImplementedException($"{layered.GetType()}�͕s���ȃV�[���K�w�ł�");
    }

    /// <summary>
    /// �w��V�[������R���|�[�l���g�̎擾
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <param name="scene"></param>
    /// <returns></returns>
    public static TComponent GetComponentFromScene<TComponent>(Scene scene) where TComponent : MonoBehaviour
    {
        TComponent component = default;

        // GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        // �܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var rootGameObject in scene.GetRootGameObjects())
        {
            if (rootGameObject.TryGetComponent<TComponent>(out component))
            {
                break;
            }
        }
        return component;
    }

    /// <summary>
    /// TODO ReplenishLayeredSceneOnPlayMode�̕��Ŏ������Ă���̂ŗp�ς�
    /// </summary>
    /// <param name="factory"></param>
    //internal async void NoticeDefaultTransition(Func<ISceneTransitioner> factory)
    //{
    //    Logger.Debug("NoticeDefaultTransition Count: " + _transitioners.Count);
    //    if (_transitioners == default || _transitioners.Count == 0)
    //    {
    //        Instance.Initialize();

    //        var transition = factory();
    //        Logger.Debug("NoticeDefaultTransition: ���������ăv�b�V������" + transition.GetSceneName());

    //        await Instance.ReplaceAll(transition);
    //    }
    //    else
    //    {
    //        // TODO _transitions���i�[���Ă��Ă��f�o�b�O�ŕێ��������Ă邾���ŏ����������̃f�o�b�O�ŌĂ΂�Ă��Ȃ����[�g������
    //        // GameManagersSceneAutoLoader�ŏ������ĂԂ��Ƃł��̃��[�g��f�H
    //        Logger.Debug($"NoticeDefaultTransition ���������Ȃ��B transitions���F{_transitioners.Count}  �F{_transitioners.Last().GetSceneName()}");
    //    }
    //}

    //internal async UniTask TrySetup()
    //{
    //    // GameManagerScene�𐶐�
    //    // GameManagerScene���q�G�����L�[�g�b�v��
    //    // �A�N�e�B�u�V�[���͕ύX����
    //    var sceneName = Enum.GetName(typeof(SceneEnum), SceneEnum.GameManagersScene);
    //    Logger.SetEnableLogging(true);
    //    Scene scene = SceneManager.GetSceneByName(sceneName);
    //    if (scene.isLoaded)
    //    {
    //        Logger.Debug("GameManagerScene�𐶐��@���Ȃ�: " + scene.buildIndex);
    //        return;
    //    }

    //    // GameManagersScene�͏풓������
    //    await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    //    scene = SceneManager.GetSceneByName(sceneName);
    //    Logger.Debug("GameManagerScene�𐶐��@�����H: " + scene.buildIndex);
    //}

}


#if DEVELOPMENT_BUILD || UNITY_EDITOR
/// <summary>
/// Awake�O��ManagerScene�������Ń��[�h����N���X
/// 
/// �R�s�y��
/// �S�ẴV�[���ɑ��݂��A���A��������݂��Ă͂����Ȃ��}�l�[�W���[�I���݂̎������@�yUnity�z - (:3[kan�̃�����]
/// https://kan-kikuchi.hatenablog.com/entry/ManagerSceneAutoLoader
/// </summary>
public class GameManagersSceneAutoLoader
{
    //�Q�[���J�n��(�V�[���ǂݍ��ݑO)�Ɏ��s�����
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        //ReplenishLayeredSceneOnPlayMode();
        LoadGameManagersScene();

        ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ExSceneManager.Instance.Initialize();
        // TODO �}�l�[�W���[�풓�̈Ӌ`������Ȃ�g������
        // ���̎��̓V���O���g����������߂�
        // CreateManagerInstance();
    }

    /// <summary>
    /// playmode�N���ŃV�[���K�w��Hierarchy�ɑ���ĂȂ��Ȃ��[����
    /// </summary>
    //private async static void ReplenishLayeredSceneOnPlayMode(CancellationTokenSource cts)
    //{
    //    Scene scene = SceneManager.GetActiveScene();
    //    if (scene == default)
    //    {
    //        return;
    //    }

    //    if (EnumExt.TryParseToEnum(scene.name, out SceneEnum sceneEnum))
    //    {
    //        await ExSceneManager.Instance.PushOrRetry(sceneEnum,cts);
    //    }
    //    else
    //    {
    //        throw new FormatException($"�V�[�����ϊ����ɗ�O���������܂����B{scene.name} is not SceneEnum.");
    //    }
    //}

    /// <summary>
    /// �풓������GameManagersScene���N������
    /// </summary>
    private static void LoadGameManagersScene()
    {
        string sceneName = Enum.GetName(typeof(SceneEnum), SceneEnum.GameManagersScene);

        // ManagerScene���L���łȂ���(�܂��ǂݍ���ł��Ȃ���)�����ǉ����[�h����悤��
        if (!SceneManager.GetSceneByName(sceneName).IsValid())
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    /// <summary>
    /// �펞�V�[���ɃQ�[���I�u�W�F�N�g�Ƃ��Đ�������O��ŏ풓�T�[�r�X�Ƃ���
    /// </summary>
    private static void CreateManagerInstance()
    {
        //Manager�Ƃ������O��GameObject���쐬���AManager�Ƃ����N���X��Add����
        new GameObject("ExSceneManager", typeof(ExSceneManager));
    }
}
#endif