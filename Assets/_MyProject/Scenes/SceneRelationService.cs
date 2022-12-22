using System;
using System.Collections.Generic;
using System.Linq;

//public class SceneRelationService
//{
//    public static ISceneTransitioner GetSceneTransitionerByName(string sceneName)
//    {
//        if (EnumExt.TryParseToEnum(sceneName, out SceneEnum sceneEnum) == false) {
//            // 例外一覧
//            // https://www.midnightunity.net/csharp-exception-summary/#%E4%BE%8B%E5%A4%96%E3%82%A8%E3%83%A9%E3%83%BC%E6%97%A9%E8%A6%8B%E8%A1%A8
//            throw new FormatException($"シーン名変換中に例外が発生しました。{sceneName} is not SceneEnum.");
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
/// 文字からenumにパース
/// 
/// コピペ元
/// https://qiita.com/masaru/items/a44dc30bfc18aac95015
/// </summary>
public static class EnumExt
{
    public static bool TryParseToEnum<TEnum>(this string s, out TEnum enm)
        where TEnum : struct, IComparable, IFormattable, IConvertible   //コンパイル時にできるだけ制約
    {
        if (typeof(TEnum).IsEnum)
        {
            return Enum.TryParse(s, out enm) && Enum.IsDefined(typeof(TEnum), enm);
        }
        else
        {
            //実行時チェックでenumじゃなかった場合
            enm = default;
            return false;
        }
    }
}
/// <summary>
/// 階層構造シーン関連付け定義
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
/// 階層構造シーン関連付け
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
/// ルートシーン列挙
/// 
/// File→BuildSettings での登録と同様のものを記載
/// </summary>
public enum SceneEnum
{
    GameManagersScene, // シーン間をコントロールしたいシーンより上概念のインスタンスを常駐させる
    TitleScene,
    HomeScene,
    TutorialScene,
    CreditNotationScene,
    BattleScene,
}
