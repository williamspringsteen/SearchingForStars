using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagnetizedByPlayer : MonoBehaviour
{
    public enum Type { Attract, Repel }

    [SerializeField]
    private float RepelForce = 1000.0f;

    [SerializeField]
    private float MinimumDistance = 1.0f;

    [SerializeField]
    private Type MagnetizeType = Type.Repel;

    private Player mPlayer;
    private Rigidbody mBody;

    public Type ForceType { get { return MagnetizeType; } }

    void Awake()
    {
        mPlayer = FindObjectOfType<Player>();
        mBody = GetComponent<Rigidbody>();
    }

	void Update()
    {
        if( mPlayer != null)
        {
            Vector3 difference = MagnetizeType == Type.Repel ? transform.position - mPlayer.transform.position : mPlayer.transform.position - transform.position;
            if( difference.magnitude <= MinimumDistance )
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
}
