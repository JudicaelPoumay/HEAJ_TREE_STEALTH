using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Node = BehaviorTree.Node;
using Tree = BehaviorTree.Tree;
using Sequence = BehaviorTree.Sequence;

public class Guard : Tree {

	public static event System.Action OnGuardHasSpottedPlayer;
	public float awarenessTimer = .5f;

	public Light spotlight; //Guards spot light, and vision cone
	public float viewDistance;
	public LayerMask viewMask;

	private NavMeshAgent navAgent;

	float viewAngle;
	float playerVisibleTimer;

	public Transform pathHolder;
	Transform player;
	Color MainSpotLightColor;

	void Start() {
		base.Start();

		//finds the player tag
		player = GameObject.FindGameObjectWithTag ("Player").transform;
		viewAngle = spotlight.spotAngle;
		MainSpotLightColor = spotlight.color;
	}
	
	void Awake(){
		navAgent = GetComponent<NavMeshAgent>();
	}

	void Update (){
		base.Update();
		
		if (CanSeePlayer()) {
		//counts up or down the visible timer if player is seen or not
			playerVisibleTimer += Time.deltaTime;
		} else {
			playerVisibleTimer -= Time.deltaTime;

		}
		//clap timer
		playerVisibleTimer = Mathf.Clamp(playerVisibleTimer,0, awarenessTimer);
		//the colour of the spotlight depends on the awareness of the guard with the 2 functions
		spotlight.color = Color.Lerp(MainSpotLightColor,Color.red, playerVisibleTimer / awarenessTimer);

		if (playerVisibleTimer >= awarenessTimer) {
			//guard has spotted players
			if (OnGuardHasSpottedPlayer != null){
				OnGuardHasSpottedPlayer ();
			}

		}
	}

	protected override Node SetupTree()
	{
		Vector3[] waypoints = new Vector3[pathHolder.childCount];
		for (int i = 0; i < waypoints.Length; i++) {
				waypoints [i] = pathHolder.GetChild (i).position;
				waypoints [i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
		}
		
		Debug.Log(navAgent);
		Node root = new Sequence(new List<Node>
		{
			new TaskPatrol(transform, waypoints, navAgent)
		});

		return root;
	}

	bool CanSeePlayer(){
		//checks if the distance between player and guard position is less than view distance
		if (Vector3.Distance(transform.position,player.position) < viewDistance) {
				//checks the angles
				Vector3 dirToPlayer = (player.position - transform.position).normalized;
				float angleBetweenGuardAndPlayer = Vector3.Angle (transform.forward, dirToPlayer);
				if (angleBetweenGuardAndPlayer < viewAngle /2f) {
					//line of sight blocking check
					if (!Physics.Linecast(transform.position, player.position,viewMask)) {
						return true;
					}
				}
		}
		return false;
	}

	//Looping through the path Game object, adding a Line and Texture to each Child object so it is visible in the Scene
	void OnDrawGizmos() {
		Vector3 startPosition = pathHolder.GetChild(0).position;
		Vector3 previousPosition = startPosition;

		foreach (Transform waypoint in pathHolder) {
			Gizmos.DrawSphere (waypoint.position, .3f);
			Gizmos.DrawLine (previousPosition, waypoint.position);
			previousPosition = waypoint.position;
		}
			Gizmos.DrawLine(previousPosition, startPosition);
			//guard vision cone drawn in game
			Gizmos.color = Color.red;
			Gizmos.DrawRay(transform.position,transform.forward * viewDistance);

	}
}
