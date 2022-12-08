using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

using Logger = MyLogger.MapBy<TitleScene>;

/// <summary>
/// �^�C�g���V�[���̓����W�b�N���C���[�V�[��
/// 
/// �N���X���̓V�[���I�u�W�F�N�g�Ɠ��ꖼ�ɂ���
/// </summary>
[PageAsset("TitleScenePage.prefab")]
public class TitleScene : MonoBehaviour, ILayeredSceneLogic
{
    TitleSceneDomain _domain;

    [Inject]
    public void Construct(TitleSceneDomain domain)
    {
        _domain = domain;
        Logger.SetEnableLogging(true);
        Logger.Debug("Construct ���̂���H�F" + (domain != null));
        _domain.SetLayerScene(this);
    }

    void Start()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        // �񓯊����\�b�h���s
        DelayAsync(ct).Forget();
    }
    // TODO�e�X�g�p�񓯊����\�b�h
    private async UniTask DelayAsync(CancellationToken token)
    {
        await UniTask.Delay(
            TimeSpan.FromSeconds(1),
            DelayType.UnscaledDeltaTime,
            PlayerLoopTiming.Update, token);

        Logger.SetEnableLogging(true);
        Logger.Debug("TryStart��������");
        await _domain.TryStart();
        Logger.Debug("TryStart�������� ����� -------------------");
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        Logger.UnloadEnableLogging();
    }

    //async void OnButtonClicked()
    //{
    //    // TODO �C�x���g�n���h�����O�Ńh���C���Ăяo����
    //    // ���������h���C�����̂��n���h�����O���邩
    //    //var domain = new TitleSceneDomain();
    //    //await domain.Login();
    //}
}

public class TitleSceneTransition : LayerdSceneTransition<TitleUIScene.CreateParameter>
{
    public TitleSceneTransition()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(TitleScene) },
            { SceneLayer.UI, typeof(TitleUIScene) },
            //{ SceneLayer.Field, typeof(TitleFieldScene) },
        };
    }
}

