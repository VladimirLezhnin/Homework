using System.Collections.Generic;
using Game.Spawner.PlacementStrategies.Base;
using UnityEngine;

namespace Game.Spawner
{
    public class SingleObjectSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject objectToSpawn;

        [SerializeField]
        private uint count;

        [SerializeField]
        private CirclePlacementStrategy placementStrategy;
        
        private readonly List<Transform> spawnedTransforms = new();
        private CirclePlacementStrategy activeStrategy;

        private void Awake()
        {
            if (objectToSpawn == null || placementStrategy == null)
            {
                Debug.LogError("Assign objectToSpawn and placementStrategy before starting.", this);
                enabled = false;
                return;
            }
            
            foreach (var position in placementStrategy.GetLocalPositions(count))
            {
                var instance = Instantiate(objectToSpawn, transform, false);
                instance.transform.localPosition = position;
                spawnedTransforms.Add(instance.transform);
            }
        }

        private void OnEnable() =>  RefreshPlacement();

        private void OnDisable() => SetActiveStrategy(null);

        private void OnValidate()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
                return;

            RefreshPlacement();
        }

        private void RefreshPlacement()
        {
            SetActiveStrategy(placementStrategy != null ? placementStrategy : null);
            UpdatePositions();
        }
        
        private void SetActiveStrategy(CirclePlacementStrategy newStrategy)
        {
            if (ReferenceEquals(activeStrategy, newStrategy))
                return;

            if (activeStrategy != null)
                activeStrategy.Updated -= UpdatePositions;

            activeStrategy = newStrategy;

            if (activeStrategy != null)
                activeStrategy.Updated += UpdatePositions;
        }

        private void UpdatePositions()
        {
            if (activeStrategy == null || spawnedTransforms.Count == 0)
                return;

            using var enumerator = activeStrategy
                .GetLocalPositions((uint)spawnedTransforms.Count)
                .GetEnumerator();

            foreach (var spawnedTransform in spawnedTransforms)
            {
                if (!enumerator.MoveNext())
                    break;

                if (spawnedTransform != null)
                    spawnedTransform.localPosition = enumerator.Current;
            }
        }
    }
}
