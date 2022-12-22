/// <summary>
/// デフォルト状態のシナリオ
/// </summary>
public class DefaultScenario : IScenario
{
    public IScenarioParams Params { get; protected set; }

    public DefaultScenario(string id)
    {
        Params = new DummyScenarioParams()
        {
            ID = id
        };
    }

    void IScenario.OnActive()
    {
        // 特に何もしない
    }
}
