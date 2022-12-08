using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

using Logger = MyLogger.MapBy<TutorialSceneDomain>;

public class TutorialSceneDomain
{
    public class Parameter
    {
        public ISceneTransition NextSceneTransition = new TitleSceneTransition();
    }
    Parameter CreateParam;

    ExSceneManager _exSceneManager;
    public TutorialSceneDomain()
    {
        // TODO　親かマネージャーが生成or引き渡し
        CreateParam = new Parameter();
        _exSceneManager = ExSceneManager.Instance;
    }

    /// <summary>
    /// 次のシーンへ遷移
    /// </summary>
    public async UniTask ChangeSceneNext()
    {
        Logger.Debug("CreateParam null?:" + (null == CreateParam));
        Logger.Debug("nextSceneTransition null?:" + (null == CreateParam.NextSceneTransition));
        // TODO　チュートリアルシーンを重ねて開始して、次シーンはチュートリアルシーン終了だけでもいい
        // が、シーン開発分離とExSceneの機構で親シーンが子シーンを制御する作りになっていない
        await _exSceneManager.Replace(CreateParam.NextSceneTransition);
    }
}
