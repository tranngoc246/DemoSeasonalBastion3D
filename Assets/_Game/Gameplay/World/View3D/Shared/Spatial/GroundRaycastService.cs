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
            hit = default;
            if (camera == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out hit, _maxDistance, _groundMask, QueryTriggerInteraction.Ignore);
        }
    }
}
