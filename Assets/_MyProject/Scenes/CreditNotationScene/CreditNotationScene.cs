using UnityEngine.UIElements;

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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        BackButton.clickable.clicked -= OnButtonClicked;
    }

    private void OnButtonClicked()
    {
        // TODO元のシーンに変える
        // OR このシーンを削除
    }
}
