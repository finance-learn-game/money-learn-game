using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.TownBuilding
{
    public class BuildingPlacer : MonoBehaviour
    {
        [SerializeField] private LayerMask buildingLayer;
        [SerializeField] private LayerMask groundLayer;

        private readonly List<GameObject> _locations = new();

        private void Start()
        {
            CollectLocationsCanBePlaced();
        }

        private void CollectLocationsCanBePlaced()
        {
            GameObject.FindGameObjectsWithTag("CanBePlaceBuilding", _locations);
        }

        [Button]
        private void Place(GameObject prefab)
        {
            try
            {
                if (_locations.Count == 0)
                {
                    Log.Warning("No locations available for placing buildings.");
                    return;
                }

                // Randomly select a location from the list
                var randomIndexes = Enumerable.Range(0, _locations.Count).ToArray();
                randomIndexes.Shuffle();

                var bounds = prefab.GetComponent<Renderer>().bounds;
                var selectedIndex = randomIndexes
                    .Select(i => (int?)i)
                    .FirstOrDefault(index =>
                    {
                        var location = _locations[index.Value].transform.position;
                        var corners = new Vector3[]
                        {
                            new(-bounds.extents.x, 0, -bounds.extents.z),
                            new(bounds.extents.x, 0, -bounds.extents.z),
                            new(-bounds.extents.x, 0, bounds.extents.z),
                            new(bounds.extents.x, 0, bounds.extents.z)
                        };
                        return corners.All(corner =>
                            Physics.Raycast(
                                corner + location + bounds.center,
                                Vector3.down,
                                out var hit,
                                bounds.extents.y + 1,
                                groundLayer
                            ) && _locations.Contains(hit.collider.gameObject)
                        );
                        // return !Physics.CheckBox(
                        //     location + bounds.center,
                        //     bounds.size,
                        //     Quaternion.identity,
                        //     buildingLayer
                        // );
                    });

                if (selectedIndex == null)
                {
                    Log.Warning("[Building Placer] No valid location found for placement.");
                    return;
                }

                var location = _locations[selectedIndex.Value].transform.position;
                Instantiate(prefab, location, Quaternion.identity, transform);

                var overlapColliders = new Collider[20];
                var overlapColliderCnt = Physics.OverlapBoxNonAlloc(
                    location + new Vector3(bounds.center.x, 1, bounds.center.z),
                    new Vector3(bounds.extents.x * 0.95f, 2, bounds.extents.z * 0.95f),
                    overlapColliders,
                    Quaternion.identity,
                    groundLayer
                );
                if (overlapColliderCnt <= 1)
                {
                    Log.Warning("[Building Placer] No colliders found in the area.");
                    return;
                }

                Log.Info("Cnt: {0}", overlapColliderCnt);

                foreach (var overlapCollider in overlapColliders[..overlapColliderCnt])
                    _locations.Remove(overlapCollider.gameObject);
            }
            catch (Exception e)
            {
                Log.Error($"[Building Placer] Error placing building: {e.Message}");
            }
        }
    }
}