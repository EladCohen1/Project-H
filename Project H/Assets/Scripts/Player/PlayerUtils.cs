using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Relies on a Rigidbody2D and the MutliLock class
// Offers utils for a playerController
// playerDirectionAsNumber will give 1 or -1 depending on the direction faced
// canMoveLocker is a multiLocker that is meant to list lockers for player movement
// _canMove tracks if any lockers for player movement currently exist and returns true if none exist
// Edit enum MoveLocker to add or remove types of movement lockers

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerUtils : MonoBehaviour
{
    [Header("Components")]
    Rigidbody2D rb;

    public float playerdirectionAsNumber => rb.transform.localScale.x > 0 ? 1 : -1;
    public MultiLock<MoveLocker> canMoveLocker = new MultiLock<MoveLocker>();
    public bool _canMove => canMoveLocker.IsFree();


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
        rb = GetComponent<Rigidbody2D>();
    }
}

public enum MoveLocker
{
    WallSlide,
    wallHop
}