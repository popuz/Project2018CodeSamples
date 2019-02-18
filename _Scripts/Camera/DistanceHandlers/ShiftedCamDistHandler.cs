using UnityEngine;

namespace Project2018CodeSamples
{
    public class ShiftedCamDistHandler : BaseCamDistanceHandler
    {
        private Vector3 _maxCamPos;

        public override void Init(Transform _XForm_Camera)
        {
            base.Init(_XForm_Camera);
            WallProtector = GetComponent<ProtectCamFromWall>();
            _maxCamPos = _camera.localPosition;
        }

        protected override void SetTargetPos(float targetDist, float scrollDampening)
        {
            var cameraLocalPosition = _camera.localPosition;
            var zCurrentToMaxRatio = cameraLocalPosition.z / _zoomClamp.y * -1f;

            var xLerpedByZ = Mathf.Lerp(0f, _maxCamPos.x, zCurrentToMaxRatio);
            var yLerpedByZ = Mathf.Lerp(0f, _maxCamPos.y, zCurrentToMaxRatio);
            var clampedTargetDist = Mathf.Clamp(targetDist, _zoomClamp.x, _zoomClamp.y) * -1f;
            var zLerpedByTime = Mathf.Lerp(cameraLocalPosition.z, clampedTargetDist, scrollDampening * Time.deltaTime);

            _targetPos.Set(xLerpedByZ, yLerpedByZ, zLerpedByTime);
        }

        public override void RefreshCamPos()
        {
            var cameraLocalPosition = _camera.localPosition;
            _maxCamPos = cameraLocalPosition / (cameraLocalPosition.z / _zoomClamp.y * -1f);
        }
    }
}