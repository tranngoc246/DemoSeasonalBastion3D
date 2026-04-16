using UnityEngine;

namespace SeasonalBastion
{
    public sealed class GroundRaycastService
    {
        private readonly LayerMask _groundMask;
        private readonly float _maxDistance;

        public GroundRaycastService(LayerMask groundMask, float maxDistance = 5000f)
        {
            _groundMask = groundMask;
            _maxDistance = Mathf.Max(0.01f, maxDistance);
        }

        public bool TryRaycast(Camera camera, Vector2 screenPosition, out RaycastHit hit)
        {
            return TryRaycast(camera, screenPosition, out hit, out _);
        }

        public bool TryRaycast(Camera camera, Vector2 screenPosition, out RaycastHit hit, out Ray ray)
        {
            hit = default;
            ray = default;
            if (camera == null)
                return false;

            ray = camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out hit, _maxDistance, _groundMask, QueryTriggerInteraction.Ignore);
        }
    }
}
