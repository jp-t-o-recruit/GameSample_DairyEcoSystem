using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Logger = MyLogger.MapBy<ExSceneManager>;


public interface ISceneTransition
{
    public abstract UniTask<Scene> LoadScene();
}
public abstract class SceneTransition<TParam>: ISceneTransition where TParam : new()
{
    public string SceneName { get; protected set; }
    public TParam Parameter { get; set; }
    public async UniTask<Scene> LoadScene()
    {
        await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        Scene scene = SceneManager.GetSceneByName(SceneName);
        AttachParameter(scene);
        return scene;
    }

    /// <summary>
    /// �V�[���N���X�i�X�N���v�g�j�Ƀp�����[�^���A�^�b�`����
    /// </summary>
    /// <param name="scene"></param>
    /// <returns></returns>
    private SceneBase<TParam> AttachParameter(Scene scene)
    {
        SceneBase<TParam> sceneBase = null;

        // GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        // �܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase<TParam>>();
            if (sceneBase != null)
            {
                sceneBase.CreateParam = Parameter ?? new TParam();
                break;
            }
        }
        return sceneBase;
    }
}


/// <summary>
/// ����V�[���}�l�[�W���[
/// </summary>
public class ExSceneManager : SingletonBase<ExSceneManager>
{
    ///// <summary>
    ///// �R���X�g���N�^
    ///// </summary>
    public ExSceneManager()
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("ExSceneManager �R���X�g���N�^�I");

        Scene scene = SceneManager.GetActiveScene();
        Logger.Debug($"ExSceneManager �V�[���F {null != scene}, �V�[����:{SceneManager.sceneCount }");
    }

    ///// <summary>
    ///// �}�l�[�W���[�Ƃ��Ď��g���쐬
    ///// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    // ExSceneManager�Ƃ������O��GameObject���쐬���AExSceneManager�Ƃ����N���X��Add����
    //    new GameObject("ExSceneManager", typeof(ExSceneManager));
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

    async UniTask UnloadSceneAsync(string sceneName)
    {
        await SceneManager.UnloadSceneAsync(sceneName);
        //UniTask.Delay(5000);
        await Resources.UnloadUnusedAssets().ToUniTask();
    }

    public async UniTask PushAsync(ISceneTransition transition)
    {
        Scene scene = await transition.LoadScene();
        Logger.Debug("PushAsync ���[�h��");
        SceneManager.SetActiveScene(scene);
        Reflesh();
    }

    internal async UniTask Replace(ISceneTransition transition)
    {
        var removeScene = SceneManager.GetActiveScene();
        Logger.Debug("Replace �X�^�b�N����");
        // �V���ȃV�[�����A�N�e�B�u�ɂ���
        await PushAsync(transition);
        Logger.Debug("Replace �A�����[�h�O");
        // �A�N�e�B�u�ȃV�[���������ԂňȑO�̃V�[���A�����[�h
        await UnloadSceneAsync(removeScene.name);
    }

    internal async UniTask ReplaceAll(ISceneTransition transition)
    {
        // ���݂̃V�[���������ׂĎ擾
        // �X�^�b�N�I�N���A�Ȃ̂ŋt���Ŏ擾
        List<string> unloadScenes = new();
        foreach(var index in Enumerable.Range(0, SceneManager.sceneCount).Reverse())
        {
            unloadScenes.Add(SceneManager.GetSceneAt(index).name);
        }

        // ��ɒǉ����Ă���
        await PushAsync(transition);

        // �ȑO�̃V�[�����A�����[�h
        foreach (var name in unloadScenes){
            await UnloadSceneAsync(name);
        };
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
        //if (EditorSceneManager.loadedSceneCount > 1)
        // TODO�@Unity�o�[�W����2021.3�܂ł̓��[�h�ς݂̐�
        // 2022.2����̓��[�h����A�����[�h�����܂�
        if (SceneManager.sceneCount > 1)
        {
            return false;
        }

        // �X�^�b�N�Ō���폜
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        await UnloadSceneAsync(sceneName);

        return true;
    }

    void Reflesh()
    {
        // TODO �X�V���r���[�ɔ��f
    }

    private SceneBase<TParam> GetSceneBaseForLast<TParam>()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneBase<TParam> sceneBase = null;

        //GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
        //�܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            sceneBase = gameObject.GetComponent<SceneBase<TParam>>();
            if (sceneBase != null)
            {
                break;
            }
        }
        return sceneBase;
    }
}
