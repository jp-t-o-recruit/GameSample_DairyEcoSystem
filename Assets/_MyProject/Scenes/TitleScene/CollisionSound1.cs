using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 音出すテスト
/// 
/// 作り方コピペ
/// https://dianxnao.com/unity%EF%BC%9A%E3%81%B6%E3%81%A4%E3%81%8B%E3%81%A3%E3%81%9F%E6%99%82%E3%81%AB%E9%9F%B3%E3%82%92%E9%B3%B4%E3%82%89%E3%81%99/
/// </summary>
public class CollisionSound1 : MonoBehaviour
{
    // ぶつかった時の音
    public AudioClip se;
    // AudioClip再生用
    AudioSource audiosource1;

    MyInputActions _MyInputActions;

    // Start is called before the first frame update
    void Start()
    {
        // AudioSourceコンポーネント取得
        audiosource1 = GetComponent<AudioSource>();

        _MyInputActions = new MyInputActions();
        _MyInputActions.Enable();
        _MyInputActions.Player.Reset.performed += Sound;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // ぶつかった時に音を鳴らす
    void OnCollisionEnter(Collision col)
    {
    }

    void Sound(InputAction.CallbackContext context)
    {
        // AudioSource.PlayClipAtPoint(se, transform.position);
        audiosource1.PlayOneShot(se);
    }
}