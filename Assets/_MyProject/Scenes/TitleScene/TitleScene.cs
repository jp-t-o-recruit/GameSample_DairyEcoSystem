using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

using LocalLogger = MyLogger.MapBy<TitleScene>;

/// <summary>
/// �^�C�g���V�[���̓����W�b�N���C���[�V�[��
/// 
/// �N���X���̓V�[���I�u�W�F�N�g�Ɠ��ꖼ�ɂ���
/// </summary>
[PageAsset("TitleScenePage.prefab")]
public class TitleScene : MonoBehaviour
{
    TitleSceneDomain _domain;

    [Inject]
    public TitleScene(TitleSceneDomain domain)
    {
        _domain = domain;
        //_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();
    }

    void Start()
    {
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        LocalLogger.UnloadEnableLogging();
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
        SceneName = _layer[SceneLayer.Logic].ToString();
    }
}


public interface ITitleModel
{
    public string GetRandomText();
}
public class TitleModel: ITitleModel
{

    public string GetRandomText()
    {
        return "123";
    }
}

public class TitleModelMock : ITitleModel
{
    public string GetRandomText()
        => "Test";
}

// Presenter�N���X
public class TitleScenePresenter : IStartable
{
    // TODO �V�[���ύX��OnDestroy�iDisposable�j�����̂��H
    private readonly TitleView _view;
    private readonly ITitleModel _model;

    [Inject]
    public TitleScenePresenter(TitleView view, ITitleModel model)
    {
        _view = view;
        _model = model;
    }

    public void Start()
    {
        // MonoBehavior��Start���\�b�h���Ă΂��^�C�~���O�Ŏ��s�����(IStartable�̂�����)
        var text = _model.GetRandomText();
        _view.DrawText(text);
    }
}