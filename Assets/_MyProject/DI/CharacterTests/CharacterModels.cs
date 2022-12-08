using VContainer;

public interface ICharacterNameConsultant
{
    string Get(int id);
}

public class CharacterNameConsultant : ICharacterNameConsultant
{
    public string Get(int id)
    {
        // 例えばSQLiteなどを使ってデータベースからidに対応する名前を取り出してくる
        var name = "";
        return name;
    }
}

public class CharacterNameConsultantMock : ICharacterNameConsultant
{
    public string Get(int id)
    {
        // 必ず「テスト」という文字を返す
        return "テスト";
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
        // 例えばSQLiteなどを使ってデータベースからidに対応する役職を取り出してくる
        var role = "";
        return role;
    }
}

public class CharacterRoleConsultantMock : ICharacterRoleConsultant
{
    public string Get(int id)
    {
        // 必ず「テスト」という文字を返す
        return "テスト";
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
        // 名前を返す時は必ず「役職名 + 名前」の形とする
        var name = _nameConsultant.Get(id);
        var role = _roleConsultant.Get(id);

        return name + role;
    }
}