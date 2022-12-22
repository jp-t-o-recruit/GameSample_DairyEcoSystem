using UnityEngine;

/// <summary>
/// �o�g������
/// </summary>
public enum BattleResult
{
    /// <summary>
    /// �������i�����l�j
    /// </summary>
    Incomplete,
    /// <summary>
    /// �G���[
    /// </summary>
    Error,
    /// <summary>
    /// �M�u�A�b�v
    /// </summary>
    Giveup,
    /// <summary>
    /// ���g���C
    /// </summary>
    Retry,
    /// <summary>
    /// �s�k
    /// </summary>
    Lose,
    /// <summary>
    /// ��������
    /// </summary>
    Draw,
    /// <summary>
    /// ����
    /// </summary>
    Win,
}

public class BattleScene : MonoBehaviour, ILayeredSceneLogic
{
    public IScenario Scenario { get; }
    BattleScene(IScenario scenario)
    {
        Scenario = scenario;
    }

    private void BuildBattleScene(IScenario scenario)
    {
        // �o�g���ɕK�v�ȃ��\�[�X����������
        // Scenario�������Ă���������ƂɓG������}�b�v��p�ӂ���
    }

    /// <summary>
    /// �M�u�A�b�v
    /// </summary>
    private void Giveup()
    {
        // �V�i���I�ɃM�u�A�b�v��`����
    }

    /// <summary>
    /// �o�g���I�����U���g
    /// </summary>
    private void Result()
    {
        var f = Scenario.Params.State;

        if (f == default)
        {
            // �A�h���F���`���[���Ăяo���H
            // �V�[���h���C�����Ăяo�����Ǘ����ׂ��Ȃ̂��H
            // �V�[���ɏI����`���A���ʂ�Ԃ�
        }
        else
        {
            // ���U���g��\�����āA���̌�~�b�V�����I����ʂɖ߂�
        }
    }
}
