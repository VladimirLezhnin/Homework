using System.Collections.Generic;
using Game.Spawner.PlacementStrategies.Base;
using Logic.Math;
using UnityEngine;

namespace Game.Spawner.PlacementStrategies
{
    [CreateAssetMenu(fileName = "ArcPlacement", menuName = "Spawn Strategies/Arc")]
    public class ArcPlacementStrategy : CirclePlacementStrategy
    {
        [SerializeField] 
        private uint radius;
        
        [SerializeField] 
        private float stepInDegrees;

        public override IEnumerable<Vector3> GetPositions(Vector3 center, uint count) => 
            Circle.GetArcPoints(center, radius, count, stepInDegrees);
    }
}