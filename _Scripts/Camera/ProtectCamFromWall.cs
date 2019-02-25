using System;
using System.Collections;
using UnityEngine;

namespace Project2018CodeSamples
{
  public class ProtectCamFromWall : MonoBehaviour
  {
    private const int MAX_HITTED_COLLIDERS = 10;

    [Tooltip("don't clip against objects on this layer (useful for not clipping against the targeted object)")]
    [SerializeField]
    private LayerMask _dontClipLayerMask;

    [SerializeField] private bool _showDebugRays;

    [Space] [SerializeField] private float _moveTimeToAvoidClipping = 0.05f;
    [SerializeField] private float _moveTimeToTargetPosition = 0.4f;
    [SerializeField] private float _sphereCastRadius = 0.1f;
    [SerializeField] private float _camToTargetMinDist = 0.5f;

    private Transform _camTransform;
    private Transform _orbitPivot;
    private Ray _camToTargetRay;
    private RaycastHit[] _camToTargetRayHits;
    private RayHitComparer _rayHitDistComparer;
    private Collider[] _hitColliders;
    private float _startCamDist;
    private float _camToTargetDist;
    private float _targetDist;
    private float _moveSpeed;

    public bool HitSomething { get; private set; }

    private void Start()
    {
      _camTransform = GetComponent<Camera>().transform;
      _orbitPivot = _camTransform.parent;
      _camToTargetDist = _startCamDist = _camTransform.localPosition.magnitude;

      _rayHitDistComparer = new RayHitComparer();
      _hitColliders = new Collider[MAX_HITTED_COLLIDERS];
    }

    public float ManageWallClipping()
    {
      HitSomething = false;
      _targetDist = _camToTargetDist = _startCamDist = Mathf.Abs(_camTransform.localPosition.z);

      SetRayOriginAndDirection();
      if (!HasInitialIntersect(_camTransform.position))
        return _targetDist;

      RaycastWithShiftedRayOrigin();
      Array.Sort(_camToTargetRayHits, _rayHitDistComparer);

      var nearest = Mathf.Infinity;
      foreach (var hit in _camToTargetRayHits)
        if (hit.collider != null && hit.distance < nearest)
          nearest = SetTargetDistToHitAndReturnHitDist(hit);

      if (_showDebugRays && HitSomething)
        Debug.DrawRay(_camToTargetRay.origin, _camToTargetRay.direction * _targetDist, Color.red);

      var protectionMoveTime = _camToTargetDist > _targetDist ? _moveTimeToAvoidClipping : _moveTimeToTargetPosition;
      _camToTargetDist = Mathf.SmoothDamp(_camToTargetDist, _targetDist, ref _moveSpeed, protectionMoveTime);

      return _camToTargetDist;
    }

    private void SetRayOriginAndDirection()
    {
      _camToTargetRay.origin = _orbitPivot.position;
      _camToTargetRay.direction = (_camTransform.position - _camToTargetRay.origin).normalized;
    }

    private bool HasInitialIntersect(Vector3 centerPoint)
    {
      if (Physics.OverlapSphereNonAlloc(
            centerPoint, _sphereCastRadius, _hitColliders, ~_dontClipLayerMask, QueryTriggerInteraction.Ignore) == 0)
        return false;

      foreach (var col in _hitColliders)
        if (col.attachedRigidbody == null || !col.attachedRigidbody.CompareTag("Player"))
          return true;

      return false;
    }

    private void RaycastWithShiftedRayOrigin()
    {
      var rayDist = _startCamDist + _sphereCastRadius;
      _camToTargetRay.origin += _camToTargetRay.direction * _sphereCastRadius;
      _camToTargetRayHits =
        Physics.RaycastAll(_camToTargetRay, rayDist, ~_dontClipLayerMask, QueryTriggerInteraction.Ignore);
      if (_showDebugRays)
        Debug.DrawRay(_camToTargetRay.origin, _camToTargetRay.direction * rayDist, Color.cyan);
    }

    private float SetTargetDistToHitAndReturnHitDist(RaycastHit hit)
    {
      HitSomething = true;
      _targetDist = -(_orbitPivot.InverseTransformPoint(hit.point).z + _camToTargetMinDist);
      if (_showDebugRays)
        Debug.Log(hit.collider.gameObject.name);

      return hit.distance;
    }

    private class RayHitComparer : IComparer
    {
      public int Compare(object x, object y) =>
        ((RaycastHit) x).distance.CompareTo(((RaycastHit) y).distance);
    }
  }
}