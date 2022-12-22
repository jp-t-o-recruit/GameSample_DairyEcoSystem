using System;
using System.Collections.Generic;
using System.Linq;

//public class SceneRelationService
//{
//    public static ISceneTransitioner GetSceneTransitionerByName(string sceneName)
//    {
//        if (EnumExt.TryParseToEnum(sceneName, out SceneEnum sceneEnum) == false) {
//            // ��O�ꗗ
//            // https://www.midnightunity.net/csharp-exception-summary/#%E4%BE%8B%E5%A4%96%E3%82%A8%E3%83%A9%E3%83%BC%E6%97%A9%E8%A6%8B%E8%A1%A8
//            throw new FormatException($"�V�[�����ϊ����ɗ�O���������܂����B{sceneName} is not SceneEnum.");
//        }
//        return GetSceneTransitionerByEnum(sceneEnum);
//    }

//    public static ISceneTransitioner GetSceneTransitionerByEnum(SceneEnum sceneEnum)
//    {
//        var relation = SceneRelationDefinition.Instance.SceneDefinition[sceneEnum];
//        ISceneTransitioner res = relation.CreateTansitioner();
//        return res;
//    }
//}

/// <summary>
/// ��������enum�Ƀp�[�X
/// 
/// �R�s�y��
/// https://qiita.com/masaru/items/a44dc30bfc18aac95015
/// </summary>
public static class EnumExt
{
    public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
        where TEnum : struct, IComparable, IFormattable, IConvertible   //�R���p�C�����ɂł��邾������
    {
        if (typeof(TEnum).IsEnum)
        {
            return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm);
        }
        else
        {
            //���s���`�F�b�N��enum����Ȃ������ꍇ
            enm = default;
            return false;
        }
    }
}
/// <summary>
/// �K�w�\���V�[���֘A�t����`
/// </summary>
//public class SceneRelationDefinition : SingletonBase<SceneRelationDefinition>
//{
//    readonly public Dictionary<SceneEnum, SceneTypes> SceneDefinition;

//    public SceneRelationDefinition()
//    {
//        SceneDefinition = new List<SceneTypes>() {
//            new (SceneEnum.TitleScene,() => new LayeredSceneTransitioner(new TitleSceneDomain()), () => new TitleSceneDomain()),
//            new (SceneEnum.HomeScene,() => new LayeredSceneTransitioner(new HomeSceneDomain()), () => new HomeSceneDomain()),
//            new (SceneEnum.BattleScene,() => new LayeredSceneTransitioner(new BattleSceneDomain()), () => new BattleSceneDomain()),
//            new (SceneEnum.CreditNotationScene,() => new LayeredSceneTransitioner(new CreditNotationSceneDomain()), () => NullDomain.Create())
//        }.ToDictionary(n => n.SceneEnum, n => n);
//    }
//}

/// <summary>
/// �K�w�\���V�[���֘A�t��
/// </summary>
//public class SceneTypes
//{
//    public SceneEnum SceneEnum;
//    private Func<ISceneTransitioner> _callbackT1;
//    private Func<ILayeredSceneDomain> _callbackT2;

//    public SceneTypes(SceneEnum sceneEnum,
//                      Func<ISceneTransitioner> callbackT1,
//                      Func<ILayeredSceneDomain> callbackT2)
//    {
//        SceneEnum = sceneEnum;
//        _callbackT1 = callbackT1;
//        _callbackT2 = callbackT2;
//    }

//    public ISceneTransitioner CreateTansitioner()
//    {
//        return _callbackT1();
//    }
//    public ILayeredSceneDomain CreateDomain()
//    {
//        return _callbackT2();
//    }

//}


/// <summary>
/// ���[�g�V�[����
/// 
/// File��BuildSettings �ł̓o�^�Ɠ��l�̂��̂��L��
/// </summary>
public enum SceneEnum
{
    GameManagersScene, // �V�[���Ԃ��R���g���[���������V�[������T�O�̃C���X�^���X���풓������
    TitleScene,
    HomeScene,
    TutorialScene,
    CreditNotationScene,
    BattleScene,
}
