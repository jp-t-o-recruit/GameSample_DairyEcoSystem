/// <summary>
/// �ʐM����
/// �{�ԗp�������̃v���W�F�N�g�̓T���v���Ȃ̂Ń��b�N�����̂܂܌p��
/// </summary>
public class WebService: WebMock
{
}

/// <summary>
/// WebService�V���O���g���Ǘ�
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