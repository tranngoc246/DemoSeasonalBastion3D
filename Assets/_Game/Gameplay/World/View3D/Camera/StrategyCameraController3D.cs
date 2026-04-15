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
        [SerializeField] private PlacementPreviewController3D _placementPreview;

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

        [Header("Rotation")]
        [SerializeField] private bool _enableMouseRotate = true;
        [SerializeField] private float _mouseRotateSpeed = 0.18f;
        [SerializeField] private float _rotateStepDegrees = 45f;
        [SerializeField] private float _yawSmooth = 12f;
        [SerializeField] private float _pitchSmooth = 12f;
        [SerializeField] private float _minPitch = 25f;
        [SerializeField] private float _maxPitch = 80f;
        [SerializeField] private KeyCode _rotateLeftKey = KeyCode.Q;
        [SerializeField] private KeyCode _rotateRightKey = KeyCode.E;
        [SerializeField] private KeyCode _resetCameraKey = KeyCode.Home;

        [Header("Zoom")]
        [SerializeField] private float _zoomStep = 8f;
        [SerializeField] private float _zoomSmooth = 12f;

        [Header("Bounds")]
        [SerializeField] private float _boundsPadding = 6f;
        [SerializeField] private bool _snapToTerrainCenterOnStart = true;

        private Vector3 _focusPoint;
        private float _targetDistance;
        private float _targetYaw;
        private float _targetPitch;
        private float _defaultDistance;
        private float _defaultYaw;
        private float _defaultPitch;
        private bool _initialized;
        private bool _panDragging;
        private bool _rotateDragging;
        private Vector2 _lastPointerPosition;

        private void Awake()
        {
            ResolveRefs();
            _targetDistance = _distance;
            _targetYaw = _yaw;
            _targetPitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            _pitch = _targetPitch;
            _defaultDistance = _distance;
            _defaultYaw = _yaw;
            _defaultPitch = _pitch;
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

            HandleRotation();
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
            if (_placementPreview == null)
                _placementPreview = FindFirstObjectByType<PlacementPreviewController3D>();
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
                _focusPoint = GetMapWorldBounds().center;

            if (_fitToMapOnStart)
            {
                Bounds mapBounds = GetMapWorldBounds();
                float mapSpan = Mathf.Max(mapBounds.size.x, mapBounds.size.z);
                float fitted = Mathf.Clamp(mapSpan * 0.55f, _minDistance, _maxDistance);
                _defaultDistance = fitted;
            }
            else
            {
                _defaultDistance = _distance;
            }

            _defaultYaw = _yaw;
            _defaultPitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            ResetToMapCenter(true);
            _initialized = true;
        }

        private void HandleRotation()
        {
            if (WasPressedThisFrame(_resetCameraKey))
            {
                ResetToMapCenter(false);
                return;
            }

            if (WasPressedThisFrame(_rotateLeftKey))
                _targetYaw -= _rotateStepDegrees;
            if (WasPressedThisFrame(_rotateRightKey))
                _targetYaw += _rotateStepDegrees;

            if (_enableMouseRotate && Mouse.current != null)
            {
                bool placementModeActive = _placementPreview != null
                    && _placementPreview.enabled
                    && _placementPreview.gameObject.activeInHierarchy
                    && _placementPreview.PlacementModeActive;
                bool rotateHeld = !placementModeActive && Mouse.current.rightButton.isPressed;
                Vector2 pointer = Mouse.current.position.ReadValue();

                if (rotateHeld && !_rotateDragging)
                {
                    _rotateDragging = true;
                    _lastPointerPosition = pointer;
                    return;
                }

                if (!rotateHeld)
                {
                    _rotateDragging = false;
                }
                else
                {
                    Vector2 delta = pointer - _lastPointerPosition;
                    _lastPointerPosition = pointer;
                    float rotateScale = _mouseRotateSpeed * Time.deltaTime * 60f;
                    _targetYaw += delta.x * rotateScale;
                    _targetPitch = Mathf.Clamp(_targetPitch - delta.y * rotateScale, _minPitch, _maxPitch);
                }
            }

            _yaw = Mathf.LerpAngle(_yaw, _targetYaw, 1f - Mathf.Exp(-_yawSmooth * Time.deltaTime));
            _pitch = Mathf.Lerp(_pitch, _targetPitch, 1f - Mathf.Exp(-_pitchSmooth * Time.deltaTime));
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
            {
                Vector3 edgeInput = GetEdgePanInput();
                input += planarRight * edgeInput.x + planarForward * edgeInput.z;
            }

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

            bool dragHeld = Mouse.current.middleButton.isPressed;
            Vector2 pointer = Mouse.current.position.ReadValue();

            if (dragHeld && !_panDragging)
            {
                _panDragging = true;
                _lastPointerPosition = pointer;
                return;
            }

            if (!dragHeld)
            {
                _panDragging = false;
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

            Bounds mapBounds = GetMapWorldBounds();
            float minX = mapBounds.min.x + _boundsPadding;
            float maxX = mapBounds.max.x - _boundsPadding;
            float minZ = mapBounds.min.z + _boundsPadding;
            float maxZ = mapBounds.max.z - _boundsPadding;

            if (minX > maxX)
            {
                float centerX = mapBounds.center.x;
                minX = centerX;
                maxX = centerX;
            }

            if (minZ > maxZ)
            {
                float centerZ = mapBounds.center.z;
                minZ = centerZ;
                maxZ = centerZ;
            }

            _focusPoint.x = Mathf.Clamp(_focusPoint.x, minX, maxX);
            _focusPoint.z = Mathf.Clamp(_focusPoint.z, minZ, maxZ);

            if (_runtimeHost.Mapper.TryWorldToCell(_focusPoint, out var focusCell))
                _focusPoint.y = _runtimeHost.Mapper.GetHeightAtCell(focusCell);
        }

        private void ResetToMapCenter(bool immediate)
        {
            if (_runtimeHost?.GridMap == null || _runtimeHost.Mapper == null)
                return;

            _focusPoint = GetMapWorldBounds().center;
            _targetDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
            _distance = _targetDistance;
            _targetYaw = _defaultYaw;
            _yaw = _targetYaw;
            _targetPitch = _defaultPitch;
            _pitch = _targetPitch;
            ClampFocusPoint();
            ApplyCamera(immediate);
        }

        public bool TrySetFocusPoint(Vector3 worldPoint)
        {
            _focusPoint = worldPoint;
            ClampFocusPoint();
            return true;
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

            float followSmooth = 12f;
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, targetPosition, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
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

        private Bounds GetMapWorldBounds()
        {
            if (_runtimeHost?.Mapper == null || _runtimeHost.GridMap == null)
                return new Bounds(Vector3.zero, Vector3.zero);

            return _runtimeHost.Mapper.GetFootprintWorldBounds(
                new CellPos(0, 0),
                _runtimeHost.GridMap.Width,
                _runtimeHost.GridMap.Height);
        }

        private static bool WasPressedThisFrame(KeyCode key)
        {
            if (Keyboard.current == null)
                return false;

            return key switch
            {
                KeyCode.Q => Keyboard.current.qKey.wasPressedThisFrame,
                KeyCode.E => Keyboard.current.eKey.wasPressedThisFrame,
                KeyCode.Home => Keyboard.current.homeKey.wasPressedThisFrame,
                _ => false,
            };
        }
    }
}
