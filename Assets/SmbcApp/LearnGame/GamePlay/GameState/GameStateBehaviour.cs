using UnityEngine;
using VContainer.Unity;

namespace SmbcApp.LearnGame.Gameplay.GameState
{
    internal enum GameState
    {
        MainMenu,
        Lobby,
        InGame,
        EndGame
    }

    internal abstract class GameStateBehaviour : LifetimeScope
    {
        public static GameObject ActiveStateGo;

        /// <summary>
        ///     複数のシーンにまたがるゲームの状態
        /// </summary>
        public virtual bool Persists => false;

        public abstract GameState ActiveState { get; }

        protected override void Awake()
        {
            base.Awake();

            if (Parent != null) Parent.Container.Inject(this);
        }

        private void Start()
        {
            if (ActiveStateGo != null)
            {
                if (ActiveStateGo == gameObject) return;

                ActiveStateGo.TryGetComponent(out GameStateBehaviour previousState);
                if (previousState.Persists && previousState.ActiveState == ActiveState)
                {
                    Destroy(gameObject);
                    return;
                }

                Destroy(ActiveStateGo);
            }

            ActiveStateGo = gameObject;
            if (Persists) DontDestroyOnLoad(gameObject);
        }

        protected override void OnDestroy()
        {
            if (!Persists) ActiveStateGo = null;
        }
    }
}