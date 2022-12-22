using System;
using UnityEngine;

/// <summary>
/// �X�N���v�^�u���I�u�W�F�N�g
/// 
/// �g���悤
/// https://qiita.com/Kosei-Yoshida/items/d7dc37bcde3a52cb5265#scriptableobject
/// </summary>
[CreateAssetMenu(fileName = "CreditNotationSceneParamsSO.asset", menuName = "_MyProject/ScriptableObject/CreditNotationSceneParamsSO", order = 0)]
public class CreditNotationSceneParamsSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("�N���W�b�g")]
    [SerializeField] private string initViewLabel = "���f��: OtoLogic";
    [NonSerialized] public string ViewLabel = "���f��: OtoLogic";

    //CreditNotationSceneParamsSO���ۑ����Ă���ꏊ�̃p�X
    public const string PATH = "CreditNotationSceneParamsSO";

    //CreditNotationSceneParamsSO�̎���
    private static CreditNotationSceneParamsSO _entity;
    public static CreditNotationSceneParamsSO Entity
    {
        get
        {
            //���A�N�Z�X���Ƀ��[�h����
            if (_entity == null)
            {
                _entity = Resources.Load<CreditNotationSceneParamsSO>(PATH);

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
