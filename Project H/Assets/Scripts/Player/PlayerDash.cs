using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Requires use of PlayerMove Util for player movement
// Relies on a Rigidbody2D, TouchingDirections (util package), PlayerUtils (util package), PlayerController
// Offers 2 dash coroutines, Dash() and downDash, also makes use of unity's input system with the OnDash() function that should be attached to the dash button event

// Recommended to use in conjunction with PlayerMove Util, otherwise must make sure to disallow movement while dashing using the "_isDashing" bool

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(PlayerUtils))]
[RequireComponent(typeof(PlayerMove))]
public class PlayerDash : MonoBehaviour
{
    [Header("Components")]
    PlayerMove playerMove;
    TouchingDirections touchingDirections;
    Rigidbody2D rb;
    PlayerUtils utils;
    TrailRenderer tr;

    [Header("State")]
    [SerializeField]
    public bool _canDash = true;
    [SerializeField]
    public bool _isDashing = false;
    [SerializeField]
    public bool _isDashJumping = false;
    [SerializeField]
    public bool _canDownDash = true;

    // Stats
    public float dashImpulse = 20f;
    public float dashTime = 0.2f;
    public float dashCD = 0.7f;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        touchingDirections = GetComponent<TouchingDirections>();
        rb = GetComponent<Rigidbody2D>();
        utils = GetComponent<PlayerUtils>();
        tr = GetComponent<TrailRenderer>();
    }

    void FixedUpdate()
    {
        if (touchingDirections.IsGrounded)
        {
            _canDownDash = true;
        }
    }
    // Coroutines
    public IEnumerator Dash()
    {
        // Setup
        _canDash = false;
        _isDashing = true;
        float originalGrav = rb.gravityScale;
        rb.gravityScale = 0;
        if (tr)
            tr.emitting = true;
        // Dash
        rb.velocity = new Vector2(dashImpulse * utils.playerdirectionAsNumber, 0f);
        yield return new WaitForSeconds(dashTime);
        // Recovery
        if (tr)
            tr.emitting = false;
        rb.gravityScale = originalGrav;
        _isDashing = false;
        if (!_isDashJumping)
            rb.velocity = new Vector2(0f, rb.velocity.y);
        _isDashJumping = false;
        // Cooldown
        yield return new WaitForSeconds(dashCD);
        _canDash = true;
    }
    public IEnumerator downDash()
    {
        // Dash
        if (touchingDirections.IsGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            if (tr)
                tr.emitting = true;
            rb.velocity = new Vector2(0, -dashImpulse);
        }
        _canDownDash = false;
        yield return new WaitForSeconds(dashTime / 3);
        // Recovery
        if (tr)
            tr.emitting = false;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && utils._canMove)
        {
            if (_canDash && playerMove.IsMoving && !touchingDirections.IsOnWall)
                StartCoroutine(Dash());
            else if (playerMove.moveInput.y < 0 && _canDownDash && !playerMove.IsMoving)
                StartCoroutine(downDash());
            else if (_canDash && playerMove.moveInput.y >= 0 && !touchingDirections.IsOnWall)
                StartCoroutine(Dash());
        }
    }
}
