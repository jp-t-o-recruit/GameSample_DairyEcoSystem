using UnityEngine.UIElements;

/// <summary>
/// クレジット表記シーン
/// </summary>
public class CreditNotationScene : SceneBase, ILayeredSceneLogic
{
    public Button BackButton;
    public Label ViewLabel;

    // Start is called before the first frame update
    void Start()
    {
        BackButton = RootElement.Q<Button>("backButton");
        ViewLabel = RootElement.Q<Label>("viewLabel");
    }
}
