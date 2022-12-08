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
        // TODO�@�e���}�l�[�W���[������or�����n��
        CreateParam = new Parameter();
        _exSceneManager = ExSceneManager.Instance;
    }

    /// <summary>
    /// ���̃V�[���֑J��
    /// </summary>
    public async UniTask ChangeSceneNext()
    {
        Logger.Debug("CreateParam null?:" + (null == CreateParam));
        Logger.Debug("nextSceneTransition null?:" + (null == CreateParam.NextSceneTransition));
        // TODO�@�`���[�g���A���V�[�����d�˂ĊJ�n���āA���V�[���̓`���[�g���A���V�[���I�������ł�����
        // ���A�V�[���J��������ExScene�̋@�\�Őe�V�[�����q�V�[���𐧌䂷����ɂȂ��Ă��Ȃ�
        await _exSceneManager.Replace(CreateParam.NextSceneTransition);
    }
}
