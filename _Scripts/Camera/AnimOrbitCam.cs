using System;
using System.Collections;
using UnityEngine;

namespace Project2018CodeSamples
{
    public class AnimOrbitCam : BaseOrbitCamera
    {
        [SerializeField] protected AnimationCurve _animCurve;

        private const float DEFAULT_ANIM_TIME = 2f;
        private Coroutine _animCamCoroutine;

        private Vector3 _targetCamPos;
        private float _targetCamDist;

        public void AnimCamWithDistance(Vector3 targetPos, Quaternion targetRot, float dist, float animTime,
            Action onComplete)
        {
            _targetCamDist = dist < 0 ? _startCamDist : dist;            
            RestartAnimation(targetPos, targetRot, animTime, LerpCameraDistance, onComplete);
        }
             
        public void AnimCamWithShift(Vector3 targetPos, Quaternion targetRot, Vector3 camPos, float animTime,
            Action onComplete)
        {
            _targetCamPos = camPos;
            RestartAnimation(targetPos, targetRot, animTime, LerpCameraLocalPosition, onComplete);
        }
        
        private void LerpCameraDistance(Vector3 initLocalPos, float curveValue)
        {
            var newDist = Mathf.Lerp(initLocalPos.z, _targetCamDist * -1f, curveValue);
            _camTransform.localPosition.Set(initLocalPos.x, initLocalPos.y, newDist);
        }
        
        private void LerpCameraLocalPosition(Vector3 initLocalPos, float curveValue) =>
            _camTransform.localPosition = Vector3.Lerp(initLocalPos, _targetCamPos, curveValue);

        private void RestartAnimation(Vector3 parentPos, Quaternion parentRot, float animTime,
            Action<Vector3, float> AdjustCamLocalPos, Action onComplete)
        {
            StopCurrentRotation();
            var newAnimTime = animTime < 0 ? DEFAULT_ANIM_TIME : animTime;
            _animCamCoroutine =
                StartCoroutine(RefocusAnim(parentPos, parentRot, newAnimTime, AdjustCamLocalPos, onComplete));
        }

        protected override void StopCurrentRotation()
        {
            base.StopCurrentRotation();
            if (_animCamCoroutine != null)            
                StopCoroutine(_animCamCoroutine);             
        }

        private IEnumerator RefocusAnim(Vector3 targetPos, Quaternion targetRot, float animTime,
            Action<Vector3, float> AdjustCamLocalPos, Action onComplete)
        {
            var initPos = _camParentTransform.position;
            var initRot = _camParentTransform.rotation;
            var initLocalPos = _camTransform.localPosition;

            _camInputIsDisabled = true;
            float t = 0;

            while (t < animTime)
            {
                var curveValue = _animCurve.Evaluate(t / animTime);
                
                AdjustCamLocalPos(initLocalPos, curveValue);
                AdjustCameraParentTransform(targetPos, targetRot, initPos, initRot, curveValue);

                t += Time.deltaTime;
                yield return null;
            }

            StopCurrentRotation();
            _camInputIsDisabled = false;
            _distanceHandler.RefreshCamPos();
            onComplete?.Invoke();
        }

        private void AdjustCameraParentTransform(Vector3 targetPos, Quaternion targetRot, Vector3 initPos,
            Quaternion initRot, float curveValue)
        {
            _camParentTransform.position = Vector3.Lerp(initPos, targetPos, curveValue);
            _camParentTransform.localRotation = Quaternion.Slerp(initRot, targetRot, curveValue);
        }      
    }
}