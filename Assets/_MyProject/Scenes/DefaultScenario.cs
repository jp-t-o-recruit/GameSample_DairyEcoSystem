/// <summary>
/// �f�t�H���g��Ԃ̃V�i���I
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
        // ���ɉ������Ȃ�
    }
}
