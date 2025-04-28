using SmbcApp.LearnGame.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace SmbcApp.LearnGame.GamePlay.TownBuilding
{
    internal sealed class TownCameraMover : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private CinemachineCamera cam;

        private CinemachineInputAxisController _axisController;
        private GameInput _input;

        private void Start()
        {
            _input = new GameInput();
            _input.Player.Enable();

            cam.TryGetComponent(out _axisController);
        }

        private void Update()
        {
            OnMove();
            OnLook();
        }

        private void OnDestroy()
        {
            _input.Player.Disable();
        }

        private void OnLook()
        {
            _axisController.enabled = _input.Player.EnableLook.IsPressed();
        }

        private void OnMove()
        {
            var move = _input.Player.Move.ReadValue<Vector2>();
            var moveDirection = new Vector3(move.x, 0, move.y);
            moveDirection = cam.transform.TransformDirection(moveDirection);
            var targetPosition = cam.transform.position + moveDirection * (speed * Time.deltaTime);
            targetPosition.y = cam.transform.position.y; // Y軸の移動を無効にする
            cam.transform.position = targetPosition;
        }
    }
}