using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Entities.Creatures
{
    [BurstCompile]
    public struct CalculateDistanceToPlayer : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> creaturePositions;
        [ReadOnly] public NativeArray<Vector3> playerPosition;
        public float cullingDistance;
        public NativeArray<bool> creatureStates;

        public void Execute(int index)
        {
            float distance = Vector3.Distance(creaturePositions[index], playerPosition[0]);
            creatureStates[index] = distance < cullingDistance;
        }
    }
}