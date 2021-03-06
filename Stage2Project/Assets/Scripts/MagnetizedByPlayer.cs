﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagnetizedByPlayer : MonoBehaviour
{
    public enum Type { Attract, Repel }

    [SerializeField]
    private float RepelForce = 400.0f;

    [SerializeField]
    private float MinimumDistance = 1.0f;

    [SerializeField]
    private Type MagnetizeType = Type.Repel;

    [SerializeField]
    private float MassRepelForce = 550.0f;

    private float mMassRepelDistance = 3000.0f;

    private Player mPlayer;
    private Rigidbody mBody;
    private Type mInitialMagnetizeType;
    private float mInitialRepelForce;
    private float mInitialMinimumDistance;

    public Type ForceType { get { return MagnetizeType; } }

    void Awake()
    {
        mPlayer = FindObjectOfType<Player>();
        mBody = GetComponent<Rigidbody>();
        mInitialMagnetizeType = MagnetizeType;
        mInitialRepelForce = RepelForce;
        mInitialMinimumDistance = MinimumDistance;
    }

	void Update()
    {
        if( mPlayer != null)
        {
            Vector3 difference = MagnetizeType == Type.Repel ? transform.position - mPlayer.GetCenter() : mPlayer.GetCenter() - transform.position;
            if ( difference.magnitude <= MinimumDistance )
            {
                mBody.AddForce(difference * RepelForce * Time.deltaTime);
            }
        }		
	}

    internal void FlipForce()
    {
        if (MagnetizeType == Type.Repel)
        {
            MagnetizeType = Type.Attract;
        }
        else
        {
            MagnetizeType = Type.Repel;
        }
    }

    internal void MakeRepelling(bool zero)
    {
        if (zero)
        {
            mBody.velocity = Vector3.zero;
        }
        MagnetizeType = Type.Repel;
    }

    internal void RevertMagnetizeType()
    {
        MagnetizeType = mInitialMagnetizeType;
    }

    internal void SetMassRepelForce()
    {
        RepelForce = MassRepelForce;
    }

    internal void RevertMassRepelForce()
    {
        RepelForce = mInitialRepelForce;
    }

    internal void SetMassRepelDistance()
    {
        MinimumDistance = mMassRepelDistance;
    }

    internal void RevertMassRepelDistance()
    {
        MinimumDistance = mInitialMinimumDistance;
    }
}
