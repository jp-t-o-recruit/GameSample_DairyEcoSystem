using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEditor;
using UnityEngine;


/// <summary>
/// 【Unite Tokyo 2018】Audio機能の基礎と実装テクニック
/// https://www.slideshare.net/UnityTechnologiesJapan002/unite-tokyo-2018audio
/// </summary>
public class AudioBuildSettings
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Audio Clipのパラメーター設定を自動化 
    //TODO
    // Asset Post Processorを使う 
        
    public void OnPostprocessAudio(AudioClip AudioClip)
    {
        //AudioImporter audioImporter = assetImporter as AudioImporter;
        //// MenuSeフォルダの中身は全部モノラル化　// 
        //// ネーミングとしてMenuSeがあればモノラル強制
        //string path = audioImporter.assetPath;
        //audioImporter.forceToMono = path.Contains("MenuSe");

        ////BGMフォルダの中身は全部バックグラウンド読み込み//
        //// ネーミングとしてBGMがあればバックグラウンドロード強制
        //audioImporter.loadInBackground = path.Contains("BGM");
        
        //// ※プラットフォームごとの設定はAudioImporterSampleSettingsクラス経由で設定   
    }
}
