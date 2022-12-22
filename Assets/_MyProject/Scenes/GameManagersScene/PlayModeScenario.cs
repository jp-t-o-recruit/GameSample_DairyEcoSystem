#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine.SceneManagement;
using System;
using Logger = MyLogger.MapBy<PlayModeScenario>;
using System.Linq;
using Cysharp.Threading.Tasks;

/// <summary>
/// Unity��PlayMode�ŌĂяo���V�i���I
/// </summary>
public class PlayModeScenario : IScenario
{
    public IScenarioParams Params { get; set; }

    public PlayModeScenario()
    {
        Params = new DummyScenarioParams()
        {
            ID = $"{typeof(PlayModeScenario)}"
        };

    }
    /// <summary>
    /// �v���C���[�h�Ȃ̂ŃA�N�e�B�u�V�[���ɑ����ē���J�n
    /// �w��s���ȃf�[�^�͊e�V�[�������O�ŕ₤
    /// </summary>
    public void OnActive()
    {
        // play���[�h�ŃC���X�^���X�������Ă邩������Ȃ��̂ŏ�����
        ExSceneManager.Instance.Initialize();
        //SetupScenarioBySaveData();

        Scene scene = SceneManager.GetActiveScene();
        IScenario scenario = null;

        if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.TitleScene, () => new TitleScenario())) { }
        else if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.HomeScene, () => new HomeScenario("TODOgame", null, new HomeScenarioState()))) { }
        else if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.BattleScene, () => new DefaultScenario("TODO999"))) { }

        Logger.SetEnableLogging(true);
        Logger.Debug($"PlayMode��{scenario.GetType()}�V�i���I���J�n");

        ScenarioContainer.SetActive(scenario, ChinType.NotChain);
    }

    private bool TryParceSceneToScenario(out IScenario scenario, string sceneName, SceneEnum sceneEnum, Func<IScenario> callback)
    {
        scenario = default;
        if (sceneName == Enum.GetName(typeof(SceneEnum), sceneEnum))
        {
            scenario = callback();
        }
        return scenario != default;
    }

    /// <summary>
    /// �Z�[�u�f�[�^����V�i���I�쐬&�V�[���J�ڂ܂ł��
    /// </summary>
    /// <returns></returns>
    private bool SetupScenarioBySaveData()
    {
        var accountService = UserAccountDomainManager.GetService();
        var first = accountService.User.Build.ScenarioProgression.AsEnumerable().First();
        if (first != default)
        {
            // TODO �܂��Z�[�u����̓ǂݍ��݂�����Ă��Ȃ�
            // debug�J�n�p�̃Z�[�u�f�[�^������Ȃ炻��ŊJ�n
            // TODO�@�V�i���IID����V�i���I�쐬����T�[�r�X�����
            // TODO�@�V�i���I�쐬������V�[���J�ڂ��鏈�������
            new HomeSceneDomain().SceneTransition().Forget();
        }
        return first != default;
    }
}

#endif