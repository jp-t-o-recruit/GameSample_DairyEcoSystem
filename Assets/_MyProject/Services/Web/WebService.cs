/// <summary>
/// 通信処理
/// 本番用だがこのプロジェクトはサンプルなのでモックをそのまま継承
/// </summary>
public class WebService: WebMock
{
}

/// <summary>
/// WebServiceシングルトン管理
/// </summary>
public class WebServiceManager
{
    private static IWebServiceImplementation _instance;

    public static IWebServiceImplementation GetWebService()
    {
        if (null == _instance)
        {
            _instance = new WebMock();
        }
        return _instance;
    }
}