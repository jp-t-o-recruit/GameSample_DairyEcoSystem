/// <summary>
/// デフォルト状態のシナリオ
/// </summary>
public class DefaultScenario : IScenario
{
    public IScenarioParams Params { get; protected set; }

    void IScenario.OnActive()
    {
        // 特に何もしない
    }
}
