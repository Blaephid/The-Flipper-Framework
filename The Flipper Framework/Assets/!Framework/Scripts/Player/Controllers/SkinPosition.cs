using UnityEngine;
using System.Collections;

public class SkinPosition : MonoBehaviour {

    S_SetPosition setPosScript;
    public S_PlayerPhysics Player;

    void Awake()
    {
        setPosScript.GetComponent<S_SetPosition>();
    }

	void Update () {

        setPosScript.UseDynamicOffset(Player.transform.up);

	}
}
