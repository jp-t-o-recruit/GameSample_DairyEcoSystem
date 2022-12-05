using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �{�^���A�j���[�V��������N���X
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
    /// �R���X�g���N�^
    /// </summary>
    /// <param name="ownar">�A�j���\�V����������g�����X�t�H�[�������I�[�i�[</param>
    public ButtonAnimation(Transform ownar) => _ownarTransform = ownar;

    /// <summary>
    /// �A�j���[�V�����J�n
    /// </summary>
    /// <param name="time">�X���[�X����</param>
    /// <param name="endScale">�ό`�I�����_�̃��[�J���X�P�[��</param>
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
    /// �A�j���[�V������~
    /// </summary>
    public void StopAnim()
    {
        _handle = null;
    }

    /// <summary>
    /// ���[�J���X�P�[���ύX����X���[�X�A�j���[�V����
    /// </summary>
    private void SmoothAnim()
    {
        // �A�j���[�V�����ŏ��X��end�ɋ߂Â���
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
    /// �A�j���[�V�����I��
    /// </summary>
    private void OnEndAnim()
    {
        OnAnimEndCallback?.Invoke();
        _handle = null;
    }

    /// <summary>
    /// �X�V�ɂ��A�j���[�V�����Đ�
    /// </summary>
    public void Update()
    {
        if (null != _handle) _handle();
    }
}
