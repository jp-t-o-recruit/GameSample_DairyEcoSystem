using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class HomeSceneTransition : LayerdSceneTransition<HomeUIScene.CreateParameter>
{
    public HomeSceneTransition()
    {
        _layer = new Dictionary<SceneLayer, System.Type>()
        {
            { SceneLayer.Logic, typeof(HomeScene) },
            { SceneLayer.UI, typeof(HomeUIScene) },
            { SceneLayer.Field, typeof(HomeFieldScene) },
        };
        SceneName = _layer[SceneLayer.Logic].ToString();
    }
}

//[PageAsset("HomeScenePage.prefab")]
//public class HomeScenePage : BasePage<HomeScenePage.CreateParameter>

//    //async void OnButtonClicked()
//    //{
//    //    ////���[�h�ς݂̃V�[���ł���΁A���O�ŕʃV�[�����擾�ł���
//    //    //Scene scene = SceneManager.GetSceneByName("GameManagersScene");

//    //    //PageManager pageManager;

//    //    ////GetRootGameObjects�ŁA���̃V�[���̃��[�gGameObjects
//    //    ////�܂�A�q�G�����L�[�̍ŏ�ʂ̃I�u�W�F�N�g���擾�ł���
//    //    //foreach (var rootGameObject in scene.GetRootGameObjects())
//    //    //{
//    //    //    pageManager = rootGameObject.GetComponent<PageManager>();
//    //    //    if (pageManager != null)
//    //    //    {
//    //    //        await pageManager.Replace(new FooPage.Transition() { Parameter = { Bar = "home�Ŏw��" } }); // ���݂̃y�[�W��j�����Ēǉ�����
//    //    //        break;
//    //    //    }
//    //    //}
//    //}
//}

