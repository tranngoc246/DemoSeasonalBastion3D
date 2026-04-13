using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class NpcMovementPresenter3D : MonoBehaviour
    {
        [SerializeField] private float _positionSmooth = 12f;
        [SerializeField] private float _rotationSmooth = 14f;
        [SerializeField] private float _minLookSqrMagnitude = 0.0001f;

        private Vector3 _lastTargetPosition;
        private bool _hasLastTarget;

        public void Present(CellWorldMapper3D mapper, NpcState state, Vector3 visualOffset)
        {
            if (mapper == null)
                return;

            Vector3 targetPosition = mapper.CellToWorldCenter(state.Cell) + visualOffset;
            Vector3 currentPosition = transform.position;
            float dt = Mathf.Max(Time.deltaTime, 0f);
            float posT = 1f - Mathf.Exp(-_positionSmooth * dt);
            transform.position = Vector3.Lerp(currentPosition, targetPosition, posT);

            if (_hasLastTarget)
            {
                Vector3 delta = targetPosition - _lastTargetPosition;
                delta.y = 0f;
                if (delta.sqrMagnitude > _minLookSqrMagnitude)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
                    float rotT = 1f - Mathf.Exp(-_rotationSmooth * dt);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotT);
                }
            }

            _lastTargetPosition = targetPosition;
            _hasLastTarget = true;
        }
    }
}
