using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.OdinInspector;
using SmbcApp.LearnGame.ConnectionManagement;
using SmbcApp.LearnGame.GamePlay.Configuration;
using SmbcApp.LearnGame.Infrastructure;
using SmbcApp.LearnGame.Utils;
using Unity.Netcode;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.GamePlayObjects.Character
{
    /// <summary>
    ///     プレイヤーごとに街の発展のために購入した街パーツの状態を管理するクラス
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class NetworkTownPartsState : NetworkBehaviour
    {
        [SerializeField] [Required] private TownPartsRegistry townPartsRegistry;

        /// <summary>
        ///     プレイヤーが購入した街パーツのリスト
        /// </summary>
        public NetworkList<TownPartData> TownPartDataList;

        public Observable<NetworkListEvent<TownPartData>> OnChanged => TownPartDataList.OnChangedAsObservable();

        public Observable<int> OnChangeCurrentPoint => TownPartDataList
            .OnChangedAsObservable()
            .Select(_ => CalcCurrentPoint())
            .DistinctUntilChanged()
            .Prepend(CalcCurrentPoint);

        public int CurrentPoint => CalcCurrentPoint();

        private void Awake()
        {
            TownPartDataList = new NetworkList<TownPartData>();
        }

        public IEnumerable<TownPartData> ToEnumerable()
        {
            foreach (var townPartData in TownPartDataList) yield return townPartData;
        }

        public TownPartData[] ToArray()
        {
            var array = new TownPartData[TownPartDataList.Count];
            for (var i = 0; i < TownPartDataList.Count; i++) array[i] = TownPartDataList[i];
            return array;
        }

        public bool TryGetPartData(NetworkGuid partDataId, out TownPartData partData, out int index)
        {
            for (var i = 0; i < TownPartDataList.Count; i++)
            {
                if (TownPartDataList[i].DataId != partDataId) continue;
                partData = TownPartDataList[i];
                index = i;
                return true;
            }

            partData = default;
            index = -1;
            return false;
        }

        public void SetTownParts(TownPartData[] data)
        {
            TownPartDataList.Clear();
            foreach (var amount in data) TownPartDataList.Add(amount);
        }

        private int CalcCurrentPoint()
        {
            return ToEnumerable()
                .Sum(d => townPartsRegistry.TownParts[d.PartId].Point);
        }
    }
}