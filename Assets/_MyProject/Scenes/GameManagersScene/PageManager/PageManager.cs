using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using LocalLogger = MyLogger.MapBy<PageManager>;

/// <summary>
/// �y�[�W�}�l�[�W���[
/// https://learning.unity3d.jp/6688/
/// �yUnity�zDI�R���e�iVContainer�̎g�����܂Ƃ�
/// https://light11.hatenadiary.com/entry/2021/02/01/203252
/// VContainer����(1) - IContainerBuilder��IObjectResolver
/// https://qiita.com/sakano/items/3a009019e279024fda19
/// </summary>
public class PageManager : SingletonBase<PageManager>
{
    List<BasePage> _pages;

    /// <summary>
    /// �V���O���g������
    /// </summary>
    //public static PageManager Instance {
    //    get {
    //        if (null == _instance)
    //        {
    //            _instance = new PageManager();
    //        }
    //        return _instance;
    //    }
    //}
    //private static PageManager _instance;

    ///// <summary>
    ///// �R���X�g���N�^
    ///// </summary>
    public PageManager()
    {
        LocalLogger.SetEnableLogging(true);
        LocalLogger.Debug("PageManager �R���X�g���N�^�I");
        _pages = new List<BasePage>();
    }

    ///// <summary>
    ///// �}�l�[�W���[�Ƃ��Ď��g���쐬
    ///// </summary>
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //private static void CreateSelf()
    //{
    //    //Manager�Ƃ������O��GameObject���쐬���ASceneManager�Ƃ����N���X��Add����
    //    new GameObject("Manager", typeof(PageManager));
    //}

    // Start is called before the first frame update
    void Start()
    {
        //MyLogger = new MyDebugLogger() { IsLogging = true };
        LocalLogger.Debug("PageManager Start�I");

        //Invoke("ChangeScene", 3f);
        //Invoke("EndScene", 3f + 5f);


        //ChangeScene("SampleScene");
        //EndSceneAsync("SampleScene");
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDestroy()
    {
        LocalLogger.Debug("Page Manager OnDestroy�I");
    }

    public void ChangeScene(string sceneName)
    {
        // File��BuildSettings �ł̓o�^���K�v
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    async void EndSceneAsync(string sceneName)
    {
        // File��BuildSettings �ł̓o�^���K�v
        await SceneManager.UnloadSceneAsync(sceneName);
        await UniTask.Delay(5000);
        await Resources.UnloadUnusedAssets();
    }

    public async UniTask PushAsync(IPageTransition transition)
    {
        var page = await transition.LoadPage();
        await page.Initialize();
        _pages.Add(page);
        Reflesh();
    }

    internal async UniTask Replace(IPageTransition transition)
    {
        await PageTopDelete();
        await PushAsync(transition);
    }

    internal async UniTask ReplaceAll(IPageTransition transition)
    {
        // �X�^�b�N�I�N���A�Ȃ̂ŋt���Ăяo��(�ꉞ���X�g�̔j�󂵂Ȃ��t���Ăяo��)
        foreach (var page in _pages.AsEnumerable().Reverse())
        {
            await page.Discard();
        }
        _pages.Clear();

        await PushAsync(transition);
    }

    internal async UniTask Pop()
    {
        await PageTopDelete();

        // �X�^�b�N�Ōオ�X�V���ꂽ�̂ōČĂяo��
        BasePage page = _pages.Last();
        if (page == null)
        {
            // TODO��O�����H
            // �V�[�����Z�b�g
            throw new InvalidOperationException("�d�l�Ƃ��Ă��������y�[�W���S���Ȃ��Ȃ�����");
        }
        else
        {
            await page.Resume();
        }
        Reflesh();
    }

    private async UniTask PageTopDelete()
    {
        // �X�^�b�N�Ō���폜
        BasePage popPage = _pages.Last();
        if (popPage == null)
        {
            // TODO��O�����H
            // �V�[�����Z�b�g
            throw new InvalidOperationException("�d�l�Ƃ��Ă��������y�[�W������������");
        }
        else
        {
            await popPage.Discard();
            _pages.RemoveAt(_pages.Count - 1);
        }
    }

    void Reflesh()
    {
        // TODO �X�V���r���[�ɔ��f
    }
}
