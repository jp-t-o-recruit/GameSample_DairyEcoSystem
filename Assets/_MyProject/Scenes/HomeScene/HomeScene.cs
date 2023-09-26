using UnityEngine;


using LocalLogger = MyLogger.MapBy<HomeScene>;

public class HomeScene : MonoBehaviour, ILayeredSceneLogic
{
    public 

    // Start is called before the first frame update
    void Start()
    {
        //ExSceneManager.Instance.NoticeDefaultTransition(() => new HomeSceneTransitioner());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        LocalLogger.UnloadEnableLogging();
    }
}