using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Relies on PlayerUtils util and touchingDirections util
// Offers air Acceleration lock and slow regain after wallhop
// PlayerMove Util use is recommended, otherwise self implement PlayerMove with the following properties: airAcceleration,airAccelerationMax,airAccelerationMin where:
// airAcceleration is the used air acceleration determinator
// airAccelerationMax is the default value of the air acceleration
// airAccelerationMin is the minimum air acceleration that will be the first value given to the player when they wall hop before regaining their full acceleration ability

// Shows "allowCheckWallHop" allowing to disable the wall hop check on fixedupdate that will update the value of "_canWallHop"

// Recommended to use in conjunction with "PlayerJump" util and set "canWallHop" to true there

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(PlayerUtils))]
public class PlayerWallHop : MonoBehaviour
{
    [Header("Components")]
    PlayerMove playerMove;
    PlayerUtils utils;
    TouchingDirections touchingDirections;
    Rigidbody2D rb;


    [Header("State")]
    [SerializeField]
    public bool _canWallHop = false;

    [Header("Stats")]
    public float wallhopXImpulse = 10f;
    public float wallhopYImpulse = 8f;

    // Variables
    public Coroutine wallHopGraceCoroutine = null;
    public bool allowCheckWallHop = true;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        utils = GetComponent<PlayerUtils>();
        touchingDirections = GetComponent<TouchingDirections>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (allowCheckWallHop)
            CheckWallHop();
    }

    public void CheckWallHop()
    {
        if (touchingDirections.IsOnSlidableWall || touchingDirections.IsOnSlidableWallFromBehind)
        {
            _canWallHop = true;
            if (wallHopGraceCoroutine != null)
            {
                StopCoroutine(wallHopGraceCoroutine);
            }
            wallHopGraceCoroutine = StartCoroutine(WallHopGrace());
        }
    }

    public IEnumerator WallHopGrace()
    {
        yield return new WaitForSeconds(0.2f);
        _canWallHop = false;
    }
    public IEnumerator WallHopLock()
    {
        // slowly regaining air control
        utils.canMoveLocker.Lock(MoveLocker.wallHop);
        yield return new WaitForSeconds(0.3f);
        utils.canMoveLocker.Unlock(MoveLocker.wallHop);
        float airAccelerationAdd = (playerMove.airAccelerationMax - playerMove.airAccelerationMin) / 5;
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.1f);
            playerMove.airAcceleration += airAccelerationAdd;
        }
        playerMove.airAcceleration = playerMove.airAccelerationMax;
    }

    public void WallHop()
    {
        rb.velocity = new Vector2(wallhopXImpulse * touchingDirections.slidingWallXDirection * -1, wallhopYImpulse);
        if (touchingDirections.slidingWallXDirection * -1 > 0)
        {
            utils.IsFacingRight = true;
        }
        else
        {
            utils.IsFacingRight = false;
        }
        // lock into wall jump for a while after wall jumping to stop wall climbing
        playerMove.airAcceleration = playerMove.airAccelerationMin;
        StartCoroutine(WallHopLock());
    }
}
