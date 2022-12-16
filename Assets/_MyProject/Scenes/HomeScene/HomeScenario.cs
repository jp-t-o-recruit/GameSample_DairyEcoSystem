using Cysharp.Threading.Tasks;
using System;


public class HomeScenarioState
{

}
public class HomeScenario : IScenario
{
    public IScenarioParams Params { get; set; }

    private Func<HomeScenario, UniTask> _completedAction;

    public HomeScenarioState State { get; set; }

    public HomeScenario(string id,
                        Func<HomeScenario, UniTask> callback,
                        HomeScenarioState state)
    {
        Params = new DummyScenarioParams()
        {
            ID = id
        };
        _completedAction = callback;
        State = state;
    }

    public async void OnActive()
    {
        // �ʏ�̃z�[���V�[�������s
        var domain = new HomeSceneDomain();
        var transitioner = new HomeSceneTransitioner(domain);
        await transitioner.Transition();
        Initialize(domain._uiLayer);
    }

    public void Initialize(HomeUIScene homeUIScene)
    {
        homeUIScene._toTitleSceneButton.clickable.clicked += OnClicked;
    }

    public void OnClicked()
    {
        var scenario = new TitleScenario();
        ScenarioContainer.SetActive(scenario, ChinType.NotChain);
    }

    /// <summary>
    /// �V�i���I�̊���
    /// ���V�i���I���Ăяo��
    /// </summary>
    private async UniTask Complete()
    {
        await _completedAction(this);
    }
}