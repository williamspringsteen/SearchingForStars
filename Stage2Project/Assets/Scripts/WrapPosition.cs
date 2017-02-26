using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrapPosition : MonoBehaviour
{


	void Awake()
    {
		
	}
	
	void Update ()
    {
        Vector3 pivotPosition = transform.position;
        Vector3 actualPosition = transform.GetComponent<Renderer>().bounds.center;

        if (actualPosition.x < Arena.Width * -0.5f)
        {
            pivotPosition.x += Arena.Width;
        }
        else if (actualPosition.x > Arena.Width * 0.5f)
        {
            pivotPosition.x -= Arena.Width;
        }

        if (actualPosition.z < Arena.Height * -0.5f)
        {
            pivotPosition.z += Arena.Height;
        }
        else if (actualPosition.z > Arena.Height * 0.5f)
        {
            pivotPosition.z -= Arena.Height;
        }

        transform.position = pivotPosition;
    }
}
