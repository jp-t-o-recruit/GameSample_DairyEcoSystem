using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ボタンアニメーション制御クラス
/// </summary>
class ButtonAnimation
{
    public Action
        OnAnimEndCallback;
    Vector3 _endScale;
    Vector3 _velocity = Vector3.zero;
    float _time;
    Action _handle;
    Transform _ownarTransform;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ownar">アニメ―ションさせるトランスフォームを持つオーナー</param>
    public ButtonAnimation(Transform ownar) => _ownarTransform = ownar;

    /// <summary>
    /// アニメーション開始
    /// </summary>
    /// <param name="time">スムース時間</param>
    /// <param name="endScale">変形終了時点のローカルスケール</param>
    public void StartAnim(float time, Vector3 endScale)
    {
        _time = time;
        _endScale = endScale;
        if (null != _handle)
        {
            OnEndAnim();
        }
        _handle = SmoothAnim;
    }

    /// <summary>
    /// アニメーション停止
    /// </summary>
    public void StopAnim()
    {
        _handle = null;
    }

    /// <summary>
    /// ローカルスケール変更するスムースアニメーション
    /// </summary>
    private void SmoothAnim()
    {
        // アニメーションで徐々にendに近づける
        _ownarTransform.localScale = Vector3.SmoothDamp(
            _ownarTransform.localScale,
            _endScale,
            ref _velocity,
            _time);

        if (_ownarTransform.localScale == _endScale)
        {
            OnEndAnim();
        }
    }

    /// <summary>
    /// アニメーション終了
    /// </summary>
    private void OnEndAnim()
    {
        OnAnimEndCallback?.Invoke();
        _handle = null;
    }

    /// <summary>
    /// 更新によるアニメーション再生
    /// </summary>
    public void Update()
    {
        if (null != _handle) _handle();
    }
}
