using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of "IsCombat" and updates anim param of the same name
// Requires use of AnimationStrings Dic

public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    Rigidbody2D rb;
    Animator anim;

    public bool IsCombat
    {
        get { return _isCombat; }
        set
        {
            _isCombat = value;
            if (anim != null && GeneralUtils.HasParameter(AnimationStrings.IsCombat, anim))
                anim.SetBool(AnimationStrings.IsCombat, value);
        }
    }


    [SerializeField]
    private bool _isCombat = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
}
