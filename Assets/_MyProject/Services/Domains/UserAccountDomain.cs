using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユーザー情報
/// </summary>
[System.Serializable]
public class UserInfo
{
    public string userId;
    public string userName;
}

public class NullUserInfo: UserInfo
{
    public NullUserInfo()
    {
        userId = "これが参照、表示されるのはおかしい。---UserId";
        userName = "これが参照、表示されるのはおかしい。---UserName";
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