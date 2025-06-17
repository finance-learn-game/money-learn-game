using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using SmbcApp.LearnGame.Infrastructure;
using Unity.Logging;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace SmbcApp.LearnGame.GamePlay.TownBuilding
{
    internal sealed class GridBuildingPlacer : MonoBehaviour
    {
        private readonly Queue<BuildingData> _buildingsQueue = new();
        private bool[][] _grid;
        private Vector3 _gridOffset;
        private Bounds _tileBounds;

        private void Start()
        {
            BuildGrid(out _grid, out _tileBounds, out _gridOffset);
        }

        private void OnDrawGizmosSelected()
        {
            if (_grid == null) return;

            Gizmos.color = Color.red;
            for (var i = 0; i < _grid.Length; i++)
            for (var j = 0; j < _grid[i].Length; j++)
                if (!_grid[i][j])
                    Gizmos.DrawWireCube(
                        new Vector3(
                            j * _tileBounds.size.x - _tileBounds.extents.x,
                            1,
                            i * _tileBounds.size.z - _tileBounds.extents.z
                        ) + _gridOffset,
                        _tileBounds.size
                    );
        }

        public async UniTask<Vector3?> Place(
            AssetReferenceGameObject prefab,
            NetworkGuid townPartDataId,
            Action<NetworkGuid> onRemoveBuilding
        )
        {
            if (_grid == null)
            {
                Log.Error("[GridBuildingPlacer] Grid is not initialized.");
                return null;
            }

            // プレハブをロード
            var buildingHandle = prefab.InstantiateAsync();
            if (!(await buildingHandle).TryGetComponent(out BoxCollider buildingCollider))
            {
                Log.Error("[GridBuildingPlacer] Prefab does not have a Renderer component.");
                buildingHandle.Release();
                return null;
            }

            while (true)
            {
                var gridSize = CalcBuildingGridSize(buildingCollider);
                var groups = CalcGroup(gridSize)
                    .GroupBy(group => CalcGroupScore(group, gridSize));

                // スコアが最小のグループを取得
                var minScoreGroup = groups.FirstOrDefault()?.ToArray();
                // スコアが最小のグループがない場合は、既存の建物を削除して再試行
                if (minScoreGroup == null)
                {
                    Log.Info("[GridBuildingPlacer] No valid position found for the building.");
                    if (!_buildingsQueue.TryDequeue(out var queuedData))
                    {
                        // キューが空の場合は終了
                        Log.Info("[GridBuildingPlacer] No queued buildings to destroy.");
                        buildingHandle.Release(); // プレハブのハンドルを解放
                        return null;
                    }

                    // 既存の建物を削除
                    queuedData.Release();
                    onRemoveBuilding(queuedData.TownPartDataId);

                    for (var i = 0; i < queuedData.Size.y; i++)
                    for (var j = 0; j < queuedData.Size.x; j++)
                        _grid[queuedData.Pos.y + i][queuedData.Pos.x + j] = true;
                    Log.Info("[GridBuildingPlacer] Destroyed queued object.");
                    continue;
                }

                var selectedGroup = minScoreGroup[Random.Range(0, minScoreGroup.Length)];
                var pos = new Vector3((selectedGroup.x + gridSize.x - 1) * _tileBounds.size.x, 0,
                    (selectedGroup.y + gridSize.y - 1) * _tileBounds.size.x);

                buildingCollider.transform.SetParent(transform);
                buildingCollider.transform.position = pos + _gridOffset;

                _buildingsQueue.Enqueue(new BuildingData(buildingHandle, selectedGroup, gridSize, townPartDataId));

                for (var i = 0; i < gridSize.y; i++)
                for (var j = 0; j < gridSize.x; j++)
                    _grid[selectedGroup.y + i][selectedGroup.x + j] = false;
                return pos + _gridOffset;
            }
        }

        private int2 CalcBuildingGridSize(BoxCollider buildingCollider)
        {
            var xSize = (int)(buildingCollider.bounds.size.x / _tileBounds.size.x);
            var zSize = (int)(buildingCollider.bounds.size.z / _tileBounds.size.z);
            return new int2(xSize, zSize);
        }

        private IEnumerable<int2> CalcGroup(int2 size)
        {
            for (var i = 0; i < _grid.Length - size.y; i++)
            for (var j = 0; j < _grid[i].Length - size.x; j++)
                if (IsValidPosition(j, i))
                    yield return new int2(j, i);
            yield break;

            bool IsValidPosition(int x, int y)
            {
                for (var i = 0; i < size.y; i++)
                for (var j = 0; j < size.x; j++)
                    if (!_grid[y + i][x + j])
                        return false;

                return true;
            }
        }

        private int CalcGroupScore(int2 pos, int2 size)
        {
            // 側面が空いている数をカウント
            var score = 0;
            for (var i = 0; i < size.y; i++)
            {
                if (pos.x > 0 && _grid[pos.y + i][pos.x - 1]) score++;
                if (pos.x + size.x < _grid[0].Length && _grid[pos.y + i][pos.x + size.x]) score++;
            }

            for (var i = 0; i < size.x; i++)
            {
                if (pos.y > 0 && _grid[pos.y - 1][pos.x + i]) score++;
                if (pos.y + size.y < _grid.Length && _grid[pos.y + size.y][pos.x + i]) score++;
            }

            return score;
        }

        private static void BuildGrid(out bool[][] grid, out Bounds tileBounds, out Vector3 gridOffset)
        {
            var tiles = new List<GameObject>();
            GameObject.FindGameObjectsWithTag("CanBePlaceBuilding", tiles);
            tiles.First().TryGetComponent(out Collider tileCollider);
            tileBounds = tileCollider.bounds;

            var maxPos = new Vector3(
                tiles.Max(v => v.transform.position.x) + tileBounds.size.x,
                0,
                tiles.Max(v => v.transform.position.z) + tileBounds.size.z
            );
            var minPos = new Vector3(
                tiles.Min(v => v.transform.position.x),
                0,
                tiles.Min(v => v.transform.position.z)
            );

            gridOffset = minPos;

            var xSize = (int)((maxPos.x - minPos.x) / tileBounds.size.x);
            var zSize = (int)((maxPos.z - minPos.z) / tileBounds.size.z);
            grid = new bool[zSize][];
            for (var i = 0; i < grid.Length; i++) grid[i] = new bool[xSize];

            foreach (var pos in tiles.Select(tile => tile.transform.position))
            {
                var x = (int)((pos.x - minPos.x) / tileBounds.size.x);
                var z = (int)((pos.z - minPos.z) / tileBounds.size.z);
                grid[z][x] = true;
            }
        }

        private readonly struct BuildingData
        {
            private readonly AsyncOperationHandle<GameObject> _handle;
            public readonly int2 Pos;
            public readonly int2 Size;
            public readonly NetworkGuid TownPartDataId;

            public BuildingData(AsyncOperationHandle<GameObject> handle, int2 pos, int2 size,
                NetworkGuid townPartDataId)
            {
                _handle = handle;
                Pos = pos;
                Size = size;
                TownPartDataId = townPartDataId;
            }

            public void Release()
            {
                var handle = _handle;
                if (handle.IsValid())
                    Addressables.Release(_handle);
                else
                    Log.Error("[GridBuildingPlacer] Handle is not valid.");
            }
        }
    }
}