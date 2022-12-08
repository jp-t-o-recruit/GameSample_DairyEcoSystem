using UnityEngine.UIElements;

public class CreditNotationScene : SceneBase
{
    Button _backButton;
    Label _viewLabel;

    // Start is called before the first frame update
    void Start()
    {
        _backButton = RootElement.Q<Button>("backButton");
        _viewLabel = RootElement.Q<Label>("viewLabel");

        _backButton.clickable.clicked += OnButtonClicked;
        _viewLabel.text = CreditNotationSceneParamsSO.Entity.ViewLabel.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _backButton.clickable.clicked -= OnButtonClicked;
    }

    private void OnButtonClicked()
    {
        // TODO元のシーンに変える
        // OR このシーンを削除
    }
}
