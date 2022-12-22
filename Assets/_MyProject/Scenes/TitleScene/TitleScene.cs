using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VContainer;

using Logger = MyLogger.MapBy<TitleScene>;

/// <summary>
/// �^�C�g���V�[���̓����W�b�N���C���[�V�[��
/// 
/// �N���X���̓V�[���I�u�W�F�N�g�Ɠ��ꖼ�ɂ���
/// </summary>
public class TitleScene : MonoBehaviour, ILayeredSceneLogic
{
    [Inject]
    public void Construct()
    {
        //Logger.SetEnableLogging(true);
        //Logger.Debug("Construct ���̂���H�F" + (domain != null));
        //_domain.SetLayerScene(this);
    }
    void Awake()
    {
    }
    void Start()
    {
        Logger.SetEnableLogging(false);
        Logger.Debug("TitleScene �X�^�[�g�I");

        if (EditorApplication.isPlaying)
        {
            //ExSceneManager.Instance.NoticeDefaultTransition(() => new TitleSceneTransitioner());
        }
    }
    // TODO�e�X�g�p�񓯊����\�b�h
    private async UniTask DelayAsync(CancellationToken token)
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1),
            DelayType.UnscaledDeltaTime,
            PlayerLoopTiming.Update, token);
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        Logger.UnloadEnableLogging();
    }
}