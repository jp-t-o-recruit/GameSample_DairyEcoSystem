using VContainer;

public interface ICharacterNameConsultant
{
    string Get(int id);
}

public class CharacterNameConsultant : ICharacterNameConsultant
{
    public string Get(int id)
    {
        // �Ⴆ��SQLite�Ȃǂ��g���ăf�[�^�x�[�X����id�ɑΉ����閼�O�����o���Ă���
        var name = "";
        return name;
    }
}

public class CharacterNameConsultantMock : ICharacterNameConsultant
{
    public string Get(int id)
    {
        // �K���u�e�X�g�v�Ƃ���������Ԃ�
        return "�e�X�g";
    }
}

public interface ICharacterRoleConsultant
{
    string Get(int id);
}

public class CharacterRoleConsultant : ICharacterRoleConsultant
{
    public string Get(int id)
    {
        // �Ⴆ��SQLite�Ȃǂ��g���ăf�[�^�x�[�X����id�ɑΉ������E�����o���Ă���
        var role = "";
        return role;
    }
}

public class CharacterRoleConsultantMock : ICharacterRoleConsultant
{
    public string Get(int id)
    {
        // �K���u�e�X�g�v�Ƃ���������Ԃ�
        return "�e�X�g";
    }
}

public class CharacterLibrary
{
    private readonly ICharacterNameConsultant _nameConsultant;
    private readonly ICharacterRoleConsultant _roleConsultant;

    [Inject]
    public CharacterLibrary(ICharacterNameConsultant nameConsultant, ICharacterRoleConsultant roleConsultant)
    {
        _nameConsultant = nameConsultant;
        _roleConsultant = roleConsultant;
    }

    public string Get(int id)
    {
        // ���O��Ԃ����͕K���u��E�� + ���O�v�̌`�Ƃ���
        var name = _nameConsultant.Get(id);
        var role = _roleConsultant.Get(id);

        return name + role;
    }
}