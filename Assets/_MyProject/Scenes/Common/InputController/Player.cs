using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputSystemのチュートリアル
/// https://forpro.unity3d.jp/unity_pro_tips/2021/05/20/1957/
/// </summary>
public class Player : MonoBehaviour
{
    Vector3 move;

    float Speed;
    List<float> SpeedList = new () { 1f, 3f, 5f, 10f, 20f };
    MyInputActions _MyInputActions;

    public Player()
    {
        Speed = SpeedList.First();
    }
    private void Start()
    {
        _MyInputActions = new MyInputActions();
        _MyInputActions.Enable();
        _MyInputActions.Player.Move.performed += OnMove;
        _MyInputActions.Player.Fire.performed += OnFire;
        _MyInputActions.Player.ChangeSpeed.performed += OnChangeSpeed;
        _MyInputActions.Player.Reset.performed += OnReset;
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        transform.position = Vector3.zero;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Fire");
        }
    }

    public void OnChangeSpeed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            var index = SpeedList.IndexOf(Speed);
            var nextIndex = (SpeedList.Count > index + 1) ? index + 1 : 0;
            Speed = SpeedList[nextIndex];
        }
    }

    void Update()
    {
        transform.Translate(move * Speed * Time.deltaTime);
    }
}