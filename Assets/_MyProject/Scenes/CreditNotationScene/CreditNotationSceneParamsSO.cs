using System;
using UnityEngine;

/// <summary>
/// スクリプタブルオブジェクト
/// 
/// 使いよう
/// https://qiita.com/Kosei-Yoshida/items/d7dc37bcde3a52cb5265#scriptableobject
/// </summary>
[CreateAssetMenu(fileName = "CreditNotationSceneParamsSO.asset", menuName = "_MyProject/ScriptableObject/CreditNotationSceneParamsSO", order = 0)]
public class CreditNotationSceneParamsSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("クレジット")]
    [SerializeField] private string initViewLabel = "音素材: OtoLogic";
    [NonSerialized] public string ViewLabel = "音素材: OtoLogic";

    //CreditNotationSceneParamsSOが保存してある場所のパス
    public const string PATH = "CreditNotationSceneParamsSO";

    //CreditNotationSceneParamsSOの実体
    private static CreditNotationSceneParamsSO _entity;
    public static CreditNotationSceneParamsSO Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<CreditNotationSceneParamsSO>(PATH);

                //ロード出来なかった場合はエラーログを表示
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
        // Editor上では再生中に変更したScriptableObject内の値が実行終了時に消えない。
        // そのため、初期値と実行時に使う変数は分けておき、初期化する必要がある。
        ViewLabel = initViewLabel;
    }

    public void OnBeforeSerialize() { /* do nothing */ }
}
