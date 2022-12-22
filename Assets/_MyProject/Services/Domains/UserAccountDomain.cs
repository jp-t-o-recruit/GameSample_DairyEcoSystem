using System.Collections.Generic;

/// <summary>
/// �V�i���I�i�s�Ǘ����R�[�h
/// </summary>
public class ScenarioProgressionRecord
{
    /// <summary>
    /// �D�揇��
    /// </summary>
    public int Priority;

    /// <summary>
    /// �V�i���IID
    /// </summary>
    public int ID;
}


/// <summary>
/// TODO ���[�U�[���̃v���p�e�B�݌v����Ȃ��̃l�X�g
/// </summary>
public class UserInofBuild
{
    /// <summary>
    /// �A�N�e�B�u�ȃV�i���I�i�s�\
    /// </summary>
    public List<ScenarioProgressionRecord> ScenarioProgression;

    public UserInofBuild()
    {
        ScenarioProgression = new();
    }
}

/// <summary>
/// ���[�U�[���
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
        UserId = "���ꂪ�Q�ƁA�\�������̂͂��������B---UserId";
        UserName = "���ꂪ�Q�ƁA�\�������̂͂��������B---UserName";
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
/// UserAccountDomain�V���O���g���Ǘ�
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