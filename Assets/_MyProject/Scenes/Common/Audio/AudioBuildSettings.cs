using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEditor;
using UnityEngine;


/// <summary>
/// �yUnite Tokyo 2018�zAudio�@�\�̊�b�Ǝ����e�N�j�b�N
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

    //Audio Clip�̃p�����[�^�[�ݒ�������� 
    //TODO
    // Asset Post Processor���g�� 
        
    public void OnPostprocessAudio(AudioClip AudioClip)
    {
        //AudioImporter audioImporter = assetImporter as AudioImporter;
        //// MenuSe�t�H���_�̒��g�͑S�����m�������@// 
        //// �l�[�~���O�Ƃ���MenuSe������΃��m��������
        //string path = audioImporter.assetPath;
        //audioImporter.forceToMono = path.Contains("MenuSe");

        ////BGM�t�H���_�̒��g�͑S���o�b�N�O���E���h�ǂݍ���//
        //// �l�[�~���O�Ƃ���BGM������΃o�b�N�O���E���h���[�h����
        //audioImporter.loadInBackground = path.Contains("BGM");
        
        //// ���v���b�g�t�H�[�����Ƃ̐ݒ��AudioImporterSampleSettings�N���X�o�R�Őݒ�   
    }
}
