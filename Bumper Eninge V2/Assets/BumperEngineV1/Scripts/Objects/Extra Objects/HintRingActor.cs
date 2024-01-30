using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintRingActor : MonoBehaviour
{
    S_HintBox _hintbox;
    [TextArea(3, 20)]
    public string[] hintText;
    [TextArea(3, 20)]
    public string[] hintTextGamePad;
    [TextArea(3, 20)]
    public string[] hintTextXbox;
    [TextArea(3, 20)]
    public string[] hintTextPS4;
    public float[] hintDuration;

    public float MaxDistance;
    public float MinDistance;

    [Header("Procedural Animation")]
    public Transform Mesh;
    public Transform[] OrbitDots;
    public Transform Center;
    public float DotRotationSpeed;
    public AnimationCurve RotationSpeedOverDistance; //This is to make the dots spin faster when Sonic is closer
    public float CenterAmplitude;
    public float CenterFrequency;
    Vector3 InitialPos;
    public float LookSpeed; //Speed at which it looks at the player
    Transform LookTarget;
    [HideInInspector] public AudioSource hintSound;
    // Start is called before the first frame update
    void Start()
    {
        _hintbox = S_HintBox.instance;
        InitialPos = Center.localPosition;
        LookTarget = Camera.main.transform;
        hintSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Call the procedural animation functions
        DotRotation();
        CenterMovement();
        Vector3 Direction = LookTarget.position - transform.position;
        if ((Direction.sqrMagnitude / MaxDistance) / MaxDistance <= 1f)
        {
            Vector3 LookDir = Vector3.ProjectOnPlane(Direction, Vector3.up);
            Quaternion meshRot = Quaternion.LookRotation(LookDir);
            Mesh.rotation = Quaternion.Slerp(Mesh.rotation, meshRot, Time.deltaTime * LookSpeed);
        }
    }

    void DotRotation()
    {
        if (OrbitDots.Length == 0) return;
        float DistanceThreshold = MaxDistance - MinDistance;
        float Distance = Vector3.Distance(transform.position, LookTarget.position) - MinDistance;
        float RotSpeed = DotRotationSpeed * RotationSpeedOverDistance.Evaluate(Distance / DistanceThreshold);
        for (int i = 0; i < OrbitDots.Length; i++)
        {
            if (OrbitDots[i] == null) break;
            Vector3 Rotation = OrbitDots[i].localEulerAngles;
            Rotation.z += RotSpeed * Time.deltaTime;
            OrbitDots[i].localRotation = Quaternion.Euler(Rotation);
        }
    }

    void CenterMovement()
    {
        if (Center == null) return;
        Vector3 Bob = InitialPos;
        Bob.y += Mathf.Sin(Time.fixedTime * Mathf.PI * CenterFrequency) * CenterAmplitude;
        Center.localPosition = Bob;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (!_hintbox.IsShowing)
    //        {
    //            _hintbox.ShowHint(hintText, hintDuration);
    //            hintSound.Play();
    //        }
    //    }
    //}
}
