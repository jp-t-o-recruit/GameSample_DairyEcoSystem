using System.Collections.Generic;


public class CreditNotationSceneTransitioner : LayerdSceneTransitioner<CreditNotationSceneDomain.DomainParam>
{
    public CreditNotationSceneTransitioner(ILayeredSceneDomain domain = default): base(domain)
    {
        _domain = domain ?? new CreditNotationSceneDomain();
        SetupLayer();
    }

    private void SetupLayer()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(CreditNotationScene) },
        };
    }
}