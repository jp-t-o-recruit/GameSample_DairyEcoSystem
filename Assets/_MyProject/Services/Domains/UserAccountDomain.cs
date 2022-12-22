using System.Collections.Generic;

/// <summary>
/// シナリオ進行管理レコード
/// </summary>
public class ScenarioProgressionRecord
{
    /// <summary>
    /// 優先順位
    /// </summary>
    public int Priority;

    /// <summary>
    /// シナリオID
    /// </summary>
    public int ID;
}


/// <summary>
/// TODO ユーザー情報のプロパティ設計未定なものネスト
/// </summary>
public class UserInofBuild
{
    /// <summary>
    /// アクティブなシナリオ進行表
    /// </summary>
    public List<ScenarioProgressionRecord> ScenarioProgression;

    public UserInofBuild()
    {
        ScenarioProgression = new();
    }
}

/// <summary>
/// ユーザー情報
/// </summary>
[System.Serializable]
public class UserInfo
{
    public string UserId;
    public string UserName;
    public UserInofBuild Build;

    public UserInfo(UserInofBuild build = default)
    {
        Build = build ?? new();
    }
}

public class NullUserInfo: UserInfo
{
    public NullUserInfo()
    {
        UserId = "これが参照、表示されるのはおかしい。---UserId";
        UserName = "これが参照、表示されるのはおかしい。---UserName";
    }
}

public class UserAccountDomain
{
    public UserInfo User { get; private set; }

    public UserAccountDomain(UserInfo userInfo)
    {        
        LoginUser(userInfo);
    }

    public bool LoginUser(UserInfo userInfo)
    {
        User = userInfo ?? new NullUserInfo();
        return true;
    }

    public bool IsLogined()
    {
        if (User.GetType() == typeof(NullUserInfo))
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// UserAccountDomainシングルトン管理
/// </summary>
public class UserAccountDomainManager
{
    private static UserAccountDomain _instance;

    public static UserAccountDomain GetService()
    {
        if (null == _instance)
        {
            _instance = new UserAccountDomain(new NullUserInfo());
        }
        return _instance;
    }
}