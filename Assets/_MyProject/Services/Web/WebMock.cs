using Cysharp.Threading.Tasks;
using System;
using System.Threading;


/// <summary>
/// 通信処理インターフェース
/// 一応RESTAPIで設計
/// </summary>
public interface IWebServiceImplementation
{
    public UniTask<UserInfo> GetUserInfo(string str, CancellationToken cancellationToken);

    public UniTask<UserInfo> PutLogin(string str, CancellationToken cancellationToken);
}

/// <summary>
/// 通信処理モック
/// 
/// try catch の参考
/// https://neue.cc/2022/07/13_Cancellation.html
/// </summary>
public class WebMock: IWebServiceImplementation
{
    private static TimeSpan _timeout = TimeSpan.FromSeconds(30);

    private async UniTask MockDelay(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        /// 通信シミュレーションディレイタイム
        TimeSpan delayTime = TimeSpan.FromSeconds(UnityEngine.Random.Range(1, 5));

        try
        {
            await UniTask.Delay(delayTime, cancellationToken: cts.Token);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // 引数のCancellationTokenが原因なので、それを保持したOperationCanceledExceptionとして投げる
                throw new OperationCanceledException(ex.Message, ex, cancellationToken);
            }
            else
            {
                // タイムアウトが原因なので、TimeoutException(或いは独自の例外)として投げる
                throw new TimeoutException($"The request was canceled due to the configured Timeout of { _timeout.TotalSeconds } seconds elapsing.", ex);
            }
        }
    }

    public async UniTask<UserInfo> GetUserInfo(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return new UserInfo() { userId = "123", userName = "マイケル鈴木" };
    }
    public async UniTask<UserInfo> PutLogin(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return new UserInfo() { userId = "123", userName = "マイケル鈴木" };
    }

    public async UniTask<bool> PostUserResource(string str, CancellationToken cancellationToken)
    {
        await MockDelay(cancellationToken);
        return true;
    }
}
