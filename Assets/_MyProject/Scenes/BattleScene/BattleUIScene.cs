using UnityEngine.UIElements;

public class BattleUIScene : SceneBase, ILayeredSceneUI
{
    public Button questButton;
    public Button searchButton;
    public Button dungeonButton;
    public Button InvestigationButton;
    public Button clanBattleButton;
    public Button arenaButton;
    public Button pArenaButton;

    // Start is called before the first frame update
    void Start()
    {
        questButton = RootElement.Q<Button>("questButton");
        searchButton = RootElement.Q<Button>("searchButton");
        dungeonButton = RootElement.Q<Button>("dungeonButton");
        InvestigationButton = RootElement.Q<Button>("InvestigationButton");
        clanBattleButton = RootElement.Q<Button>("clanBattleButton");
        arenaButton = RootElement.Q<Button>("arenaButton");
        pArenaButton = RootElement.Q<Button>("nextSceneButton");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
