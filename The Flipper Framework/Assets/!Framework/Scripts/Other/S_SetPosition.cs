using UnityEngine;
using System.Collections;

public class S_SetPosition : MonoBehaviour {

    public Transform TargetPosition;
	public Rigidbody _RB;
    public Vector3 Offset;

    Vector3 DynamicOffset;
    bool Dynamic;

	void Update () {

        if (!Dynamic)
        {
			//transform.position = _RB.position + Offset;
            transform.position = TargetPosition.position + Offset;
        }
        else
        {
			//transform.position = _RB.position + DynamicOffset;
			transform.position = TargetPosition.position + DynamicOffset;
			
		}

	}

    public void UseDynamicOffset(Vector3 offset)
    {
        Dynamic = true;
        DynamicOffset = (Vector3.Scale(Offset, offset));
    }
}
