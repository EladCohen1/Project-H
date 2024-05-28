using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving && IsModPressed)
            {
                return runSpeed;
            }
            else if (IsMoving)
            {
                return walkSpeed;
            }
            return 0;
        }
    }

    [Header("Components")]
    Rigidbody2D rb;
    Animator anim;

    [Header("Inputs")]
    private Vector2 moveInput;

    // Public status bools
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; anim.SetBool(AnimationStrings.IsMoving, value); } }
    public bool IsModPressed { get { return _isModPressed; } set { _isModPressed = value; anim.SetBool(AnimationStrings.IsModPressed, value); } }

    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        private set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
                _isFacingRight = value;
            }
        }
    }

    [Header("Status Bools")]
    // Private status bools
    [SerializeField]
    private bool _isMoving = false;
    [SerializeField]
    private bool _isModPressed = false;
    [SerializeField]
    private bool _isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        HandlePlayerRotation();
        rb.velocity = new Vector2(GetXMovement(), rb.velocity.y);
    }

    private float GetXMovement()
    {
        float movement = 0;
        if (moveInput.x > 0)
        {
            movement = 1;
        }
        else if (moveInput.x < 0)
        {
            movement = -1;
        }
        return CurrentMoveSpeed * movement;
    }

    private void HandlePlayerRotation()
    {
        if (GetXMovement() > 0)
        {
            IsFacingRight = true;
        }
        else if (GetXMovement() < 0)
        {
            IsFacingRight = false;
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        IsMoving = moveInput.x != 0;
    }

    public void OnMod(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsModPressed = true;
        }
        else if (context.canceled)
        {
            IsModPressed = false;
        }
    }
}
