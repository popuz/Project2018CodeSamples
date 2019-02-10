using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Project2018CodeSamples.Ctrl
{
    /// <summary>
    /// Responsible for proceeding camera transition form FPS to Orbit Look Behaviour
    /// </summary>
    public class CameraCtrl : BaseCtrl
    {
        private const float CENTER_TO_HEAD_DIST = 0.8f;
        private Transform _character;
        private ICamera _activeCam;
        private CharacterCamera _fpsCam;
        private AnimOrbitCam _orbitCam;

        private bool _lockCursor = true;
        private bool _cursorIsLocked = true;
        private Vector3 _mouseAxisInput;
        private Vector2 _lastFpsCameraRot;
        private Quaternion _lastFpsCameraRotByOrbit;

        private Coroutine _currCoroute;
        private RaycastHit _hit;
        private Ray _ray;

        public Transform CameraTransform { get; private set; }
        public Camera UnityCamera { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _character = GameObject.FindGameObjectWithTag("Player").transform;
            UnityCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (UnityCamera != null)
                CameraTransform = UnityCamera.transform;
            _fpsCam = FindObjectOfType<CharacterCamera>();
            _orbitCam = FindObjectOfType<AnimOrbitCam>();
            _activeCam = _fpsCam;
        }

        private void Start()
        {
            _fpsCam.Init(CameraTransform, _character);
            _orbitCam.Init(CameraTransform, CameraTransform.parent);

            GameManager.Instance.OnGamePause += ToggleActivation;
        }

        private void ToggleActivation(bool isPaused)
        {
            if (isPaused)
                Off();
            else
                On();
        }

        private void Update() => _mouseAxisInput = GetMouseInput();

        private Vector3 GetMouseInput()
        {
            if (_activeCam == _orbitCam && _orbitCam.IsControlledOnMouseDown == false)
                return new Vector3(-CrossPlatformInputManager.GetAxis("Horizontal"),
                    -CrossPlatformInputManager.GetAxis("Vertical"),
                    CrossPlatformInputManager.GetAxis("Mouse ScrollWheel"));

            return new Vector3(CrossPlatformInputManager.GetAxis("Mouse X"),
                CrossPlatformInputManager.GetAxis("Mouse Y"), CrossPlatformInputManager.GetAxis("Mouse ScrollWheel"));
        }

        private void FixedUpdate()
        {
            _activeCam.Tick(_mouseAxisInput.x, _mouseAxisInput.y, _mouseAxisInput.z);

            if (_lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                _cursorIsLocked = false;
            else if (Input.GetMouseButtonUp(0))
                _cursorIsLocked = true;

            RefreshCursorLockStateAndVisibility();
        }

        private void RefreshCursorLockStateAndVisibility()
        {
            Cursor.lockState = _cursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !_cursorIsLocked;
        }

        public override void On()
        {
            base.On();
            SetCursorLockAndVisibility(GameManager.Instance.State == GameStateTypes.FPS);
        }

        public override void Off()
        {
            base.Off();
            SetCursorLockAndVisibility(false);
        }

        private void SetCursorLockAndVisibility(bool isLocked)
        {
            _lockCursor = isLocked;
            _cursorIsLocked = isLocked;
            RefreshCursorLockStateAndVisibility();
        }

        public void SwitchToFpsCam(float animTime, Action onComplete)
        {
            if (_activeCam == _fpsCam) return;

            _orbitCam.AnimCamTo(_character.position + Vector3.up * CENTER_TO_HEAD_DIST, _lastFpsCameraRotByOrbit,
                Vector3.zero, animTime, null, onComplete: OnTransitionToFpsCamComplete + onComplete);

            SetCursorLockAndVisibility(true);
        }

        private void OnTransitionToFpsCamComplete()
        {
            _activeCam = _fpsCam;
            _orbitCam.SetStartRot(Vector2.zero);
            _fpsCam.SetRot(_lastFpsCameraRot.y, _lastFpsCameraRot.x);
        }

        public void SwitchToOrbitCam(Transform targetPivot, Transform targetCamTrsf, float animTime)
        {
            if (_activeCam == _fpsCam)
            {
                _lastFpsCameraRot = _fpsCam.GetRotAndResetAllRotToZero();
                _orbitCam.SetStartRot(_lastFpsCameraRot);
                _lastFpsCameraRotByOrbit = _orbitCam.GetCameraParentRot();
                _activeCam = _orbitCam;
            }

            _orbitCam.AnimCamTo(targetPivot.position, targetPivot.rotation, targetCamTrsf.localPosition, animTime, null,
                onComplete: () => SetCursorLockAndVisibility(false));
        }

        public bool CompareCamRayHit(Collider target, float rayDist)
        {
            bool isFpsState = GameManager.Instance.State == GameStateTypes.FPS;
            _ray = isFpsState ? GetRayForFps() : GetRayForOrbit();

            if (Physics.Raycast(_ray, out _hit, rayDist) && _hit.collider)
                return _hit.collider == target;

            return false;
        }

        private Ray GetRayForFps() => new Ray(CameraTransform.position, CameraTransform.forward);
        private Ray GetRayForOrbit() => UnityCamera.ScreenPointToRay(Input.mousePosition);
    }
}