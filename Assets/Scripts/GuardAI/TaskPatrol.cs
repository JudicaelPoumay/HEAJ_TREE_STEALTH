using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskPatrol : BehaviorTree.Node
{
	private Transform _transform;
	private Animator _animator;
	private Vector3[] _waypoints;
	private int _currentWayPointIdx = 0;

	private float _waitTime = 1f;
	private float _waitCounter = 0f;
	private bool _waiting = false;
	private UnityEngine.AI.NavMeshAgent _navAgent;

	public TaskPatrol(Transform transform, Vector3[] waypoints, UnityEngine.AI.NavMeshAgent navAgent)
	{
		_transform = transform;
		_animator = transform.GetComponent<Animator>();
		_waypoints = waypoints;
		_navAgent = navAgent;
	}

	public override BehaviorTree.NodeState Evaluate()
	{
		if(_currentWayPointIdx == _waypoints.Length)
			return BehaviorTree.NodeState.SUCCESS;
		if(_waiting)
		{
			_waitCounter += Time.deltaTime;
			if(_waitCounter >= _waitTime)
			{
				_waiting = false;
				_animator.SetBool("Walking",true);
			}
		}
		else
		{
			Vector3 wp = _waypoints[_currentWayPointIdx];
			if(Vector3.Distance(_transform.position, wp) < 1f)
			{
				_waitCounter = 0f;
				_waiting = true;
				_currentWayPointIdx = _currentWayPointIdx+1;

				_animator.SetBool("Walking",false);
			}
			else
			{
				_navAgent.destination = wp;
				TurnToFace(wp);
			}
		}

		

		return BehaviorTree.NodeState.RUNNING;
	}



	// turning towards way point before moving to the next one
	public void TurnToFace(Vector3 lookTarget) {
		Vector3 dirToLookTarget = (lookTarget - _transform.position).normalized;
		float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;
		//rotate to target over time
		//while loop will stop running once the guard is facing the look target
		while (Mathf.Abs(Mathf.DeltaAngle(_transform.eulerAngles.y, targetAngle)) > 0.05f) {
			float angle = Mathf.MoveTowardsAngle(_transform.eulerAngles.y, targetAngle, 140 * Time.deltaTime);
			_transform.eulerAngles = Vector3.up * angle;
			return;
		}

	}

	public override void Reset()
	{
		_currentWayPointIdx = 0;
	}
}
