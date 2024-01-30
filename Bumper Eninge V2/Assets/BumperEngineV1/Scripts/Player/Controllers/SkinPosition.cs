using UnityEngine;
using System.Collections;

public class SkinPosition : MonoBehaviour {

    SetPosition setPosScript;
    public S_PlayerPhysics Player;

    void Awake()
    {
        setPosScript.GetComponent<SetPosition>();
    }

	void Update () {

        setPosScript.UseDynamicOffset(Player.transform.up);

	}
}
