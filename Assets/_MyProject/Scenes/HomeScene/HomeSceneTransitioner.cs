using System.Collections.Generic;

public class HomeSceneTransitioner : LayerdSceneTransitioner<HomeSceneDomain.DomainParam>
{
    public HomeSceneTransitioner(ILayeredSceneDomain domain = default) : base(domain)
    {
        _domain = domain ?? new HomeSceneDomain();
        SetupLayer();
    }

    private void SetupLayer()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(HomeScene) },
            { SceneLayer.UI, typeof(HomeUIScene) },
            { SceneLayer.Field, typeof(HomeFieldScene) },
        };
    }
}

