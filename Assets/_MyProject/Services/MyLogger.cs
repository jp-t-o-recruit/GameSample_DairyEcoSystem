using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ログレベルの列挙比較演算用
/// </summary>
public static class LogLevelMath
{
    /// <summary>
    /// 第一引数が第二引数より大きい
    /// </summary>
    /// <param name="level"></param>
    /// <param name="bordar"></param>
    /// <returns></returns>
    public static bool IsOver(LogType level, LogType bordar)
    {
        return (short)level > (short)bordar;
    }

    /// <summary>
    /// 第一引数が第二引数以下である
    /// </summary>
    /// <param name="level"></param>
    /// <param name="bordar"></param>
    /// <returns></returns>
    public static bool IsBelow(LogType level, LogType bordar)
    {
        return (short)level <= (short)bordar;
    }
}

/// <summary>
/// ロガー
/// </summary>
public class MyLogger : SingletonBase<MyLogger>
{
    /// <summary>
    /// 全体のログ出力レベル
    /// </summary>
    public LogType LoggingLevel = LogType.Log;

    /// <summary>
    /// ロガー実態
    /// </summary>
    public ILogger Logger
    {
        get => _logger;
        set
        {
            if (null != value)
            {
                _logger = value;
            }
        }
    }
    /// <summary>
    /// ロガー実態
    /// </summary>
    public ILogger _logger;

    /// <summary>
    /// ログ出力有効の型をキーにしたマッピング
    /// </summary>
    protected Dictionary<Type, bool> LoggingDic
    {
        get
        {
            if (null == _LoggingDic)
            {
                _LoggingDic = new Dictionary<Type, bool>();
                // TODO シングルトン実装確認用のログ出力
                Debug.Log("MyLogger2 生成 _isLogging" + typeof(Type).Name);
            }
            return _LoggingDic;
        }
    }
    private Dictionary<Type, bool> _LoggingDic;

    /// <summary>
    /// 型によるログ出力有効フラグ割振り用内部クラス
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class MapBy<TType>
    {
        /// <summary>
        /// ログ出力処理
        /// </summary>
        /// <param name="str">出力したい文字列</param>
        public static void Debug(string str)
        {
            if (LogLevelMath.IsBelow(Instance.LoggingLevel, LogType.Log) &&
                GetEnableLogging())
            {
                Instance.Logger.Log(str);
            }
        }

        /// <summary>
        /// ログ出力有効フラグを取得
        /// </summary>
        /// <returns></returns>
        public static bool GetEnableLogging()
        {
            bool isLogging = Instance.LoggingDic.GetValueOrDefault(typeof(TType), false);
            return isLogging;
        }

        /// <summary>
        /// ログ出力有効フラグ設定
        /// </summary>
        /// <param name="enable"></param>
        public static void SetEnableLogging(bool enable)
        {
            Instance.LoggingDic.TryAdd(typeof(TType), enable);
        }

        /// <summary>
        /// ログ出力有効フラグをキー毎削除する（シーンの切り替えなどでその振り分けが必要なくなるときにメモリ開放的に実施する）
        /// </summary>
        public static void UnloadEnableLogging()
        {
            Instance.LoggingDic.Remove(typeof(TType));
        }
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MyLogger()
    {
        // TODO シングルトン実装確認用のログ出力
        Debug.Log("コンストラクタ MyLogger");

        Logger = Debug.unityLogger;
    }
}