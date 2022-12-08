using Cysharp.Threading.Tasks;
using System;
using System.Threading;


/// <summary>
/// �ʐM�����C���^�[�t�F�[�X
/// �ꉞRESTAPI�Ő݌v
/// </summary>
public interface IWebServiceImplementation
{
    public UniTask<UserInfo> GetUserInfo(string str, CancellationToken cancellationToken);

    public UniTask<UserInfo> PutLogin(string str, CancellationToken cancellationToken);
}

/// <summary>
/// �ʐM�������b�N
/// 
/// try catch �̎Q�l
/// https://neue.cc/2022/07/13_Cancellation.html
/// </summary>
public class WebMock: IWebServiceImplementation
{
    private static TimeSpan _timeout = TimeSpan.FromSeconds(30);

    private async UniTask MockDelay(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        /// �ʐM�V�~�����[�V�����f�B���C�^�C��
        TimeSpan delayTime = TimeSpan.FromSeconds(UnityEngine.Random.Range(1, 5));

        try
        {
            await UniTask.Delay(delayTime, cancellationToken: cts.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // ������CancellationToken�������Ȃ̂ŁA�����ێ�����OperationCanceledException�Ƃ��ē�����
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else
            {
                // �^�C���A�E�g�������Ȃ̂ŁATimeoutException(�����͓Ǝ��̗�O)�Ƃ��ē�����
                throw new TimeoutException($"The request was canceled due to the configured Timeout of { _timeout.TotalSeconds } seconds elapsing.", ex);
            }
        }
    }

    public async UniTask<UserInfo> GetUserInfo(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return new UserInfo() { userId = "123", userName = "�}�C�P�����" };
    }
    public async UniTask<UserInfo> PutLogin(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return new UserInfo() { userId = "123", userName = "�}�C�P�����" };
    }

    public async UniTask<bool> PostUserResource(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return true;
    }
}
