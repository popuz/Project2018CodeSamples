using System;
using UnityEngine;

namespace Project2018CodeSamples
{
    public class CharacterCamera : MonoBehaviour, ICamera
    {
        public float XSensitivity = 1f;
        public float YSensitivity = 1f;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth = true;
        public float smoothTime = 10f;

        private Transform _camera;
        private Transform _character;

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;

        public void Init(Transform cam, Transform pivot)
        {
            _camera = cam ? cam : transform;
            _character = pivot ? pivot : GameObject.FindGameObjectWithTag("Player").transform;

            _cameraTargetRot = _camera.localRotation;
            _characterTargetRot = _character.localRotation;
        }

        public void Tick(float mouseX, float mouseY, float scrollWheel)
        {
            if (MouseIsMoved(mouseX, mouseY))
                CalculateTargetRotation(mouseX, mouseY);

            HandleRotation();
        }

        private bool MouseIsMoved(float mouseX, float mouseY) =>
            Math.Abs(mouseX) > float.Epsilon || Math.Abs(mouseY) > float.Epsilon;

        private void CalculateTargetRotation(float inputAxisX, float inputAxisY)
        {
            var yRot = inputAxisX * XSensitivity;
            var xRot = inputAxisY * YSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

        private void HandleRotation()
        {
            if (smooth)
            {
                _character.localRotation =
                    Quaternion.Slerp(_character.localRotation, _characterTargetRot, smoothTime * Time.deltaTime);
                _camera.localRotation =
                    Quaternion.Slerp(_camera.localRotation, _cameraTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                _character.localRotation = _characterTargetRot;
                _camera.localRotation = _cameraTargetRot;
            }
        }

        public Vector2 GetRotAndResetAllRotToZero()
        {
            var localRot = new Vector2(_character.localRotation.eulerAngles.y, _camera.localRotation.eulerAngles.x);
            _character.localRotation = _camera.localRotation = Quaternion.identity;
            _characterTargetRot = _cameraTargetRot = Quaternion.identity;
            return localRot;
        }

        public void SetRot(float rotX, float rotY)
        {
            _character.localRotation = Quaternion.Euler(0, rotY, 0);
            _camera.localRotation = Quaternion.Euler(rotX, 0, 0);
            _characterTargetRot = _character.localRotation;
            _cameraTargetRot = _camera.localRotation;
        }
    }
}