using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class EnemyMovementPresenter3D : MonoBehaviour
    {
        [SerializeField] private float _positionSmooth = 16f;
        [SerializeField] private float _rotationSmooth = 18f;
        [SerializeField] private float _minLookSqrMagnitude = 0.0001f;

        public void Present(CellWorldMapper3D mapper, RunStartRuntime runStart, EnemyState state, Vector3 visualOffset)
        {
            if (mapper == null)
                return;

            Vector3 targetPosition = mapper.CellToWorldCenter(state.Cell) + visualOffset;
            Vector3 motion = Vector3.zero;

            if (runStart != null && runStart.Lanes != null && runStart.Lanes.TryGetValue(state.Lane, out var lane))
            {
                Vector3 laneStep = DirToWorld(lane.DirToHQ, mapper.CellSize);
                float progress = Mathf.Clamp01(state.MoveProgress01);
                targetPosition -= laneStep * (1f - progress);
                motion = laneStep;
            }

            float dt = Mathf.Max(Time.deltaTime, 0f);
            float posT = 1f - Mathf.Exp(-_positionSmooth * dt);
            Vector3 currentPosition = transform.position;
            Vector3 nextPosition = Vector3.Lerp(currentPosition, targetPosition, posT);
            transform.position = nextPosition;

            Vector3 look = motion;
            look.y = 0f;
            if (look.sqrMagnitude <= _minLookSqrMagnitude)
            {
                look = nextPosition - currentPosition;
                look.y = 0f;
            }

            if (look.sqrMagnitude > _minLookSqrMagnitude)
            {
                Quaternion targetRotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                float rotT = 1f - Mathf.Exp(-_rotationSmooth * dt);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotT);
            }
        }

        private static Vector3 DirToWorld(Dir4 dir, float cellSize)
        {
            return dir switch
            {
                Dir4.N => new Vector3(0f, 0f, cellSize),
                Dir4.E => new Vector3(cellSize, 0f, 0f),
                Dir4.S => new Vector3(0f, 0f, -cellSize),
                Dir4.W => new Vector3(-cellSize, 0f, 0f),
                _ => Vector3.zero,
            };
        }
    }
}
