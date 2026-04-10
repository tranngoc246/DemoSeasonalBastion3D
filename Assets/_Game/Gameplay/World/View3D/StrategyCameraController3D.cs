using SeasonalBastion.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SeasonalBastion
{
    public sealed class StrategyCameraController3D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera _camera;
        [SerializeField] private TerrainGameplayRuntimeHost _runtimeHost;

        [Header("View")]
        [SerializeField] private float _pitch = 60f;
        [SerializeField] private float _yaw = 45f;
        [SerializeField] private float _distance = 56f;
        [SerializeField] private float _minDistance = 20f;
        [SerializeField] private float _maxDistance = 120f;
        [SerializeField] private bool _fitToMapOnStart = true;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 40f;
        [SerializeField] private float _fastMoveMultiplier = 2f;
        [SerializeField] private float _dragPanSpeed = 0.18f;
        [SerializeField] private float _edgePanSize = 20f;
        [SerializeField] private bool _enableEdgePan = true;
        [SerializeField] private bool _enableMouseDragPan = true;

        [Header("Zoom")]
        [SerializeField] private float _zoomStep = 8f;
        [SerializeField] private float _zoomSmooth = 12f;

        [Header("Bounds")]
        [SerializeField] private float _boundsPadding = 6f;
        [SerializeField] private bool _snapToTerrainCenterOnStart = true;

        private Vector3 _focusPoint;
        private float _targetDistance;
        private bool _initialized;
        private bool _dragging;
        private Vector2 _lastPointerPosition;

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
                _runtimeHost = FindFirstObjectByType<TerrainGameplayRuntimeHost>();
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

            if (_fitToMapOnStart)
            {
                float mapSpan = Mathf.Max(_runtimeHost.GridMap.Width, _runtimeHost.GridMap.Height) * _runtimeHost.Mapper.CellSize;
                float fitted = Mathf.Clamp(mapSpan * 0.55f, _minDistance, _maxDistance);
                _distance = fitted;
                _targetDistance = fitted;
            }

            ClampFocusPoint();
            ApplyCamera(true);
            _initialized = true;
        }

        private void HandleMovement()
        {
            if (_camera == null)
                return;

            Vector3 planarForward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
            Vector3 planarRight = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;

            float horizontal = ReadHorizontal();
            float vertical = ReadVertical();
            Vector3 input = planarRight * horizontal + planarForward * vertical;

            if (_enableEdgePan)
                input += planarRight * GetEdgePanInput().x + planarForward * GetEdgePanInput().z;

            if (input.sqrMagnitude > 0.0001f)
            {
                input = Vector3.ClampMagnitude(input, 1f);
                float speed = _moveSpeed * (IsPressed(KeyCode.LeftShift) ? _fastMoveMultiplier : 1f);
                _focusPoint += input * (speed * Time.deltaTime);
            }

            HandleDragPan(planarRight, planarForward);
        }

        private void HandleDragPan(Vector3 planarRight, Vector3 planarForward)
        {
            if (!_enableMouseDragPan || Mouse.current == null)
                return;

            bool dragHeld = Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed;
            Vector2 pointer = Mouse.current.position.ReadValue();

            if (dragHeld && !_dragging)
            {
                _dragging = true;
                _lastPointerPosition = pointer;
                return;
            }

            if (!dragHeld)
            {
                _dragging = false;
                return;
            }

            Vector2 delta = pointer - _lastPointerPosition;
            _lastPointerPosition = pointer;
            if (delta.sqrMagnitude <= 0.0001f)
                return;

            float dragScale = _dragPanSpeed * Mathf.Max(1f, _distance * 0.05f);
            _focusPoint -= planarRight * (delta.x * dragScale * Time.deltaTime);
            _focusPoint -= planarForward * (delta.y * dragScale * Time.deltaTime);
        }

        private Vector3 GetEdgePanInput()
        {
            if (Mouse.current == null)
                return Vector3.zero;

            Vector3 input = Vector3.zero;
            Vector2 mouse = Mouse.current.position.ReadValue();

            if (mouse.x > 0f && mouse.x <= _edgePanSize) input.x -= 1f;
            else if (mouse.x < Screen.width && mouse.x >= Screen.width - _edgePanSize) input.x += 1f;

            if (mouse.y > 0f && mouse.y <= _edgePanSize) input.z -= 1f;
            else if (mouse.y < Screen.height && mouse.y >= Screen.height - _edgePanSize) input.z += 1f;

            return input;
        }

        private void HandleZoom()
        {
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            if (Mathf.Abs(scroll) > 0.001f)
                _targetDistance -= Mathf.Sign(scroll) * _zoomStep;

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

        private static float ReadHorizontal()
        {
            float value = 0f;
            if (Keyboard.current == null)
                return value;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                value -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                value += 1f;
            return value;
        }

        private static float ReadVertical()
        {
            float value = 0f;
            if (Keyboard.current == null)
                return value;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                value -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                value += 1f;
            return value;
        }

        private static bool IsPressed(KeyCode key)
        {
            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.LeftShift => Keyboard.current.leftShiftKey.isPressed,
                KeyCode.RightShift => Keyboard.current.rightShiftKey.isPressed,
                _ => false,
            };
        }
    }
}
