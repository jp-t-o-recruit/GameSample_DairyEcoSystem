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
    private Stack<ILayeredSceneDomain> _domains;

    ///// <summary>
    ///// �R���X�g���N�^
    ///// </summary>
    public ExSceneManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_domains != default)
        {
            ClearAll(new CancellationTokenSource()).Forget();
        }
        _domains = new Stack<ILayeredSceneDomain>();
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager �C�j�V�����C�Y�I");
    }


    async UniTask UnloadSceneAsync(ILayeredSceneDomain domain, CancellationTokenSource cts)
    {
        domain.Discard(cts);
        ISceneTransitioner transitioner = domain.CreateTransitioner();
        await transitioner.UnLoadScenes(cts);
        transitioner.Dispose();
    }

    /// <summary>
    /// �K�w�V�[���̕s����₢���S���𐮂���V�[�����[�h
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushOrRetry(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 0 && _domains.Last().GetSceneName() == transitioner.GetSceneName())
        {
            var scenes = await transitioner.LoadScenes(cts);
            Scene scene = scenes.First();
            Logger.Debug("�V�[���J�ځ@Retry ���[�h�� �g�b�v�V�[��:" + scene.name);
            SceneManager.SetActiveScene(scene);
        }
        else
        {
            Logger.Debug("�V�[���J�ځ@Retry �Ł@Push�@�Ăяo��:" + transitioner.GetSceneName());
            await Push(transitioner, cts);
        }
    }

    /// <summary>
    /// �N���X�O����Push�Ƃ��ČĂяo�����Push����
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask PushGlobal(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 0)
        {
            var lastDomain = _domains.Last();
            Logger.Debug("PushGlobal suspend���s :" + lastDomain.GetSceneName());
            lastDomain.Suspend(cts);
        }
        Logger.Debug("PushGlobal push�J�n" + transitioner.GetSceneName());
        await Push(transitioner, cts);
    }

    /// <summary>
    /// �����I��Push�Ƃ��ČĂяo��Push����
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    private async UniTask Push(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        _domains.Push(transitioner.Domain);
        Logger.Debug("Push transition�F" + transitioner.Domain.GetSceneName());
        var scenes = await transitioner.LoadScenes(cts);
        Scene scene = scenes.First();
        Logger.Debug("Push �V�[�����[�h�����F" + scene.name);
        SceneManager.SetActiveScene(scene);
    }

    /// <summary>
    /// �w��V�[�����X�^�b�N�s�[�N�Ƃ��Č�������V�[���ǂݍ���
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask Replace(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        var removeDomain = _domains.Count > 0 ? _domains.Pop(): default;

        Logger.Debug("Replace �X�^�b�N����");
        // �V���ȃV�[�����A�N�e�B�u�ɂ���
        await Push(transitioner, cts);
        Logger.Debug("Replace �A�����[�h�O");
        // �V�[����0���������Ȃ��悤�A�N�e�B�u�ȃV�[���������ԂňȑO�̃V�[���A�����[�h
        if (removeDomain != default)
        {
            await UnloadSceneAsync(removeDomain, cts);
        }
    }
    /// <summary>
    /// �w��V�[���݂̂̃X�^�b�N�ɍX�V����V�[���ǂݍ���
    /// </summary>
    /// <param name="transitioner"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    async UniTask ReplaceAll(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        await ClearAll(cts);
        await Push(transitioner, cts);
    }

    private async UniTask ClearAll(CancellationTokenSource cts)
    {
        while (_domains.Count > 0)
        {
            await UnloadSceneAsync(_domains.Pop(), cts);
        }
    }


    /// <summary>
    /// �A�N�e�B�u�V�[����Pop�A�o���Ȃ��ꍇ�t�F�[���Z�[�t�Ƃ��Ďw��V�[����Replace
    /// </summary>
    async UniTask TryPopDefaultReplace(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        if (_domains.Count > 1) {
            // Pop�\���߂�V�[�������遨�X�^�b�N��2�ȏ�Ȃ�
            await Pop(cts);
        }
        else
        {
            Logger.Warning($"�V�[����Pop������肪�o���Ȃ������B�V�[��Stack���F{_domains.Count}, �ŏ�V�[�����F{_domains.Last()?.GetSceneName()}");
            await Replace(transitioner, cts);
        }
    }

    /// <summary>
    /// �A�N�e�B�u�V�[����Pop
    /// </summary>
    async UniTask Pop(CancellationTokenSource cts)
    {
        // �X�^�b�N�Ō���폜
        var unloadDomain = _domains.Pop();
        await UnloadSceneAsync(unloadDomain, cts);
        
        var lastDomain = _domains.Last();
        ActivateSceneFromDomain(lastDomain);
        lastDomain.Resume(cts);
    }

    public async UniTask Transition(ISceneTransitioner transitioner, CancellationTokenSource cts)
    {
        switch (transitioner.StackType)
        {
            case SceneStackType.PushOrRetry:
                await PushOrRetry(transitioner, cts);
                break;
            case SceneStackType.Push:
                await PushGlobal(transitioner, cts);
                break;
            case SceneStackType.Replace:
                await Replace(transitioner, cts);
                break;
            case SceneStackType.ReplaceAll:
                await ReplaceAll(transitioner, cts);
                break;
            case SceneStackType.PopTry:
                await TryPopDefaultReplace(transitioner, cts);
                break;
            default:
                throw new ArgumentException($"�V�[���J�ڕ��@���蒆�ɗ�O���������܂����B{transitioner.StackType} is not {typeof(SceneStackType).Name}.");
        }

        transitioner.Dispose();
    }

    private bool ActivateSceneFromDomain(ILayeredSceneDomain domain)
    {
        string sceneName = domain.GetSceneName();
        var scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
            return true;
        }
        return false;
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

        if ( component == null) Logger.Warning($"�V�[���ǂݍ��݁@GetSceneBase�@�ŃV�[���R���|�[�l���g�F{typeof(TComponent).Name} �̎擾���ł��Ȃ������B");

        return component;
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