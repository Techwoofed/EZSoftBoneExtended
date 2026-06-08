/* Compound collision proxy for EZSoftBone.
 * Attach one or more of these components to a bone Transform.
 */
using UnityEngine;

namespace VAMEZSoftBones
{
    [AddComponentMenu("EZSoftBone/Bone Collision Proxy")]
    public class EZSoftBoneBoneCollider : MonoBehaviour
    {
        public enum Shape
        {
            Sphere,
            Capsule,
        }

        public enum Axis
        {
            AutoBone,
            X,
            Y,
            Z,
        }

        [SerializeField]
        private Shape m_Shape = Shape.Sphere;
        public Shape shape { get { return m_Shape; } set { m_Shape = value; } }

        [SerializeField]
        private Axis m_Axis = Axis.AutoBone;
        public Axis axis { get { return m_Axis; } set { m_Axis = value; } }

        [SerializeField]
        private Vector3 m_LocalCenter = Vector3.zero;
        public Vector3 localCenter { get { return m_LocalCenter; } set { m_LocalCenter = value; } }

        [SerializeField]
        private float m_Radius = 0.02f;
        public float radius { get { return m_Radius; } set { m_Radius = Mathf.Max(0f, value); } }

        [SerializeField]
        private float m_Height = 0.1f;
        public float height { get { return m_Height; } set { m_Height = Mathf.Max(0f, value); } }

        [SerializeField, Range(2, 16)]
        private int m_CapsuleSamples = 3;
        public int capsuleSamples { get { return m_CapsuleSamples; } set { m_CapsuleSamples = Mathf.Clamp(value, 2, 16); } }

        [SerializeField, Range(0f, 1f)]
        private float m_Weight = 1f;
        public float weight { get { return m_Weight; } set { m_Weight = Mathf.Clamp01(value); } }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_Radius = Mathf.Max(0f, m_Radius);
            m_Height = Mathf.Max(m_Radius * 2f, m_Height);
            m_CapsuleSamples = Mathf.Clamp(m_CapsuleSamples, 2, 16);
            m_Weight = Mathf.Clamp01(m_Weight);
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            Vector3 scale = transform.lossyScale;
            float maxScale = Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

            float worldRadius = Mathf.Max(0f, m_Radius) * maxScale;
            if (worldRadius <= 0f) return;

            Vector3 center = transform.TransformPoint(m_LocalCenter);

            if (m_Shape == Shape.Sphere)
            {
                Gizmos.DrawWireSphere(center, worldRadius);
                return;
            }

            Vector3 direction = GetPreviewAxis();
            if (direction.sqrMagnitude <= 1e-10f)
                direction = transform.up;
            direction.Normalize();

            float worldHeight = Mathf.Max(m_Height * maxScale, worldRadius * 2f);
            float halfLineLength = Mathf.Max(0f, worldHeight * 0.5f - worldRadius);
            Vector3 endpoint0 = center - direction * halfLineLength;
            Vector3 endpoint1 = center + direction * halfLineLength;

            int samples = Mathf.Max(2, m_CapsuleSamples);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)(samples - 1);
                Gizmos.DrawWireSphere(Vector3.Lerp(endpoint0, endpoint1, t), worldRadius);
            }

            Gizmos.DrawLine(endpoint0, endpoint1);
        }

        private Vector3 GetPreviewAxis()
        {
            if (m_Axis == Axis.AutoBone)
            {
                if (transform.childCount > 0)
                {
                    Vector3 childDirection = transform.GetChild(0).position - transform.position;
                    if (childDirection.sqrMagnitude > 1e-10f)
                        return childDirection.normalized;
                }

                if (transform.parent != null)
                {
                    Vector3 parentDirection = transform.position - transform.parent.position;
                    if (parentDirection.sqrMagnitude > 1e-10f)
                        return parentDirection.normalized;
                }

                return transform.up;
            }

            switch (m_Axis)
            {
                case Axis.X:
                    return transform.right;
                case Axis.Z:
                    return transform.forward;
                default:
                    return transform.up;
            }
        }
#endif
    }
}
