using NUnit.Framework;

/// <summary>
/// 
/// Test Frameworkの使い方
/// https://light11.hatenadiary.com/entry/2020/02/21/212657
/// 
/// Assembly Definitionの説明
/// https://qiita.com/toRisouP/items/d206af3029c7d80326ed
/// </summary>
public class CharacterTest
{
    [Test]
    public void CharacterNameConsultantTest()
    {
        var name = new CharacterNameConsultant().Get(1);
        Assert.AreEqual(name, "");
    }

    [Test]
    public void CharacterRoleConsultantTest()
    {
        var name = new CharacterRoleConsultant().Get(1);
        Assert.AreEqual(name, "");
    }

    [Test]
    public void CharacterLibraryTest()
    {
        var library = new CharacterLibrary(new CharacterNameConsultantMock(), new CharacterRoleConsultantMock());
        var result = library.Get(1);

        Assert.AreEqual(result, "テストテスト");
    }

    [Test]
    public void CharacterLibraryAndCharacterNameConsultantTest()
    {
        var library = new CharacterLibrary(new CharacterNameConsultant(), new CharacterRoleConsultantMock());
        var result = library.Get(1);

        Assert.AreEqual(result, "テスト");
    }

    [Test]
    public void CharacterLibraryAndCharacterRoleConsultantTest()
    {
        var library = new CharacterLibrary(new CharacterNameConsultantMock(), new CharacterRoleConsultant());
        var result = library.Get(1);

        Assert.AreEqual(result, "テスト");
    }

    [Test]
    public void CharacterLibraryAndCharacterNameConsultantAndCharacterRoleConsultantTest()
    {
        var library = new CharacterLibrary(new CharacterNameConsultant(), new CharacterRoleConsultant());
        var result = library.Get(1);

        Assert.AreEqual(result, "");
    }
}