using Cysharp.Threading.Tasks;
using System;

public class HomeScenarioFactory
{
    internal static IScenario Build(string detail)
    {

        Func<HomeScenario, UniTask> callback = default;
        //    async () =>
        //{
        //    await UniTask.Delay(0);
        //    return UniTask.Delay(10); 
        //};
        HomeScenarioState state = new HomeScenarioState();
        return new HomeScenario("id-home", callback , state);
    }
}
