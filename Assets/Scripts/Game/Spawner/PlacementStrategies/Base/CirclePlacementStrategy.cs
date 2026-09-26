using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Spawner.PlacementStrategies.Base
{
    public abstract class CirclePlacementStrategy : ScriptableObject
    {
        public event Action Updated;
        
        protected virtual void OnValidate() => Updated?.Invoke();
        
        public abstract IEnumerable<Vector3> GetPositions(Vector3 center, uint count);
        
        public IEnumerable<Vector3> GetLocalPositions(uint count) => GetPositions(Vector3.zero, count);
    }
}