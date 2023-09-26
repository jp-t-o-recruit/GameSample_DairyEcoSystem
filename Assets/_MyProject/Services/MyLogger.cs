using log4net.Repository.Hierarchy;
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
    private ILogger _logger;

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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                // シングルトン実装確認用のログ出力
                Debug.Log("MyLogger 生成 _isLogging" + typeof(Type).Name);
#endif
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
        private static void Log(LogType limitLogType, bool enableLogging, string str)
        {
            if (LogLevelMath.IsBelow(Instance.LoggingLevel, limitLogType) &&
                enableLogging)
            {
                Instance.Logger.Log(Instance.LoggingLevel, str);
            }
        }

        /// <summary>
        /// ログ出力処理 Debug
        /// </summary>
        /// <param name="str">出力したい文字列</param>
        public static void Debug(string str)
        {
            Log(LogType.Log, GetEnableLogging(), str);
        }
        /// <summary>
        /// ログ出力処理 Info <br/>
        /// Warningとして絶対出したいわけではないWarning
        /// </summary>
        /// <param name="str">出力したい文字列</param>
        public static void Info(string str)
        {
            Log(LogType.Warning, GetEnableLogging(), str);
        }
        /// <summary>
        /// ログ出力処理 Warning
        /// </summary>
        /// <param name="str">出力したい文字列</param>
        public static void Warning(string str)
        {
            Log(LogType.Warning, true, str);
        }
        /// <summary>
        /// ログ出力処理 Assert
        /// </summary>
        /// <param name="str">出力したい文字列</param>
        public static void Assert(string str)
        {
            Log(LogType.Assert, true, str);
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
        /// <param name="enable">true:ログ出力有効</param>
        public static void SetEnableLogging(bool enable)
        {
            var types = typeof(TType);
            if (Instance.LoggingDic.ContainsKey(types))
            {
                Instance.LoggingDic[types] = enable;
            }
            else
            {
                Instance.LoggingDic.Add(types, enable);
            }
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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        // シングルトン実装確認用のログ出力
        Debug.Log("MyLogger コンストラクタ ");
#endif
        Logger = Debug.unityLogger;
    }

    /// <summary>
    /// Poor man's DI 用コンストラクタ
    /// </summary>
    /// <param name="logger"></param>
    public MyLogger(ILogger logger)
    {
        Logger = logger;
    }
}