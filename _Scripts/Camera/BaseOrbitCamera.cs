using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using XFEL.Ctrl;

namespace Project2018CodeSamples
{
    public class BaseOrbitCamera : MonoBehaviour, ICamera
    {
        [SerializeField] private float _mouseSensitivity = 4f;
        [SerializeField] private float _scrollSensitivity = 2f;
        [SerializeField] private float _orbitDampening = 2f;
        [SerializeField] protected float _scrollDampening = 2f;
        [SerializeField] private bool _isControlledOnMouseDown = true;

        protected bool _camInputIsDisabled = false;
        protected Transform _camTransform;
        protected Transform _camParentTransform;

        private const float MIN_VERTICAL_ANGLE = -90;
        private const float MAX_VERTICAL_ANGLE = 90;

        private Vector3 _localRotation;
        private float _cameraDistance = 10f;
        protected float _startCamDist;
        protected BaseCamDistanceHandler _distanceHandler;

        public bool IsControlledOnMouseDown => _isControlledOnMouseDown;
        public Quaternion CameraParentRotation => _camParentTransform.rotation;

        public void SetStartRot(Vector2 localRot)
        {
            var QT = Quaternion.Euler(localRot.y, localRot.x, 0);
            _camParentTransform.localRotation = Quaternion.Euler(QT.eulerAngles.x, QT.eulerAngles.y, 0);
            _localRotation = localRot;
        }

        public virtual void Init(Transform cam, Transform pivot)
        {
            var myTransform = transform;
            _camTransform = cam ? cam : myTransform;
            _camParentTransform = pivot ? pivot : myTransform.parent;

            _distanceHandler = GetComponent<BaseCamDistanceHandler>();
            _distanceHandler.Init(_camTransform);

            StopCurrentRotation();

            _startCamDist = _cameraDistance;
        }

        protected virtual void StopCurrentRotation()
        {
            var camParentLocalRotation = _camParentTransform.localRotation;
            _localRotation.x = camParentLocalRotation.eulerAngles.y;
            _localRotation.y = camParentLocalRotation.eulerAngles.x;
            _cameraDistance = _camTransform.localPosition.z * -1f;
        }

        public void Tick(float mouseX, float mouseY, float scrollWheel)
        {
            if (_camInputIsDisabled) return;

            if (MouseIsMoved(mouseX, mouseY) && (!_isControlledOnMouseDown || InputCtrl.RightMouseButtonIsPressed))
                _localRotation = ClampRotation(_localRotation + CalculateDeltaRotation(mouseX, mouseY));
            _camParentTransform.localRotation = GetFinalRotation(_localRotation.y, _localRotation.x);
            
            if (MouseIsScrolled(scrollWheel))
                _cameraDistance += CalculateCamTargetDeltaDistance(scrollWheel, _cameraDistance);            
            _cameraDistance = _distanceHandler.HandleDistance(_cameraDistance, _scrollDampening);
        }

        #region Tick Functionality Methods              

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
            if (localRotation.y < MIN_VERTICAL_ANGLE)
                localRotation.y = MIN_VERTICAL_ANGLE;
            else if (localRotation.y > MAX_VERTICAL_ANGLE)
                localRotation.y = MAX_VERTICAL_ANGLE;

            return localRotation;
        }

        private Quaternion GetFinalRotation(float rotY, float rotX)
        {
            var QT = Quaternion.Euler(rotY, rotX, 0);
            var parentRotation =
                Quaternion.Slerp(_camParentTransform.localRotation, QT, _orbitDampening * Time.deltaTime);

            return Quaternion.Euler(parentRotation.eulerAngles.x, parentRotation.eulerAngles.y, 0);
        }
        
        private bool MouseIsScrolled(float scrollWheel) => Math.Abs(scrollWheel) > float.Epsilon;

        private float CalculateCamTargetDeltaDistance(float inputZoomWheel, float cameraDistance)
        {
            if (_distanceHandler.WallProtector != null && _distanceHandler.WallProtector.HitSomething)
                return 0;

            return -inputZoomWheel * _scrollSensitivity * (cameraDistance * 0.3f);
        }    
        #endregion
    }
}