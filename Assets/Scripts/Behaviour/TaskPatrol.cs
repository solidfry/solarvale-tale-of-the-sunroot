using System.Linq;
using Behaviour.Tree.Nodes;
using Creatures;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour
{
    public class TaskPatrol : Node
    {
        private Transform _transform;
        private Transform[] _waypoints;
        private Animator _animator;
        private NavMeshAgent _agent;
        private Creature _creature;
        
        private int _currentWaypointIndex = 0;
        
        private float _waitTime = 2f;
        private float _waitCounter = 0f;
        private bool _isWaiting = false;

        public TaskPatrol(Creature creature, NavMeshAgent agent, Transform[] waypoints)
        {
            _creature = creature;
            _agent = agent;
            _animator = agent.GetComponent<Animator>();
            _transform = creature.transform;
            _waypoints = waypoints;
        }
            
        public override NodeState Evaluate()
        {
          if (_isWaiting)
          {
              _waitCounter += Time.deltaTime;
              if (_waitCounter >= _waitTime)
              {
                  _isWaiting = false;
                  // _animator.SetBool("isWalking", true);
              }
          }
          else
          {
              if (_agent.remainingDistance <= _creature.GetStats.StoppingDistance)
              {
                  if (_waypoints.Length == 0 || _waypoints.All(x => x is null)) 
                      return NodeState.Failure;
                  _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                  _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
                  _isWaiting = true;
                  _waitCounter = 0f;
              }
              else if (_agent.pathStatus == NavMeshPathStatus.PathPartial)
              {
                  _agent.SetDestination(_waypoints[_currentWaypointIndex].position);
              }
          }
          
          State = NodeState.Running;
          return State;

        }
    }
}