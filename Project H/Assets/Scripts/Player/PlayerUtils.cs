using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Relies on a Rigidbody2D and the MutliLock class
// Offers utils for a playerController
// playerDirectionAsNumber will give 1 or -1 depending on the direction faced
// canMoveLocker is a multiLocker that is meant to list lockers for player movement
// _canMove tracks if any lockers for player movement currently exist and returns true if none exist
// Edit enum MoveLocker to add or remove types of movement lockers

// Allows Mod button management, bind input button to OnMod function
// automatically updates IsModPressed animator param if exists

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerUtils : MonoBehaviour
{
    [Header("Components")]
    Rigidbody2D rb;
    Animator anim;

    public float playerdirectionAsNumber => rb.transform.localScale.x > 0 ? 1 : -1;
    public MultiLock<MoveLocker> canMoveLocker = new MultiLock<MoveLocker>();
    public bool _canMove => canMoveLocker.IsFree();

    [Header("Input")]
    [SerializeField]
    private bool _isModPressed = false;

    public bool IsModPressed
    {
        get { return _isModPressed; }
        set
        {
            _isModPressed = value;
            if (anim != null && GeneralUtils.HasParameter(AnimationStrings.IsModPressed, anim))
                anim.SetBool(AnimationStrings.IsModPressed, value);
        }
    }


    [SerializeField]
    private bool _isFacingRight = true;
    public bool IsFacingRight
    {
        get
        {
            return _isFacingRight;
        }
        set
        {
            if (_isFacingRight != value)
            {
                transform.localScale *= new Vector2(-1, 1);
                _isFacingRight = value;
            }
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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

public enum MoveLocker
{
    WallSlide,
    wallHop
}