#if DEVELOPMENT_BUILD || UNITY_EDITOR

using UnityEngine.SceneManagement;
using System;

public class PlayModeScenario : IScenario
{
    public IScenarioParams Params { get; set; }

    public void OnActive() {

        Scene scene = SceneManager.GetActiveScene();
        IScenario scenario = null;

        if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.TitleScene, () => new TitleScenario())) { }
        else if (TryParceSceneToScenario(out scenario, scene.name, SceneEnum.HomeScene, () => new HomeScenario("TODOgame", null, new HomeScenarioState()))) { }

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
}

#endif