using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// ���[�gDI�R���e�i
/// 
/// DI�R���e�i�܂Ƃ�
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// </summary>
public class GameLifetimeScope : LifetimeScope
{
    //[SerializeField] public GameManagerView titleView;

    //[SerializeField] public TitleScene titleScene;
    //[SerializeField] public TitleUIScene titleUIScene;
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);


        if (EditorApplication.isPlaying)
        {
            //ExSceneManager.Instance.NoticeDefaultTransition(() => new TitleSceneTransitioner());
        }

        // ������DI�R���e�i�ɂǂ̃C���X�^���X���ǂ̌^�ŕۑ����Ă�����������

        // �C���X�^���X�𒍓�����N���X���w�肷��
        //builder.RegisterEntryPoint<TitleScenePresenter>(Lifetime.Singleton);

        // TitleModel�̃C���X�^���X��ITitleModel�̌^��DI�R���e�i�ɓo�^����
        //builder.Register<ITitleModel, TitleModel>(Lifetime.Singleton);

        // MonoBehavior���p�����Ă���N���X�͂��̂悤��DI�R���e�i�ɓo�^����
        // builder.RegisterComponentInHierarchy<TitleView>(); �ƋL�q����ƃq�G�����L�[����T���Ă��Ă����
        //builder.RegisterComponent(titleView);


        //builder.Register<IDisposable, CancellationTokenSource>(Lifetime.Transient);
        //builder.Register<TitleSceneDomain.DomainParam, TitleSceneDomain.DomainParam>(Lifetime.Transient);
        //builder.RegisterEntryPoint<TitleSceneDomain>(Lifetime.Singleton);

        ////_domain = DomainManager.GetDomain<TitleSceneDomain, TitleSceneDomain.DomainParam>();

        //builder.RegisterComponent(titleScene);
        //builder.RegisterComponent(titleUIScene);
    }
}



public interface ITitleModel
{
    public string GetRandomText();
}
public class TitleModel : ITitleModel
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
    private readonly GameManagerView _view;
    private readonly ITitleModel _model;

    [Inject]
    public TitleScenePresenter(GameManagerView view, ITitleModel model)
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