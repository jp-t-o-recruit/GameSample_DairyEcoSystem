using System;

/// <summary>
/// シングルトン実装基板クラス
/// 
/// class Hoge : SingletonBase<Hoge> を定義し
/// Hoge.Instanceを実体に持つ
/// </summary>
/// <typeparam name="SelfType"></typeparam>
public abstract class SingletonBase<SelfType> where SelfType : SingletonBase<SelfType>, new()
{
    /// <summary>
    /// コンストラクタ制限
    /// </summary>
    protected SingletonBase() { }

    /// <summary>
    /// シングルトン実装
    /// </summary>
    public static SelfType Instance
    {
        get
        {
            if (null == _instance)
            {
                
                _instance = new Lazy<SelfType>();
            }
            return _instance.Value;
        }
    }

    /// <summary>
    /// シングルトン実体
    /// マルチスレッド対応のためLazy
    /// </summary>
    private static Lazy<SelfType> _instance;

}