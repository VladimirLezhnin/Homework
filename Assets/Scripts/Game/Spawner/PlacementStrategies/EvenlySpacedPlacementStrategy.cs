using System.Collections.Generic;
using Game.Spawner.PlacementStrategies.Base;
using Logic.Math;
using UnityEngine;

namespace Game.Spawner.PlacementStrategies
{
    [CreateAssetMenu(fileName = "EvenlySpacedPlacement", menuName = "Spawn Strategies/EvenlySpaced")]
    public class EvenlySpacedPlacementStrategy : CirclePlacementStrategy
    {
        [SerializeField] 
        private uint radius;

        public override IEnumerable<Vector3> GetPositions(Vector3 center, uint count) =>
            Circle.GetEvenlySpacedPoints(center, radius, count);
    }
}