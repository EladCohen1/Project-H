using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CapsuleCollider2D))]
public class TouchingDirections : MonoBehaviour
{
    public ContactFilter2D castFilter;
    public ContactFilter2D SlideableWallsLayerCastFilter;
    public LayerMask groundLayerMask;
    public float groundDistance = 0.05f;
    public float wallDistance = 0.4f;
    public float cellingDistance = 0.05f;

    private CapsuleCollider2D touchingCol;
    private RaycastHit2D[] groundHits = new RaycastHit2D[5];
    private RaycastHit2D[] wallHits = new RaycastHit2D[5];
    private RaycastHit2D[] slidableWallHits = new RaycastHit2D[5];
    private RaycastHit2D[] cellingHits = new RaycastHit2D[5];

    public bool IsGrounded
    {
        get { return _isGrounded; }
        private set
        {
            _isGrounded = value;
            if (anim != null)
            {
                anim.SetBool(AnimationStrings.IsGrounded, value);
            }
        }
    }

    public bool IsOnWall
    {
        get { return _isOnWall; }
        private set
        {
            _isOnWall = value;
            if (anim != null)
            {
                anim.SetBool(AnimationStrings.IsOnWall, value);
            }
        }
    }

    public bool IsOnWallFromBehind
    {
        get { return _isOnWallFromBehind; }
        private set
        {
            _isOnWallFromBehind = value;
            if (anim != null)
            {
                anim.SetBool(AnimationStrings.IsOnWallFromBehind, value);
            }
        }
    }

    public bool IsOnCelling
    {
        get { return _isOnCelling; }
        private set
        {
            _isOnCelling = value;
            if (anim != null)
            {
                anim.SetBool(AnimationStrings.IsOnCelling, value);
            }
        }
    }

    public bool IsOnSlidableWall
    {
        get { return _isOnSlidableWall; }
        private set
        {
            _isOnSlidableWall = value;
        }
    }

    public float WallXDirection;

    [SerializeField]
    private bool _isGrounded = true;
    [SerializeField]
    private bool _isOnWall = false;
    [SerializeField]
    private bool _isOnWallFromBehind = false;
    [SerializeField]
    private bool _isOnCelling = false;
    [SerializeField]
    private bool _isOnSlidableWall = false;

    // Components
    Animator anim;
    private Vector2 forwardDirection => transform.localScale.x < 0 ? Vector2.left : Vector2.right;

    void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        IsGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        IsOnWall = touchingCol.Cast(forwardDirection, castFilter, wallHits, wallDistance) > 0;
        IsOnWallFromBehind = touchingCol.Cast(forwardDirection * new Vector2(-1, 1), castFilter, wallHits, wallDistance) > 0;
        IsOnCelling = touchingCol.Cast(Vector2.up, castFilter, cellingHits, cellingDistance) > 0;
        IsOnSlidableWall = touchingCol.Cast(forwardDirection, SlideableWallsLayerCastFilter, slidableWallHits, wallDistance) > 0;
        if (IsOnWall)
        {
            WallXDirection = forwardDirection.x;
        }
        if (IsOnWallFromBehind)
        {
            WallXDirection = forwardDirection.x * -1;
        }
    }
}
