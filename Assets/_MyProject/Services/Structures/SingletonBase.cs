using System;

/// <summary>
/// �V���O���g��������N���X
/// 
/// class Hoge : SingletonBase<Hoge> ���`��
/// Hoge.Instance�����̂Ɏ���
/// </summary>
/// <typeparam name="SelfType"></typeparam>
public abstract class SingletonBase<SelfType> where SelfType : SingletonBase<SelfType>, new()
{
    /// <summary>
    /// �R���X�g���N�^����
    /// </summary>
    protected SingletonBase() { }

    /// <summary>
    /// �V���O���g������
    /// </summary>
    public static SelfType Instance
    {
        get
        {
            if (null == _instance)
            {
                
                _instance = new Lazy<SelfType>();
            }
            return _instance.Value;
        }
    }

    /// <summary>
    /// �V���O���g������
    /// �}���`�X���b�h�Ή��̂���Lazy
    /// </summary>
    private static Lazy<SelfType> _instance;

}