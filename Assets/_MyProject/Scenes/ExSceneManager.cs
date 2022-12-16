using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;


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

        //Scene scene = SceneManager.GetActiveScene();
        //Logger.Debug($"ExSceneManager �V�[���F {null != scene}, �V�[����:{SceneManager.sceneCount}");
    }


    async UniTask UnloadSceneAsync(ISceneTransitioner transition)
    {
        await transition.Discard();
        await transition.UnLoadScenes();
    }

    public async UniTask PushOrRetry(SceneEnum sceneEnum)
    {
        string sceneName = Enum.GetName(typeof(SceneEnum), sceneEnum);
        // TODO ����m�F�����O
        Logger.Debug("Retry:--------------------------------------");
        Logger.Debug("Retry Count:" + _transitioners.Count);
        Logger.Debug("Retry class is:" + sceneName);
        if (_transitioners.Count > 0)
        {
            Logger.Debug("Retry Last______:" + _transitioners.Last().GetType());
            Logger.Debug("Retry bool______:" + (_transitioners.Last().GetType().Name == sceneName));
        }

        if (_transitioners.Count > 0 && _transitioners.Last().GetType().Name == sceneName)
        {
            ISceneTransitioner transition = SceneRelationService.GetSceneTransitionerByEnum(sceneEnum);
            var scenes = await transition.LoadScenes();
            Scene scene = scenes.First();
            Logger.Debug("Retry ���[�h�� �g�b�v�V�[��:" + scene.name);
            SceneManager.SetActiveScene(scene);

            await transition.Initialize();
        }
        else
        {
            ISceneTransitioner transition = SceneRelationService.GetSceneTransitionerByEnum(sceneEnum);
            await Push(transition);
        }
    }

    public async UniTask PushAsync(ISceneTransitioner transition)
    {
        Logger.Debug("PushAsync ���X�g�́H" + _transitioners.Count);
        var lastTrantion = _transitioners.Last();
        await lastTrantion.Suspend();

        await Push(transition);
    }

    private async UniTask Push(ISceneTransitioner transition)
    {
        _transitioners.Push(transition);
        Logger.SetEnableLogging(true);
        Logger.Debug("Push transition�F" + transition.GetType());
        var scenes = await transition.LoadScenes();
        Scene scene = scenes.First();
        Logger.Debug("Push �V�[�����[�h�����F" + scene.name);
        SceneManager.SetActiveScene(scene);

        await transition.Initialize();
    }

    internal async UniTask Replace(ISceneTransitioner transition)
    {
        var removeTransition = _transitioners.Pop();

        Logger.Debug("Replace �X�^�b�N����");
        // �V���ȃV�[�����A�N�e�B�u�ɂ���
        await Push(transition);
        Logger.Debug("Replace �A�����[�h�O");
        // �A�N�e�B�u�ȃV�[���������ԂňȑO�̃V�[���A�����[�h
        await UnloadSceneAsync(removeTransition);
    }

    internal async UniTask ReplaceAll(ISceneTransitioner transition)
    {
        while(_transitioners.Count > 0)
        {
            var unloadTransition = _transitioners.Pop();
            await UnloadSceneAsync(unloadTransition);
        }

        await Push(transition);
    }

    /// <summary>
    /// �A�N�e�B�u�V�[����Pop
    /// </summary>
    internal async UniTask Pop()
    {
        // �X�^�b�N�Ō���폜
        var unloadTransition = _transitioners.Pop();
        await UnloadSceneAsync(unloadTransition);
        
        var lastTrantion = _transitioners.Last();
        SceneActivateFromTransition(lastTrantion);
        await lastTrantion.Resume();
    }

    public async UniTask Transition(ISceneTransitioner transition)
    {
        switch (transition.NextRelation)
        {
            case SceneRelation.Free:
                if (new List<SceneRelation>(){ SceneRelation.StartLink }.Contains(transition.PrevRelation) )
                {
                    await ReplaceAll(transition);
                } else
                {
                    await Replace(transition);
                }
                break;
            case SceneRelation.None:
                await Pop();
                break;
            case SceneRelation.ChainLink:
            case SceneRelation.StartLink:
            case SceneRelation.HookLink:
                await PushAsync(transition);
                break;
            default:
                throw new ArgumentException($"�V�[���J�ڕ��@���蒆�ɗ�O���������܂����B{transition.NextRelation} is not {typeof(SceneRelation).Name}.");
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
        ReplenishLayeredSceneOnPlayMode();
        LoadGameManagersScene();

        //ScenarioContainer.SetActive(new PlayModeScenario(), ChinType.NotChain);

        //ExSceneManager.Instance.Initialize();
        // TODO �}�l�[�W���[�풓�̈Ӌ`������Ȃ�g������
        // ���̎��̓V���O���g����������߂�
        // CreateManagerInstance();
    }
    private static void CreateScenario(SceneEnum sceneEnum)
    {
        // TODO
        //var hoge = new HomeScenario();
        //var sev = new TitleScenario();
    }

    /// <summary>
    /// playmode�N���ŃV�[���K�w��Hierarchy�ɑ���ĂȂ��Ȃ��[����
    /// </summary>
    private async static void ReplenishLayeredSceneOnPlayMode()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene == default)
        {
            return;
        }

        if (EnumExt.TryParseToEnum(scene.name, out SceneEnum sceneEnum))
        {
            await ExSceneManager.Instance.PushOrRetry(sceneEnum);
            CreateScenario(sceneEnum);
        }
        else
        {
            throw new FormatException($"�V�[�����ϊ����ɗ�O���������܂����B{scene.name} is not SceneEnum.");
        }
    }

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