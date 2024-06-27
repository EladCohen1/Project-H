using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Requires creation of IsWallSliding, IsMoving, moveInput in PlayerController Where
// IsWallSliding is a bool that will store the wall sliding state of the player
// IsMoving is a bool that stores whether the player is currently moving (requires implementation)
// moveInput is a Vector2 that stores the movement of the player (requires implementation)

// Shows "allowWallSlide" allowing to disable wallsliding, true by default


[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerWallSlide : MonoBehaviour
{
    [Header("Components")]
    PlayerController playerController;
    PlayerUtils utils;
    TouchingDirections touchingDirections;
    Rigidbody2D rb;

    public bool allowWallSlide = true;
    public Coroutine wallSlideGraceCoroutine = null;

    [Header("Stats")]
    public float maxWallSlidingSpeed = 2f;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        utils = GetComponent<PlayerUtils>();
        touchingDirections = GetComponent<TouchingDirections>();
        rb = GetComponent<Rigidbody2D>();
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
        if (!touchingDirections.IsGrounded && touchingDirections.IsOnSlidableWall && rb.velocity.y < 0 && playerController.moveInput.y >= 0)
        {
            playerController.IsWallSliding = true; // wall sliding
            utils.canMoveLocker.Lock(MoveLocker.WallSlide); // stick to wall
        }
        else
        {
            playerController.IsWallSliding = false;
            playerController.IsMoving = playerController.moveInput.x != 0;
            utils.canMoveLocker.Unlock(MoveLocker.WallSlide);
        }

        // Checking for detaching from the wall by holding the other direction
        if (playerController.IsWallSliding)
        {
            if (playerController.moveInput.x * touchingDirections.slidingWallXDirection < 0) // if holding the opposite direction from the wall
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
        if (playerController.IsWallSliding)
        {
            if (rb.velocity.y <= -maxWallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxWallSlidingSpeed);
            }
        }
    }
}
