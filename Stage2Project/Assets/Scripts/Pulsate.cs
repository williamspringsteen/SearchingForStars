using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pulsate : MonoBehaviour
{

    [SerializeField]
    private float GrowthRate = 0.5f;

    [SerializeField]
    private float MaxGrowth = 10.0f;

    private Rigidbody mBody;
    private float mInitialXSize;
    private float mMaxXSize;
    private bool mGrowing;

    void Awake()
    {
        mBody = GetComponent<Rigidbody>();
        mInitialXSize = transform.localScale.x;
        mMaxXSize = mInitialXSize * MaxGrowth;
        mGrowing = true;
    }

    void Update()
    {
		if (transform.localScale.x >= mMaxXSize)
        {
            mGrowing = false;
        }
        else if (transform.localScale.x <= mInitialXSize)
        {
            mGrowing = true;
        }

        //float timeScale = Time.timeScale;
        float timeScale = Time.deltaTime;

		if (mGrowing)
        {
            transform.localScale += new Vector3(timeScale * GrowthRate, 0.0f, timeScale * GrowthRate);
        }
        else
        {
            transform.localScale -= new Vector3(timeScale * GrowthRate, 0.0f, timeScale * GrowthRate);
        }
    }

}
