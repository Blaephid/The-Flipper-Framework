using UnityEngine;
using System.Collections;
using System;
using System.Linq;
//using Luminosity.IO;

namespace SplineMesh
{
	[RequireComponent(typeof(Spline))]
	public class S_AI_RailEnemy : MonoBehaviour
	{

		[Header("Tools")]
		public Rigidbody rb;
		public GameObject[] models;
		public float modelDistance = 30;
		public LayerMask railMask;

		[Header("Start Rails")]
		public Spline startSpline;
		Spline RailSpline;
		public S_AddOnRail startingConnectedRails;
		S_AddOnRail ConnectedRails;

		[Header("Type")]
		public bool Rhino;
		public bool ArmouredTrain;

		[Header("Control")]
		public float StartSpeed = 60f;
		public float OffsetRail;
		public Vector3 setOffSet;
		public bool followPlayer;
		public bool backwards;

		[Header("Stats")]
		public AnimationCurve followByDistance;
		public AnimationCurve followBySpeedDif;
		public float followSpeed = 0.5f;
		public float SlopePower = 2.5f;
		public float UpHillMultiplier = 0.25f;
		public float DownHillMultiplier = 0.35f;
		public float HopDelay;
		public float hopDistance = 12;

		bool active = false;
		[HideInInspector] public S_Action05_Rail playerRail;
		[HideInInspector] public S_ActionManager _PlayerActions;
		float currentSpeed;
		float range;
		CurveSample sample;

		float CurrentDist, dist;
		float ClosestSample = 0f;
		Transform RailTransform;
		float playerDistance;
		float playerSpeed;
		Vector3 startPos;

		bool firstSet = true;
		Vector3 useOffset;


		private void Start () {
			startPos = transform.position;
			ConnectedRails = startingConnectedRails;
			RailSpline = startSpline;
			RailTransform = RailSpline.transform;
			currentSpeed = 0;

			range = GetClosestPos(transform.position, RailSpline);
			sample = RailSpline.GetSampleAtDistance(range);
			useOffset = setOffSet;

			setPos(sample, gameObject);
			alignCars();
		}


		public void InitialEvents () {
			active = true;
			if (currentSpeed == 0)
				currentSpeed = StartSpeed;

			S_Manager_LevelProgress.OnReset += EventReturnOnDeath;

		}


		private void OnDestroy () {

			//LevelProgressControl.onReset -= ReturnOnDeath;

		}

		private void OnEnable () {
		}


		private void FixedUpdate () {
			//if(active)
			//{
			//    railGrind();
			//}
		}

		void alignCars () {
			if (models.Length > 0)
			{
				float tempRange = GetClosestPos(transform.position, RailSpline);
				Spline thisSpline = RailSpline;

				if (firstSet)
				{
					float maxSpace = models.Length * modelDistance;
					tempRange = Mathf.Clamp(tempRange, maxSpace, thisSpline.Length - maxSpace);
					firstSet = false;
				}

				for (int i = 0 ; i < models.Length ; i++)
				{
					GameObject model = models[i];
					if ((RailSpline.IsLoop) && (tempRange < 0 || tempRange > thisSpline.Length))
					{
						if (!backwards)
						{
							tempRange += thisSpline.Length;
						}
						else
						{
							tempRange -= thisSpline.Length;
						}
					}
					else if (ConnectedRails != null)
					{
						if (tempRange < 0 && !backwards && ConnectedRails.PrevRail != null && ConnectedRails.PrevRail.isActiveAndEnabled)
						{
							thisSpline = ConnectedRails.PrevRail.GetComponentInParent<Spline>();
							tempRange += thisSpline.Length;
						}
						else if (tempRange > thisSpline.Length && backwards && ConnectedRails.nextRail != null && ConnectedRails.nextRail.isActiveAndEnabled)
						{
							tempRange -= thisSpline.Length;
							thisSpline = ConnectedRails.PrevRail.GetComponentInParent<Spline>();
						}
					}

					CurveSample tempSample = thisSpline.GetSampleAtDistance(tempRange);
					setPos(tempSample, model);

					if (backwards)
					{
						tempRange += modelDistance;
					}
					else
					{
						tempRange -= modelDistance;
					}
				}
			}
		}

		private void Update () {
			if (active && range < RailSpline.Length && range > 0)
			{
				railGrind();

				setPos(sample, gameObject);
				alignCars();
			}
		}

		void railGrind () {
			float ammount = (Time.deltaTime * currentSpeed);

			if (backwards)
			{
				range -= ammount;
			}
			else
				range += ammount;

			//Speed Changes
			if (followPlayer)
			{
				trackPlayer();
			}

			slopePhys();


			if (range < RailSpline.Length && range > 0)
			{
				sample = RailSpline.GetSampleAtDistance(range);

			}
			else
			{
				loseRail();
			}
		}

		void loseRail () {
			if (RailSpline.IsLoop)
			{
				if (!backwards)
				{
					range = range - RailSpline.Length;
					railGrind();
				}
				else
				{
					range = range + RailSpline.Length;
					railGrind();
				}
			}
			else if (ConnectedRails != null && ((!backwards && ConnectedRails.nextRail != null && ConnectedRails.nextRail.isActiveAndEnabled) || (backwards && ConnectedRails.PrevRail != null && ConnectedRails.PrevRail.isActiveAndEnabled)))
			{
				if (!backwards)
				{
					range = range - RailSpline.Length;
					S_AddOnRail temp = ConnectedRails;
					ConnectedRails = ConnectedRails.nextRail;
					useOffset = new Vector3(-ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

					RailSpline = ConnectedRails.GetComponentInParent<Spline>();
					RailTransform = RailSpline.transform.parent;
				}
				else
				{
					S_AddOnRail temp = ConnectedRails;
					ConnectedRails = ConnectedRails.PrevRail;
					useOffset = new Vector3(-ConnectedRails.GetComponent<S_PlaceOnSpline>().Offset3d.x, 0, 0);

					RailSpline = ConnectedRails.GetComponentInParent<Spline>();
					RailTransform = RailSpline.transform.parent;

					range = range + RailSpline.Length;
				}


				railGrind();
			}
			else if (Rhino)
			{
				rb.freezeRotation = false;
				rb.useGravity = true;
				active = false;
			}
			else if (ArmouredTrain)
			{
				currentSpeed = 0;
				active = false;
			}
		}

		void setPos ( CurveSample thisSample, GameObject thisObj ) {

			Vector3 binormal = Vector3.zero;

			if (useOffset != Vector3.zero)
			{
				binormal += sample.Rotation * -useOffset;
			}

			thisObj.transform.position = (thisSample.location + RailTransform.position + (thisSample.up * OffsetRail)) + binormal;

			if (backwards)
			{
				thisObj.transform.rotation = Quaternion.LookRotation(thisSample.tangent, thisSample.up);
				if (thisObj == gameObject)
					rb.velocity = thisSample.tangent * -currentSpeed;
			}
			else
			{
				thisObj.transform.rotation = Quaternion.LookRotation(-thisSample.tangent, thisSample.up);
				if (thisObj == gameObject)
					rb.velocity = thisSample.tangent * currentSpeed;
			}
		}

		void trackPlayer () {
			if (playerRail._Rail_int._PathSpline == RailSpline)
			{
				if (backwards == playerRail._isGoingBackwards)
				{
					if (backwards)
					{
						playerDistance = range - playerRail._pointOnSpline;

					}
					else
					{
						playerDistance = playerRail._pointOnSpline - range;
					}

					playerSpeed = (_PlayerActions._listOfSpeedOnPaths[0] - currentSpeed) / playerRail._railmaxSpeed_;
					float changeSpeed = followSpeed * followBySpeedDif.Evaluate(Mathf.Abs(playerSpeed));


					if (playerDistance > 0)
					{
						if (currentSpeed < _PlayerActions._listOfSpeedOnPaths[0] - 3)
							currentSpeed += changeSpeed;

						currentSpeed += followSpeed * followByDistance.Evaluate(Mathf.Abs(playerDistance));
					}

					else
					{

						currentSpeed = Mathf.MoveTowards(currentSpeed, _PlayerActions._listOfSpeedOnPaths[0] - 2, changeSpeed);
					}


				}
			}
		}


		void slopePhys () {
			if (Mathf.Abs(sample.up.y) < 0.1)
			{

			}

			currentSpeed = Mathf.Clamp(currentSpeed, 30, 120);
		}

		public float GetClosestPos ( Vector3 ColPos, Spline thisSpline ) {

			CurrentDist = 9999999f;
			for (float n = 0 ; n < thisSpline.Length ; n += 3)
			{
				dist = ((thisSpline.GetSampleAtDistance(n).location + thisSpline.transform.position) - ColPos).sqrMagnitude;
				if (dist < CurrentDist)
				{
					CurrentDist = dist;
					ClosestSample = n;
				}

			}
			return ClosestSample;
		}

		private void OnTriggerEnter ( Collider other ) {
			if (other.tag == "Rail")
			{
				if (other.GetComponentInParent<Spline>())
				{
					RailSpline = other.GetComponentInParent<Spline>();
					InitialEvents();
				}
			}
		}


		void EventReturnOnDeath ( object sender, EventArgs e ) {

			firstSet = true;
			gameObject.SetActive(true);
			rb.velocity = Vector3.zero;
			rb.useGravity = false;
			rb.freezeRotation = true;

			active = false;
			RailSpline = startSpline;
			RailTransform = RailSpline.transform;

			currentSpeed = 0;
			RailSpline = startSpline;
			useOffset = setOffSet;
			ConnectedRails = startingConnectedRails;

			range = GetClosestPos(startPos, RailSpline);
			sample = RailSpline.GetSampleAtDistance(range);

			setPos(sample, gameObject);
			alignCars();
		}
	}

}
