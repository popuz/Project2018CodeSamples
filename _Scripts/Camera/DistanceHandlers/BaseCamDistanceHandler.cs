using UnityEngine;
using UnityEngine.Serialization;

namespace Project2018CodeSamples
{
    public abstract class BaseCamDistanceHandler : MonoBehaviour
    {
        public ProtectCamFromWall WallProtector { get; protected set; }

        [SerializeField] protected Vector2 _zoomClamp = new Vector2(0f, 2.5f);
        protected Transform _camera;
        protected Vector3 _targetPos;

        public abstract void RefreshCamPos();
        public virtual void Init(Transform _XForm_Camera) => _camera = _XForm_Camera;

        public float HandleDistance(float targetDist, float scrollDampening)
        {
            SetTargetPos(targetDist, scrollDampening);
            _camera.localPosition = _targetPos;

            if (WallProtector && WallProtector.enabled)
            {
                _targetPos.z = WallProtector.ManageWallCliping() * -1f;
                _camera.localPosition = _targetPos;

                if (WallProtector.HitSomething)
                    targetDist = _targetPos.z * -1f;
            }

            return targetDist;
        }

        protected abstract void SetTargetPos(float targetDist, float scrollDampening);
    }
}