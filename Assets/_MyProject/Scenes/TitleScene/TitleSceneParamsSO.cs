using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スクリプタブルオブジェクト
/// 
/// 使いよう
/// https://qiita.com/Kosei-Yoshida/items/d7dc37bcde3a52cb5265#scriptableobject
/// </summary>
[CreateAssetMenu(fileName = "TitleSceneParamsSO.asset", menuName = "_MyProject/ScriptableObject/TitleSceneParamsSO", order = 0)]
public class TitleSceneParamsSO : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("表示ラベル")]
    [SerializeField] private string initViewLabel = "タイトル画面　ScriptableObjectから設定";
    [NonSerialized] public string ViewLabel = "タイトル画面　ScriptableObjectから設定";

    //TitleSceneParamsSOが保存してある場所のパス
    public const string PATH = "TitleSceneParamsSO";

    //TitleSceneParamsSOの実体
    private static TitleSceneParamsSO _entity;
    public static TitleSceneParamsSO Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<TitleSceneParamsSO>(PATH);

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
