﻿using System;
using UnityEngine;

public class PatrolState : EnemyState
{
    private Vector3[] _waypoints;
    private int _waypointIndex = 0;
    private float _chaseRange;
    private Transform _playerTransform;
    private Vector3 _lastWaypoint;

    public PatrolState(WalkingEnemy enemy, Vector3[] waypoints, Transform playerTransform,
        float chaseRange, int waypointIndex = 0) : base(enemy)
    {
        _waypoints = waypoints;  
        _playerTransform = playerTransform;
        _chaseRange = chaseRange;
        _waypointIndex = waypointIndex;
    }

    private void MoveToNextWaypoint() 
        => _enemy.transform.position = Vector3.MoveTowards(_enemy.transform.position, 
            _lastWaypoint, _enemy.MovementSpeed * Time.deltaTime);

    public override void Enter()
    {
        _lastWaypoint = _waypoints[_waypointIndex];
        MoveToNextWaypoint();
    }
    public override void Execute()
    {
        if (Vector3.Distance(_enemy.transform.position, _playerTransform.position) <= _chaseRange)
        {
            _enemy.StateMachine.ChangeState(new ChaseState(_enemy, _playerTransform, _chaseRange, 10.0f, _waypointIndex));
            return;
        }
        if (Vector3.Distance(_enemy.transform.position, _waypoints[_waypointIndex]) < 0.01f)       
            _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
        
        _enemy.transform.position = Vector3.MoveTowards(_enemy.transform.position, 
            _waypoints[_waypointIndex], _enemy.MovementSpeed * Time.deltaTime);   
    }
    public override void Exit() {}
}
