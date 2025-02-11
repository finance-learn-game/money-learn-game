using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using SmbcApp.LearnGame.Data;
using Unity.Logging;
using VContainer;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Utils
{
    public sealed class ProfileManager : IAsyncStartable, IDisposable
    {
        private readonly ObservableList<string> _availableProfiles = new();
        private readonly ReactiveProperty<string> _currentProfile = new();
        private readonly SaveDataService _saveDataService;
        private DisposableBag _disposableBag;

        [Inject]
        public ProfileManager(SaveDataService saveDataService)
        {
            _saveDataService = saveDataService;
            _disposableBag = new DisposableBag();
        }

        public ReadOnlyReactiveProperty<string> CurrentProfile => _currentProfile;
        public IReadOnlyObservableList<string> AvailableProfiles => _availableProfiles;
        public bool IsInitialized { get; private set; }


        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await UniTask.WaitUntil(() => _saveDataService.Initialized, cancellationToken: cancellation);

            _availableProfiles.AddRange(_saveDataService.SaveData.CurrentValue.Profiles);
            IsInitialized = true;

            _currentProfile.Value = _saveDataService.SaveData.CurrentValue.CurrentProfile;
            CurrentProfile
                .Subscribe(profile => _saveDataService.ChangeSaveData(data => data with { CurrentProfile = profile }))
                .AddTo(ref _disposableBag);
        }

        public void Dispose()
        {
            CurrentProfile?.Dispose();
            _disposableBag.Dispose();
        }

        public void CreateProfile(string profileName)
        {
            if (_availableProfiles.Contains(profileName))
            {
                Log.Warning("ProfileManager Profile {0} already exists", profileName);
                return;
            }

            _availableProfiles.Add(profileName);
            ApplyProfilesToSaveData();
        }

        public void DeleteProfile(string profileName)
        {
            if (!_availableProfiles.Remove(profileName)) return;

            if (_currentProfile.Value == profileName) _currentProfile.Value = null;
            ApplyProfilesToSaveData();
        }

        private void ApplyProfilesToSaveData()
        {
            _saveDataService.ChangeSaveData(data => data with { Profiles = _availableProfiles.ToList() });
        }
    }
}