using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Utils;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SmbcApp.LearnGame.GamePlay.TownBuilding
{
    /// <summary>
    ///     リザルトページの街を構築するためのコンポーネント
    /// </summary>
    public class ResultTownBuilder : MonoBehaviour
    {
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;

        private readonly Dictionary<int, AsyncObjectPool<GameObject>> _buildingsPool = new();
        private readonly Stack<(int PartId, GameObject Building)> _currentBuildings = new();

        private void OnDestroy()
        {
            foreach (var (_, pool) in _buildingsPool) pool.Dispose();
            _buildingsPool.Clear();
        }

        private UniTask<GameObject> RentBuilding(int partId)
        {
            if (_buildingsPool.TryGetValue(partId, out var pool)) return pool.Rent();

            pool = new AsyncObjectPool<GameObject>(
                () => townPartsRegistry.TownParts[partId].Prefab.InstantiateAsync(transform).ToUniTask(),
                obj => obj.SetActive(true),
                obj => obj.SetActive(false),
                obj => Addressables.ReleaseInstance(obj)
            );
            _buildingsPool.Add(partId, pool);

            return pool.Rent();
        }

        private void ReturnBuilding(int partId, GameObject building)
        {
            if (_buildingsPool.TryGetValue(partId, out var pool))
                pool.Return(building);
            else
                Log.Error("Building pool not found for partId: {0}", partId);
        }

        public UniTask BuildTown(IEnumerable<TownPartData> townPartDataList)
        {
            while (_currentBuildings.TryPop(out var tuple))
                ReturnBuilding(tuple.PartId, tuple.Building);

            return UniTask.WhenAll(townPartDataList
                .Where(data => data.IsPlaced)
                .Select(async data =>
                {
                    var building = await RentBuilding(data.PartId);
                    building.transform.position = data.Position;
                    _currentBuildings.Push((data.PartId, building));
                })
            );
        }
    }
}