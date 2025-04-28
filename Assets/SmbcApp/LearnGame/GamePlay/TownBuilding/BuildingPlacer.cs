using Sirenix.OdinInspector;
using SmbcApp.LearnGame.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SmbcApp.LearnGame.GamePlay.TownBuilding
{
    /// <summary>
    ///     グリッド上に建物を配置するためのスクリプト
    /// </summary>
    internal class BuildingPlacer : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask ablePlaceLayer;

        private Camera _camera;
        private BoxCollider _holdInstance;
        private GameInput _input;
        private bool _isHolding;

        private void Start()
        {
            _camera = Camera.main;

            _input = new GameInput();
            _input.Player.Enable();
            _input.Player.Place.performed += OnPlace;
        }

        private void Update()
        {
            if (_isHolding) OnHolding();
        }

        private void OnDestroy()
        {
            _input.Player.Place.performed -= OnPlace;
            _input.Player.Disable();
        }

        [Button]
        private void Putting(GameObject prefab)
        {
            Instantiate(prefab).TryGetComponent(out _holdInstance);
            _holdInstance.enabled = false;
            _isHolding = true;
        }

        private void OnHolding()
        {
            var mousePos = Mouse.current.position.ReadValue();
            var ray = _camera.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out var hit, maxDistance, ablePlaceLayer))
            {
                _holdInstance.gameObject.SetActive(true);

                // 重なるオブジェクトがあるか確認
                var overlaps = Physics.CheckBox(_holdInstance.center + hit.collider.transform.position,
                    _holdInstance.size / 2, Quaternion.identity, ~ablePlaceLayer);
                if (!overlaps) return;

                // 建物の四隅に設置可能オブジェクトがあるか確認
                // var corners = new Vector3[4];
                // var size = _holdInstance.size;
                // corners[0] = _holdInstance.center + new Vector3(-size.x / 2, 0, -size.z / 2);
                // corners[1] = _holdInstance.center + new Vector3(size.x / 2, 0, -size.z / 2);
                // corners[2] = _holdInstance.center + new Vector3(-size.x / 2, 0, size.z / 2);
                // corners[3] = _holdInstance.center + new Vector3(size.x / 2, 0, size.z / 2);
                // if (corners
                //     .Select(corner => hit.collider.transform.position + corner)
                //     .Any(cornerPos => !Physics.Linecast(cornerPos, Vector3.down * size.y, ablePlaceLayer))
                //    ) return;

                _holdInstance.transform.position = hit.collider.transform.position;
            }
            else
            {
                _holdInstance.gameObject.SetActive(false);
            }
        }

        private void OnPlace(InputAction.CallbackContext ctx)
        {
            if (!_isHolding || !_holdInstance.gameObject.activeSelf) return;

            _isHolding = false;
            _holdInstance.enabled = true;
            _holdInstance = null;
        }
    }
}