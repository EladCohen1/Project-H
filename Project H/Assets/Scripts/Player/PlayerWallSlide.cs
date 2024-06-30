using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Requires creation of IsWallSliding, IsMoving, moveInput in PlayerController Where
// IsWallSliding is a bool that will store the wall sliding state of the player
// IsMoving is a bool that stores whether the player is currently moving (requires implementation)
// moveInput is a Vector2 that stores the movement of the player (requires implementation)

// Requires AnimationStrings Dic

// Reveals bool IsWallSliding that dynamically changes based on the character's state and updates "IsWallSliding" bool in the animator
// Reveals "allowWallSlide" allowing to disable wallsliding, true by default


[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerWallSlide : MonoBehaviour
{
    [Header("Components")]
    PlayerMove playerMove;
    PlayerUtils utils;
    TouchingDirections touchingDirections;
    Rigidbody2D rb;
    Animator anim;


    public bool IsWallSliding
    {
        get { return _isWallSliding; }
        set
        {
            _isWallSliding = value;
            if (anim != null && GeneralUtils.HasParameter(AnimationStrings.IsWallSliding, anim))
                anim.SetBool(AnimationStrings.IsWallSliding, value);
        }
    }
    private bool _isWallSliding = false;
    public bool allowWallSlide = true;
    public Coroutine wallSlideGraceCoroutine = null;

    [Header("Stats")]
    public float maxWallSlidingSpeed = 2f;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        utils = GetComponent<PlayerUtils>();
        touchingDirections = GetComponent<TouchingDirections>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (allowWallSlide)
        {
            CheckWallSliding();
            HandleWallSliding();
        }
    }

    public IEnumerator WallSlidingGrace()
    {
        yield return new WaitForSeconds(0.2f);
        utils.IsFacingRight = !utils.IsFacingRight;
    }

    public void CheckWallSliding()
    {
        if (!touchingDirections.IsGrounded && touchingDirections.IsOnSlidableWall && rb.velocity.y < 0 && playerMove.moveInput.y >= 0)
        {
            IsWallSliding = true; // wall sliding
            utils.canMoveLocker.Lock(MoveLocker.WallSlide); // stick to wall
        }
        else
        {
            IsWallSliding = false;
            playerMove.IsMoving = playerMove.moveInput.x != 0;
            utils.canMoveLocker.Unlock(MoveLocker.WallSlide);
        }

        // Checking for detaching from the wall by holding the other direction
        if (IsWallSliding)
        {
            if (playerMove.moveInput.x * touchingDirections.slidingWallXDirection < 0) // if holding the opposite direction from the wall
            {
                if (wallSlideGraceCoroutine == null)
                {
                    wallSlideGraceCoroutine = StartCoroutine(WallSlidingGrace());
                }
            }
            else
            {
                if (wallSlideGraceCoroutine != null)
                {
                    StopCoroutine(wallSlideGraceCoroutine);
                }
                wallSlideGraceCoroutine = null;
            }
        }
    }
    public void HandleWallSliding()
    {
        if (IsWallSliding)
        {
            if (rb.velocity.y <= -maxWallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxWallSlidingSpeed);
            }
        }
    }
}
