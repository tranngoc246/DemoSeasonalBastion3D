using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class StrategyCameraController3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;

        [Header("View")]
        [SerializeField] private float _pitch = 55f;
        [SerializeField] private float _yaw = 45f;
        [SerializeField] private float _distance = 36f;
        [SerializeField] private float _minDistance = 12f;
        [SerializeField] private float _maxDistance = 80f;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 30f;
        [SerializeField] private float _fastMoveMultiplier = 2f;
        [SerializeField] private float _edgePanSize = 16f;
        [SerializeField] private bool _enableEdgePan = true;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 120f;
        [SerializeField] private float _zoomSmooth = 12f;

        [Header("Bounds")]
        [SerializeField] private float _boundsPadding = 4f;
        [SerializeField] private bool _snapToTerrainCenterOnStart = true;

        private Vector3 _focusPoint;
        private float _targetDistance;
        private bool _initialized;

        private void Awake()
        {
            ResolveRefs();
            _targetDistance = _distance;
        }

        private void Start()
        {
            ResolveRefs();
            InitializeFromRuntime();
            ApplyCamera(true);
        }

        private void Update()
        {
            ResolveRefs();
            if (!_initialized)
                InitializeFromRuntime();

            HandleMovement();
            HandleZoom();
            ClampFocusPoint();
            ApplyCamera(false);
        }

        private void ResolveRefs()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
            if (_runtimeHost == null)
                _runtimeHost = FindObjectOfType<TerrainGameplayRuntimeHost>();
        }

        private void InitializeFromRuntime()
        {
            if (_runtimeHost == null)
                return;
            if (_runtimeHost.GridMap == null || _runtimeHost.Mapper == null)
                _runtimeHost.Initialize();
            if (_runtimeHost.GridMap == null || _runtimeHost.Mapper == null)
                return;

            if (_snapToTerrainCenterOnStart)
            {
                int centerX = Mathf.Max(0, _runtimeHost.GridMap.Width / 2);
                int centerY = Mathf.Max(0, _runtimeHost.GridMap.Height / 2);
                CellPos centerCell = new(centerX, centerY);
                _focusPoint = _runtimeHost.Mapper.CellToWorldCenter(centerCell);
            }

            _initialized = true;
        }

        private void HandleMovement()
        {
            if (_camera == null)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = new(horizontal, 0f, vertical);

            if (_enableEdgePan)
                input += GetEdgePanInput();

            if (input.sqrMagnitude <= 0.0001f)
                return;

            input = Vector3.ClampMagnitude(input, 1f);
            Vector3 forward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;
            float speed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? _fastMoveMultiplier : 1f);
            _focusPoint += (right * input.x + forward * input.z) * (speed * Time.deltaTime);
        }

        private Vector3 GetEdgePanInput()
        {
            Vector3 input = Vector3.zero;
            Vector3 mouse = Input.mousePosition;

            if (mouse.x <= _edgePanSize) input.x -= 1f;
            else if (mouse.x >= Screen.width - _edgePanSize) input.x += 1f;

            if (mouse.y <= _edgePanSize) input.z -= 1f;
            else if (mouse.y >= Screen.height - _edgePanSize) input.z += 1f;

            return input;
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
                _targetDistance -= scroll * _zoomSpeed * Time.deltaTime;

            _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            _distance = Mathf.Lerp(_distance, _targetDistance, 1f - Mathf.Exp(-_zoomSmooth * Time.deltaTime));
        }

        private void ClampFocusPoint()
        {
            if (_runtimeHost?.Mapper == null || _runtimeHost.GridMap == null)
                return;

            float minX = _boundsPadding;
            float minZ = _boundsPadding;
            float maxX = Mathf.Max(minX, _runtimeHost.GridMap.Width * _runtimeHost.Mapper.CellSize - _boundsPadding);
            float maxZ = Mathf.Max(minZ, _runtimeHost.GridMap.Height * _runtimeHost.Mapper.CellSize - _boundsPadding);
            _focusPoint.x = Mathf.Clamp(_focusPoint.x, minX, maxX);
            _focusPoint.z = Mathf.Clamp(_focusPoint.z, minZ, maxZ);

            CellPos focusCell = _runtimeHost.Mapper.WorldToCell(_focusPoint);
            if (_runtimeHost.Mapper.IsInside(focusCell))
                _focusPoint.y = _runtimeHost.Mapper.GetHeightAtCell(focusCell);
        }

        private void ApplyCamera(bool immediate)
        {
            if (_camera == null)
                return;

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -_distance);
            Vector3 targetPosition = _focusPoint + offset;

            if (immediate)
            {
                _camera.transform.SetPositionAndRotation(targetPosition, rotation);
                return;
            }

            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, 1f - Mathf.Exp(-12f * Time.deltaTime));
            _camera.transform.rotation = rotation;
        }
    }
}
