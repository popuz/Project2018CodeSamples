using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using XFEL.Ctrl;

namespace Project2018CodeSamples.Ctrl
{
    public class BaseOrbitCamera : MonoBehaviour, ICamera
    {
        [Header("Base Camera")] [SerializeField]
        private float _mouseSensitivity = 4f;

        [SerializeField] private float _scrollSensitivity = 2f;
        [SerializeField] private float _orbitDampening = 2f;
        [SerializeField] protected float _scrollDampening = 2f;
        [SerializeField] private bool _isControlledOnMouseDown = true;

        protected Transform _camTransform;
        protected Transform _camParentTransform;

        private Vector2 _verticalClamp = new Vector2(-90, 90);
        protected bool _camInputIsDisabled = false;

        private Vector3 _localRotation;
        private float _cameraDistance = 10f;
        protected Quaternion _startParentRot;
        protected float _startCamDist;
        protected BaseCamDistanceHandler _distanceHandler;

        public bool IsControlledOnMouseDown => _isControlledOnMouseDown;

        public virtual void Init(Transform cam, Transform pivot)
        {
            _camTransform = cam ? cam : transform;
            _camParentTransform = pivot ? pivot : transform.parent;

            _distanceHandler = GetComponent<BaseCamDistanceHandler>();
            _distanceHandler.Init(_camTransform);

            StopCurrentRotation();

            _startParentRot = _camParentTransform.localRotation;
            _startCamDist = _cameraDistance;
        }

        protected virtual void StopCurrentRotation()
        {
            var gameObjectLocalRot = _camParentTransform.localRotation;
            _localRotation.x = gameObjectLocalRot.eulerAngles.y;
            _localRotation.y = gameObjectLocalRot.eulerAngles.x;
            _cameraDistance = _camTransform.localPosition.z * -1f;
        }

        public void Tick(float mouseX, float mouseY, float scrollWheel)
        {
            if (_camInputIsDisabled) return;

            if (MouseIsMoved(mouseX, mouseY) && (!_isControlledOnMouseDown || InputCtrl.RightMouseButtonIsPressed))
            {
                _localRotation += CalculateDeltaRotation(mouseX, mouseY);
                _localRotation = ClampRotation(_localRotation);
            }

            if (MouseIsScrolled(scrollWheel))
                _cameraDistance = CalculateTargetCameraDistance(scrollWheel, _cameraDistance);

            _camParentTransform.localRotation = GetFinalRotation(_localRotation.y, _localRotation.x);
            _distanceHandler.HandleDistance(ref _cameraDistance, _scrollDampening);
        }

        private bool MouseIsMoved(float mouseX, float mouseY) =>
            Math.Abs(mouseX) > float.Epsilon || Math.Abs(mouseY) > float.Epsilon;

        private Vector3 CalculateDeltaRotation(float inputAxisX, float inputAxisY)
        {
            var deltaRotation = Vector3.zero;
            deltaRotation.x += inputAxisX * _mouseSensitivity;
            deltaRotation.y -= inputAxisY * _mouseSensitivity;

            return deltaRotation;
        }

        private Vector3 ClampRotation(Vector3 localRotation)
        {
            if (localRotation.y < _verticalClamp.x) // MIN vertical angle
                localRotation.y = _verticalClamp.x;
            else if (localRotation.y > _verticalClamp.y) // MAX vertical angle
                localRotation.y = _verticalClamp.y;

            return localRotation;
        }

        private bool MouseIsScrolled(float scrollWheel) => Math.Abs(scrollWheel) > float.Epsilon;

        private float CalculateTargetCameraDistance(float inputZoomWheel, float cameraDistance)
        {
            var scrollAmount = inputZoomWheel * _scrollSensitivity;

            scrollAmount *= (cameraDistance * 0.3f);

            if (inputZoomWheel < 0 && _distanceHandler.WallProtector != null &&
                _distanceHandler.WallProtector.HitSomething) return cameraDistance;

            cameraDistance += scrollAmount * -1f;

            return cameraDistance;
        }

        private Quaternion GetFinalRotation(float rotY, float rotX)
        {
            var QT = Quaternion.Euler(rotY, rotX, 0);
            var parentRotation =
                Quaternion.Slerp(_camParentTransform.localRotation, QT, _orbitDampening * Time.deltaTime);

            return Quaternion.Euler(parentRotation.eulerAngles.x, parentRotation.eulerAngles.y, 0);
        }

        public void SetStartRot(Vector2 localRot)
        {
            var QT = Quaternion.Euler(localRot.y, localRot.x, 0);
            _camParentTransform.localRotation = Quaternion.Euler(QT.eulerAngles.x, QT.eulerAngles.y, 0);
            _localRotation = localRot;
        }

        public Quaternion GetCameraParentRot() => _camParentTransform.rotation;
    }
}