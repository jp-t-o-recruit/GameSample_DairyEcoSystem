using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �X�N���v�^�u���I�u�W�F�N�g
/// 
/// �g���悤
/// https://qiita.com/Kosei-Yoshida/items/d7dc37bcde3a52cb5265#scriptableobject
/// </summary>
[CreateAssetMenu(fileName = "TitleSceneParamsSO.asset", menuName = "_MyProject/ScriptableObject/TitleSceneParamsSO", order = 0)]
public class TitleSceneParamsSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("�\�����x��")]
    [SerializeField] private string initViewLabel = "�^�C�g����ʁ@ScriptableObject����ݒ�";
    [NonSerialized] public string ViewLabel = "�^�C�g����ʁ@ScriptableObject����ݒ�";

    //TitleSceneParamsSO���ۑ����Ă���ꏊ�̃p�X
    public const string PATH = "TitleSceneParamsSO";

    //TitleSceneParamsSO�̎���
    private static TitleSceneParamsSO _entity;
    public static TitleSceneParamsSO Entity
    {
        get
        {
            //���A�N�Z�X���Ƀ��[�h����
            if (_entity == null)
            {
                _entity = Resources.Load<TitleSceneParamsSO>(PATH);

                //���[�h�o���Ȃ������ꍇ�̓G���[���O��\��
                if (_entity == null)
                {
                    Debug.LogError(PATH + " not found");
                }
            }

            return _entity;
        }
    }

    public void OnAfterDeserialize()
    {
        // Editor��ł͍Đ����ɕύX����ScriptableObject���̒l�����s�I�����ɏ����Ȃ��B
        // ���̂��߁A�����l�Ǝ��s���Ɏg���ϐ��͕����Ă����A����������K�v������B
        ViewLabel = initViewLabel;
    }

    public void OnBeforeSerialize() { /* do nothing */ }
}
