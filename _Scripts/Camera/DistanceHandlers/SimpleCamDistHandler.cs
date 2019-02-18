using UnityEngine;

namespace Project2018CodeSamples
{
    public class SimpleCamDistHandler : BaseCamDistanceHandler
    {
        public override void Init(Transform _XForm_Camera)
        {
            base.Init(_XForm_Camera);
            WallProtector = GetComponent<ProtectCamFromWall>();
        }

        protected override void SetTargetPos(float targetDist, float scrollDampening)
        {
            var cameraLocalPosition = _camera.localPosition;
            var clampedDist = Mathf.Clamp(targetDist, _zoomClamp.x, _zoomClamp.y);

            var newZ = Mathf.Lerp(cameraLocalPosition.z, clampedDist * -1f, scrollDampening * Time.deltaTime);

            _targetPos.Set(cameraLocalPosition.x, cameraLocalPosition.y, newZ);
        }

        public override void RefreshCamPos()
        {
        }
    }
}