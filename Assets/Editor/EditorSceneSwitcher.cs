#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;

/// <summary>
/// Hierarchy��Ƀ`�F�b�N�{�b�N�X�ŃV�[����Load�AUnLoad��Ԃ�؂�ւ����@�\
/// </summary>
[InitializeOnLoad]
public class EditorSceneSwitcher : Editor
{
    // �`�F�b�N�{�b�N�X�\���ʒu
    const float Width = 40f;

    static EditorSceneSwitcher()
    {
        EditorApplication.hierarchyWindowItemOnGUI += DrawComponentToggle;
    }

    static void DrawComponentToggle(int instanceID, Rect rect)
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (EditorUtility.InstanceIDToObject(instanceID))
        {
            return;
        }

        // �V�[���I�u�W�F�N�g�擾
        var miGetSceneByHandle = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.NonPublic | BindingFlags.Static);
        Scene s = (Scene)miGetSceneByHandle.Invoke(null, new object[] { instanceID });

        // �`�F�b�N�{�b�N�X�\���ʒu
        rect.x += rect.width - Width;
        rect.width = Width;

        // �`�F�b�N�{�b�N�X�\��
        if (s.isLoaded != GUI.Toggle(rect, s.isLoaded, ""))
        {
            if (s.isLoaded)
            {
                EditorSceneManager.SaveScene(s);
                // �A�����[�h���ăV�[����1�ȏ�c��Ȃ�A�����[�h
                if (EditorSceneManager.loadedSceneCount > 1)
                {
                    EditorSceneManager.CloseScene(s, false);
                }
            }
            else
            {
                EditorSceneManager.OpenScene(s.path, OpenSceneMode.Additive);
            }
        }
    }
}
#endif