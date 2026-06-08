/* Author:          ezhex1991@outlook.com
 * CreateTime:      2018-12-18 19:33:50
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAMEZSoftBones
{
    public delegate Vector3 CustomForce(float normalizedLength);

    public class EZSoftBone : MonoBehaviour
    {
        public static readonly float DeltaTime_Min = 1e-6f;

        public enum UnificationMode
        {
            None,
            Rooted,
            Unified,
        }

        public enum DeltaTimeMode
        {
            DeltaTime,
            UnscaledDeltaTime,
            Constant,
        }

        private class Bone
        {
            public Bone parentBone;
            public Vector3 localPosition;
            public Quaternion localRotation;

            public Bone leftBone;
            public Vector3 leftPosition;
            public Bone rightBone;
            public Vector3 rightPosition;

            public List<Bone> childBones = new List<Bone>();

            public Transform transform;
            public Vector3 worldPosition;

            public Transform systemSpace;
            public Vector3 systemPosition;

            public int depth;
            public float boneLength;
            public float treeLength;
            public float normalizedLength;

            public float radius;
            public float damping;
            public float stiffness;
            public float resistance;
            public float slackness;

            //Techwoof: Cache for the flickering workaround
            public Vector3 solvedLocalPosition;
            public Quaternion solvedLocalRotation;
            public bool hasSolvedPose;

            public Vector3 renderBackupLocalPosition;
            public Quaternion renderBackupLocalRotation;
            public bool hasRenderBackup;

            public Vector3 speed;

            // Cached authoring components. Multiple proxies may be attached to one bone.
            public EZSoftBoneBoneCollider[] collisionProxies;

            public Bone(Transform systemSpace, Transform transform, IEnumerable<Transform> endBones, int startDepth, int depth, float nodeLength, float boneLength)
            {
                this.transform = transform;
                this.systemSpace = systemSpace;
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
                collisionProxies = transform.GetComponents<EZSoftBoneBoneCollider>();
                this.depth = depth;
                if (depth > startDepth)
                {
                    this.boneLength = boneLength + nodeLength;
                }
                treeLength = Mathf.Max(treeLength, this.boneLength);
                if (transform.childCount > 0 && !endBones.Contains(transform))
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        if (!child.gameObject.activeSelf) continue;
                        Bone childBone = new Bone(systemSpace, child, endBones, startDepth, depth + 1, Vector3.Distance(child.position, transform.position), this.boneLength);
                        childBone.parentBone = this;
                        childBones.Add(childBone);
                        treeLength = Mathf.Max(treeLength, childBone.treeLength);
                    }
                }
                normalizedLength = treeLength == 0 ? 0 : (this.boneLength / treeLength);
            }

            public void SetTreeLength()
            {
                SetTreeLength(treeLength);
            }
            public void SetTreeLength(float treeLength)
            {
                this.treeLength = treeLength;
                normalizedLength = treeLength == 0 ? 0 : (boneLength / treeLength);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].SetTreeLength(treeLength);
                }
            }

            public void SetLeftSibling(Bone left)
            {
                if (left == this || left == rightBone) return;
                leftBone = left;
                leftPosition = transform.InverseTransformPoint(left.worldPosition);
            }
            public void SetRightSibling(Bone right)
            {
                if (right == this || right == leftBone) return;
                rightBone = right;
                rightPosition = transform.InverseTransformPoint(right.worldPosition);
            }

            public void Inflate(float baseRadius, AnimationCurve radiusCurve)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].Inflate(baseRadius, radiusCurve);
                }
            }
            public void Inflate(float baseRadius, AnimationCurve radiusCurve, EZSoftBoneMaterial material)
            {
                radius = radiusCurve.Evaluate(normalizedLength) * baseRadius;
                damping = material.GetDamping(normalizedLength);
                stiffness = material.GetStiffness(normalizedLength);
                resistance = material.GetResistance(normalizedLength);
                slackness = material.GetSlackness(normalizedLength);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].Inflate(baseRadius, radiusCurve, material);
                }
            }

            public void RevertTransforms(int startDepth)
            {
                if (depth > startDepth)
                {
                    transform.localPosition = localPosition;
                    transform.localRotation = localRotation;
                }
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].RevertTransforms(startDepth);
                }
            }
            public void UpdateTransform(bool siblingRotationConstraints, int startDepth)
            {
                if (depth > startDepth)
                {
                    if (childBones.Count == 1)
                    {
                        Bone childBone = childBones[0];
                        transform.rotation *= Quaternion.FromToRotation(childBone.localPosition,
                                                                        transform.InverseTransformVector(childBone.worldPosition - worldPosition));

                        if (siblingRotationConstraints)
                        {
                            if (leftBone != null && rightBone != null)
                            {
                                Vector3 directionLeft0 = leftPosition;
                                Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                                Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);

                                Vector3 directionRight0 = rightPosition;
                                Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                                Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);

                                transform.rotation *= Quaternion.Lerp(rotationLeft, rotationRight, 0.5f);
                            }
                            else if (leftBone != null)
                            {
                                Vector3 directionLeft0 = leftPosition;
                                Vector3 directionLeft1 = transform.InverseTransformVector(leftBone.worldPosition - worldPosition);
                                Quaternion rotationLeft = Quaternion.FromToRotation(directionLeft0, directionLeft1);
                                transform.rotation *= rotationLeft;
                            }
                            else if (rightBone != null)
                            {
                                Vector3 directionRight0 = rightPosition;
                                Vector3 directionRight1 = transform.InverseTransformVector(rightBone.worldPosition - worldPosition);
                                Quaternion rotationRight = Quaternion.FromToRotation(directionRight0, directionRight1);
                                transform.rotation *= rotationRight;
                            }
                        }
                    }
                    transform.position = worldPosition;
                }

                if (systemSpace != null) systemPosition = systemSpace.InverseTransformPoint(worldPosition);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].UpdateTransform(siblingRotationConstraints, startDepth);
                }
            }

            public void SetRestState()
            {
                worldPosition = transform.position;
                systemPosition = systemSpace == null ? worldPosition : systemSpace.InverseTransformPoint(worldPosition);
                speed = Vector3.zero;
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].SetRestState();
                }
            }
            public void UpdateSpace()
            {
                if (systemSpace == null) return;
                worldPosition = systemSpace.TransformPoint(systemPosition);
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].UpdateSpace();
                }
            }

            public void RefreshCollisionProxies()
            {
                collisionProxies = transform.GetComponents<EZSoftBoneBoneCollider>();
                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].RefreshCollisionProxies();
                }
            }

            //techwoof: Cached pose workaround
            public bool CacheSolvedPose(int startDepth)
            {
                bool cachedAny = false;

                if (depth > startDepth)
                {
                    solvedLocalPosition = transform.localPosition;
                    solvedLocalRotation = transform.localRotation;
                    hasSolvedPose = true;
                    cachedAny = true;
                }

                for (int i = 0; i < childBones.Count; i++)
                {
                    if (childBones[i].CacheSolvedPose(startDepth))
                    {
                        cachedAny = true;
                    }
                }

                return cachedAny;
            }

            public void BackupRenderPose(int startDepth)
            {
                if (depth > startDepth && hasSolvedPose)
                {
                    renderBackupLocalPosition = transform.localPosition;
                    renderBackupLocalRotation = transform.localRotation;
                    hasRenderBackup = true;
                }

                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].BackupRenderPose(startDepth);
                }
            }

            public void ApplySolvedPose(int startDepth)
            {
                if (depth > startDepth && hasSolvedPose)
                {
                    transform.localPosition = solvedLocalPosition;
                    transform.localRotation = solvedLocalRotation;
                }

                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].ApplySolvedPose(startDepth);
                }
            }

            public void RestoreRenderPose(int startDepth)
            {
                if (depth > startDepth && hasRenderBackup)
                {
                    transform.localPosition = renderBackupLocalPosition;
                    transform.localRotation = renderBackupLocalRotation;
                    hasRenderBackup = false;
                }

                for (int i = 0; i < childBones.Count; i++)
                {
                    childBones[i].RestoreRenderPose(startDepth);
                }
            }
        }

        [SerializeField]
        private List<Transform> m_RootBones;
        public List<Transform> rootBones { get { return m_RootBones; } }
        [SerializeField]
        private List<Transform> m_EndBones;
        public List<Transform> endBones { get { return m_EndBones; } }

        [SerializeField]
        private EZSoftBoneMaterial m_Material;
        private EZSoftBoneMaterial m_InstanceMaterial;
        public EZSoftBoneMaterial sharedMaterial
        {
            get
            {
                if (m_Material == null)
                    m_Material = EZSoftBoneMaterial.defaultMaterial;
                return m_Material;
            }
            set
            {
                m_Material = value;
            }
        }
        public EZSoftBoneMaterial material
        {
            get
            {
                if (m_InstanceMaterial == null)
                {
                    m_InstanceMaterial = m_Material = Instantiate(sharedMaterial);
                }
                return m_InstanceMaterial;
            }
            set
            {
                m_InstanceMaterial = m_Material = value;
            }
        }

        #region Structure
        [SerializeField]
        private int m_StartDepth;
        public int startDepth { get { return m_StartDepth; } set { m_StartDepth = value; } }

        [SerializeField]
        private UnificationMode m_SiblingConstraints = UnificationMode.None;
        public UnificationMode siblingConstraints { get { return m_SiblingConstraints; } set { m_SiblingConstraints = value; } }
        [SerializeField]
        private bool m_ClosedSiblings = false;
        public bool closedSiblings { get { return m_ClosedSiblings; } set { m_ClosedSiblings = value; } }
        [SerializeField]
        private bool m_SiblingRotationConstraints = true;
        public bool siblingRotationConstraints { get { return m_SiblingRotationConstraints; } set { m_SiblingRotationConstraints = value; } }
        [SerializeField]
        private UnificationMode m_LengthUnification = UnificationMode.None;
        public UnificationMode lengthUnification { get { return m_LengthUnification; } set { m_LengthUnification = value; } }
        #endregion

        #region Collision
        //Techwoof: We add VAM's layer to the default value.
        //26: Extremities (hand, feet, head), 29: Body (torso, limbs)
        private const int LegacyDefaultCollisionMask = 1 << 0;
        private const int DefaultCollisionMask =
            (1 << 0) |
            (1 << 26) |
            (1 << 29);

        [SerializeField]
        private LayerMask m_CollisionLayers = DefaultCollisionMask;
        public LayerMask collisionLayers { get { return m_CollisionLayers; } set { m_CollisionLayers = value; } }

        [SerializeField]
        private List<Collider> m_ExtraColliders = new List<Collider>();
        public List<Collider> extraColliders { get { return m_ExtraColliders; } }

        [SerializeField]
        private float m_Radius = 0;
        public float radius { get { return m_Radius; } set { m_Radius = Mathf.Max(0f, value); } }

        [SerializeField, EZCurveRect(0, 0, 1, 1)]
        private AnimationCurve m_RadiusCurve = AnimationCurve.Linear(0, 1, 1, 1);
        public AnimationCurve radiusCurve { get { return m_RadiusCurve; } }

        [SerializeField]
        private bool m_UseCompoundBoneColliders = true;
        public bool useCompoundBoneColliders { get { return m_UseCompoundBoneColliders; } set { m_UseCompoundBoneColliders = value; } }

        [SerializeField]
        private bool m_CollideWithNativeColliders = false;
        public bool collideWithNativeColliders { get { return m_CollideWithNativeColliders; } set { m_CollideWithNativeColliders = value; } }

        [SerializeField, Range(1, 256)]
        private int m_NativeColliderBufferSize = 64;
        public int nativeColliderBufferSize
        {
            get { return m_NativeColliderBufferSize; }
            set
            {
                m_NativeColliderBufferSize = Mathf.Clamp(value, 1, 256);
                EnsureNativeColliderBuffer();
            }
        }

        [SerializeField, Range(0f, 0.01f)]
        private float m_NativeCollisionSkin = 0.001f;

        public float nativeCollisionSkin
        {
            get { return m_NativeCollisionSkin; }
            set { m_NativeCollisionSkin = Mathf.Max(0f, value); }
        }

        [SerializeField]
        private bool m_SyncNativeColliderTransforms = true;

        private static int s_LastPhysicsSyncFrame = -1;

        [SerializeField, Range(0f, 1f)]
        private float m_CollisionFeedback = 0.35f;
        public float collisionFeedback { get { return m_CollisionFeedback; } set { m_CollisionFeedback = Mathf.Clamp01(value); } }

        [SerializeField, Range(0f, 1f)]
        private float m_CollisionFeedbackFalloff = 0.55f;
        public float collisionFeedbackFalloff { get { return m_CollisionFeedbackFalloff; } set { m_CollisionFeedbackFalloff = Mathf.Clamp01(value); } }

        [SerializeField, Range(0, 8)]
        private int m_CollisionFeedbackDepth = 3;
        public int collisionFeedbackDepth { get { return m_CollisionFeedbackDepth; } set { m_CollisionFeedbackDepth = Mathf.Max(0, value); } }

        [SerializeField, Range(0f, 1f)]
        private float m_BackwardConstraintStrength = 0.35f;
        public float backwardConstraintStrength { get { return m_BackwardConstraintStrength; } set { m_BackwardConstraintStrength = Mathf.Clamp01(value); } }

        [SerializeField, Range(0, 4)]
        private int m_BackwardConstraintPasses = 1;
        public int backwardConstraintPasses { get { return m_BackwardConstraintPasses; } set { m_BackwardConstraintPasses = Mathf.Max(0, value); } }

        [SerializeField, Range(0f, 1f)]
        private float m_ChildOrientationFollow = 0.75f;
        public float childOrientationFollow { get { return m_ChildOrientationFollow; } set { m_ChildOrientationFollow = Mathf.Clamp01(value); } }

        [SerializeField, Range(0f, 1f)]
        private float m_CollisionChildOrientationFollow = 0.75f;
        public float collisionChildOrientationFollow { get { return m_CollisionChildOrientationFollow; } set { m_CollisionChildOrientationFollow = Mathf.Clamp01(value); } }

        [SerializeField, Range(0f, 1f)]
        private float m_CollisionChildOrientationFalloff = 0.9f;
        public float collisionChildOrientationFalloff { get { return m_CollisionChildOrientationFalloff; } set { m_CollisionChildOrientationFalloff = Mathf.Clamp01(value); } }

        [SerializeField, Range(0, 16)]
        private int m_CollisionChildOrientationDepth = 8;
        public int collisionChildOrientationDepth { get { return m_CollisionChildOrientationDepth; } set { m_CollisionChildOrientationDepth = Mathf.Max(0, value); } }
        #endregion

        #region Performance
        [SerializeField]
        private DeltaTimeMode m_DeltaTimeMode = DeltaTimeMode.DeltaTime;
        public DeltaTimeMode deltaTimeMode { get { return m_DeltaTimeMode; } set { m_DeltaTimeMode = value; } }
        [SerializeField]
        private float m_ConstantDeltaTime = 0.03f;
        public float constantDeltaTime { get { return m_ConstantDeltaTime; } set { m_ConstantDeltaTime = value; } }

        [SerializeField, Range(1, 10)]
        private int m_Iterations = 1;
        public int iterations { get { return m_Iterations; } set { m_Iterations = value; } }

        [SerializeField]
        private float m_SleepThreshold = 0.005f;
        public float sleepThreshold { get { return m_SleepThreshold; } set { m_SleepThreshold = Mathf.Max(0, value); } }
        #endregion

        #region Gravity
        [SerializeField]
        private Transform m_GravityAligner;
        public Transform gravityAligner { get { return m_GravityAligner; } set { m_GravityAligner = value; } }
        [SerializeField]
        private Vector3 m_Gravity;
        public Vector3 gravity { get { return m_Gravity; } set { m_Gravity = value; } }
        #endregion

        #region Force
        [SerializeField]
        private EZSoftBoneForceField m_ForceModule;
        public EZSoftBoneForceField forceModule { get { return m_ForceModule; } set { m_ForceModule = value; } }
        [SerializeField]
        private float m_ForceScale = 1;
        public float forceScale { get { return m_ForceScale; } set { m_ForceScale = value; } }
        #endregion

        #region References
        [SerializeField]
        private Transform m_SimulateSpace;
        public Transform simulateSpace { get { return m_SimulateSpace; } set { m_SimulateSpace = value; } }
        #endregion

        #region Render Pose Latch

        [SerializeField]
        private bool m_RenderPoseLatch = true;

        public bool renderPoseLatch
        {
            get { return m_RenderPoseLatch; }
            set { m_RenderPoseLatch = value; }
        }

        private bool m_HasSolvedPose;
        private int m_RenderLatchDepth;

        #endregion

        public float globalRadius { get; private set; }
        public Vector3 globalForce { get; private set; }

        public CustomForce customForce;

        private List<Bone> m_Structures = new List<Bone>();
        private Collider[] m_NativeColliderBuffer = new Collider[64];

        //private void OnCameraPreCull(Camera camera)
        //{
        //    ApplyCachedSolvedTransforms();
        //}

        private void Awake()
        {
            //Techwoof: Let's change default assets to VAM's layers
            EnsureRequiredCollisionLayers();
            EnsureNativeColliderBuffer();
            InitStructures();
        }
        private void OnEnable()
        {
            SetRestState();
            Camera.onPreCull += OnCameraPreCull;

            // Gives the latch a valid initial pose before the first LateUpdate.
            CacheSolvedTransforms();
        }
        private void Update()
        {
            RevertTransforms(startDepth);
        }
        private void LateUpdate()
        {
            //RevertTransforms(startDepth);

            SyncNativeColliderTransforms();
            switch (deltaTimeMode)
            {
                case DeltaTimeMode.DeltaTime:
                    UpdateStructures(Time.deltaTime);
                    break;
                case DeltaTimeMode.UnscaledDeltaTime:
                    UpdateStructures(Time.unscaledDeltaTime);
                    break;
                case DeltaTimeMode.Constant:
                    UpdateStructures(constantDeltaTime);
                    break;
            }
            UpdateTransforms();
           
        }
        private void OnDisable()
        {
            ForceReleaseRenderPoseLatch();
            UnregisterRenderPoseCallbacks();
            RevertTransforms(startDepth);
        }

        private void OnDestroy()
        {
            ForceReleaseRenderPoseLatch();
            UnregisterRenderPoseCallbacks();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_StartDepth = Mathf.Max(0, m_StartDepth);
            m_ConstantDeltaTime = Mathf.Max(DeltaTime_Min, m_ConstantDeltaTime);
            m_Iterations = Mathf.Max(1, m_Iterations);
            m_SleepThreshold = Mathf.Max(0, m_SleepThreshold);
            m_Radius = Mathf.Max(0, m_Radius);
            m_NativeColliderBufferSize = Mathf.Clamp(m_NativeColliderBufferSize, 1, 256);
            EnsureRequiredCollisionLayers(); //Techwoof: Add VAMs default layers
            EnsureNativeColliderBuffer();
            m_CollisionFeedback = Mathf.Clamp01(m_CollisionFeedback);
            m_CollisionFeedbackFalloff = Mathf.Clamp01(m_CollisionFeedbackFalloff);
            m_CollisionFeedbackDepth = Mathf.Max(0, m_CollisionFeedbackDepth);
            m_BackwardConstraintStrength = Mathf.Clamp01(m_BackwardConstraintStrength);
            m_BackwardConstraintPasses = Mathf.Max(0, m_BackwardConstraintPasses);
            m_ChildOrientationFollow = Mathf.Clamp01(m_ChildOrientationFollow);
            m_CollisionChildOrientationFollow = Mathf.Clamp01(m_CollisionChildOrientationFollow);
            m_CollisionChildOrientationFalloff = Mathf.Clamp01(m_CollisionChildOrientationFalloff);
            m_CollisionChildOrientationDepth = Mathf.Max(0, m_CollisionChildOrientationDepth);
        }
        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            if (!Application.isPlaying)
            {
                InitStructures();
            }

            for (int i = 0; i < m_Structures.Count; i++)
            {
                DrawBoneGizmos(m_Structures[i]);
            }

            if (forceModule != null)
            {
                forceModule.DrawGizmos();
            }
        }
        private void DrawBoneGizmos(Bone bone)
        {
            for (int i = 0; i < bone.childBones.Count; i++)
            {
                DrawBoneGizmos(bone.childBones[i]);
            }

            Gizmos.color = Color.Lerp(Color.white, Color.red, bone.normalizedLength);
            if (bone.parentBone != null)
                Gizmos.DrawLine(bone.worldPosition, bone.parentBone.worldPosition);
            if (bone.depth > startDepth && (!m_UseCompoundBoneColliders || !HasActiveCollisionProxies(bone)))
                Gizmos.DrawWireSphere(bone.worldPosition, bone.radius);
            if (siblingConstraints != UnificationMode.None)
            {
                if (bone.leftBone != null)
                    Gizmos.DrawLine(bone.leftBone.worldPosition, bone.worldPosition);
                if (bone.rightBone != null)
                    Gizmos.DrawLine(bone.rightBone.worldPosition, bone.worldPosition);
            }
        }
#endif

        public void RevertTransforms()
        {
            RevertTransforms(startDepth);
        }
        public void RevertTransforms(int startDepth)
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].RevertTransforms(startDepth);
            }
        }
        public void InitStructures()
        {
            CreateBones();
            SetSiblings();
            SetTreeLength();
            RefreshRadius();
        }
        public void SetRestState()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].SetRestState();
            }
        }

        private void CreateBones()
        {
            m_Structures.Clear();
            if (rootBones == null || rootBones.Count == 0) return;
            for (int i = 0; i < rootBones.Count; i++)
            {
                if (rootBones[i] == null) continue;
                Bone bone = new Bone(simulateSpace, rootBones[i], endBones, startDepth, 0, 0, 0);
                m_Structures.Add(bone);
            }
        }
        private void SetSiblings()
        {
            if (siblingConstraints == UnificationMode.Rooted)
            {
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    Queue<Bone> bones = new Queue<Bone>();
                    bones.Enqueue(m_Structures[i]);
                    SetSiblingsByDepth(bones, closedSiblings);
                }
            }
            else if (siblingConstraints == UnificationMode.Unified)
            {
                Queue<Bone> bones = new Queue<Bone>();
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    bones.Enqueue(m_Structures[i]);
                }
                if (bones.Count > 0) SetSiblingsByDepth(bones, closedSiblings);
            }
        }
        private void SetSiblingsByDepth(Queue<Bone> bones, bool closed)
        {
            Bone first = bones.Dequeue();
            for (int i = 0; i < first.childBones.Count; i++)
            {
                bones.Enqueue(first.childBones[i]);
            }
            Bone left = first;
            Bone right = null;
            while (bones.Count > 0)
            {
                right = bones.Dequeue();
                for (int i = 0; i < right.childBones.Count; i++)
                {
                    bones.Enqueue(right.childBones[i]);
                }
                if (left.depth == right.depth)
                {
                    // same depth
                    left.SetRightSibling(right);
                    right.SetLeftSibling(left);
                }
                else
                {
                    // connect the last node to the first of this tier
                    if (closed)
                    {
                        left.SetRightSibling(first);
                        first.SetLeftSibling(left);
                    }
                    // next depth
                    first = right;
                }
                left = right;
            }
            // connect the last node to the first of the last tier
            if (right != null && closed)
            {
                first.SetLeftSibling(right);
                right.SetRightSibling(first);
            }
        }
        private void SetTreeLength()
        {
            if (lengthUnification == UnificationMode.Rooted)
            {
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    m_Structures[i].SetTreeLength();
                }
            }
            else if (lengthUnification == UnificationMode.Unified)
            {
                float maxLength = 0;
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    maxLength = Mathf.Max(maxLength, m_Structures[i].treeLength);
                }
                for (int i = 0; i < m_Structures.Count; i++)
                {
                    m_Structures[i].SetTreeLength(maxLength);
                }
            }
        }
        public void RefreshRadius()
        {
            globalRadius = transform.lossyScale.Abs().Max() * radius;
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].Inflate(globalRadius, radiusCurve);
            }
        }

        public void RefreshCollisionProxies()
        {
            if (m_Structures.Count == 0)
            {
                InitStructures();
                return;
            }

            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].RefreshCollisionProxies();
            }
        }

        private void UpdateStructures(float deltaTime)
        {
            if (deltaTime <= DeltaTime_Min) return;

            // radius
            globalRadius = transform.lossyScale.Abs().Max() * radius;

            // parameters
            for (int j = 0; j < m_Structures.Count; j++)
            {
                m_Structures[j].Inflate(globalRadius, radiusCurve, sharedMaterial);
                if (simulateSpace != null) m_Structures[j].UpdateSpace();
            }

            // force
            globalForce = gravity;
            if (gravityAligner != null)
            {
                Vector3 alignedDir = gravityAligner.TransformDirection(gravity).normalized;
                Vector3 globalDir = gravity.normalized;
                float attenuation = Mathf.Acos(Vector3.Dot(alignedDir, globalDir)) / Mathf.PI;
                globalForce *= attenuation;
            }

            deltaTime /= iterations;
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < m_Structures.Count; j++)
                {
                    UpdateBones(m_Structures[j], deltaTime);
                }

                for (int pass = 0; pass < m_BackwardConstraintPasses; pass++)
                {
                    for (int j = 0; j < m_Structures.Count; j++)
                    {
                        SolveBackwardConstraints(m_Structures[j], deltaTime);
                    }
                }
            }
        }
        private void UpdateBones(Bone bone, float deltaTime)
        {
            if (bone.depth > startDepth)
            {
                Vector3 oldWorldPosition, newWorldPosition, expectedPosition;
                oldWorldPosition = newWorldPosition = bone.worldPosition;

                // Resistance (force resistance)
                Vector3 force = globalForce;
                if (forceModule != null && forceModule.isActiveAndEnabled)
                {
                    force += forceModule.GetForce(bone.normalizedLength) * forceScale;
                }
                if (customForce != null)
                {
                    force += customForce(bone.normalizedLength);
                }
                force.x *= transform.localScale.x;
                force.y *= transform.localScale.y;
                force.z *= transform.localScale.z;
                bone.speed += force * (1 - bone.resistance) / iterations;

                // Damping (inertia attenuation)
                bone.speed *= 1 - bone.damping;
                if (bone.speed.sqrMagnitude > sleepThreshold)
                {
                    newWorldPosition += bone.speed * deltaTime;
                }

                // Stiffness (shape keeper)
                // Techwfoof: Original EZSoftBone used the parent's rest Transform as the stiffness target.
                // That works for very soft chains, but rigid chains can keep aiming at the original rest pose
                // after a collision bends an upstream segment. Blend toward the simulated parent-segment
                // orientation so child bones inherit the collided bone's direction.
                expectedPosition = GetStiffnessExpectedPosition(bone);
                newWorldPosition = Vector3.Lerp(newWorldPosition, expectedPosition, bone.stiffness / iterations);

                // Slackness (length keeper)
                // Length needs to be calculated with TransformVector to match runtime scaling
                Vector3 dirToParent = (newWorldPosition - bone.parentBone.worldPosition).normalized;
                float lengthToParent = bone.parentBone.transform.TransformVector(bone.localPosition).magnitude;
                expectedPosition = bone.parentBone.worldPosition + dirToParent * lengthToParent;
                int lengthConstraints = 1;
                // Sibling constraints
                if (siblingConstraints != UnificationMode.None)
                {
                    if (bone.leftBone != null)
                    {
                        Vector3 dirToLeft = (newWorldPosition - bone.leftBone.worldPosition).normalized;
                        float lengthToLeft = bone.transform.TransformVector(bone.leftPosition).magnitude;
                        expectedPosition += bone.leftBone.worldPosition + dirToLeft * lengthToLeft;
                        lengthConstraints++;
                    }
                    if (bone.rightBone != null)
                    {
                        Vector3 dirToRight = (newWorldPosition - bone.rightBone.worldPosition).normalized;
                        float lengthToRight = bone.transform.TransformVector(bone.rightPosition).magnitude;
                        expectedPosition += bone.rightBone.worldPosition + dirToRight * lengthToRight;
                        lengthConstraints++;
                    }
                }
                expectedPosition /= lengthConstraints;
                newWorldPosition = Vector3.Lerp(expectedPosition, newWorldPosition, bone.slackness / iterations);

                // Collision
                ResolveCollision(bone, ref newWorldPosition, deltaTime);

                bone.speed = (bone.speed + (newWorldPosition - oldWorldPosition) / deltaTime) * 0.5f;
                bone.worldPosition = newWorldPosition;
            }
            else
            {
                bone.worldPosition = bone.transform.position;
            }

            for (int i = 0; i < bone.childBones.Count; i++)
            {
                UpdateBones(bone.childBones[i], deltaTime);
            }
        }

        private void ResolveCollision(Bone bone, ref Vector3 newWorldPosition, float deltaTime)
        {
            bool useCompound = m_UseCompoundBoneColliders && HasActiveCollisionProxies(bone);
            if (!useCompound && bone.radius <= 0f) return;

            Vector3 positionBeforeCollision = newWorldPosition;

            if (useCompound)
            {
                ResolveCompoundCollision(bone, ref newWorldPosition);
            }
            else
            {
                ResolveCollisionPoint(bone, ref newWorldPosition, bone.radius);
            }

            Vector3 collisionCorrection = newWorldPosition - positionBeforeCollision;
            if (collisionCorrection.sqrMagnitude <= 1e-10f) return;

            // Rotate descendants while the parent is still at its pre-feedback position.
            ApplyCollisionOrientationToChildren(bone, positionBeforeCollision, newWorldPosition, deltaTime);
            ApplyCollisionFeedbackToParents(bone, collisionCorrection, deltaTime);
        }

        private bool HasActiveCollisionProxies(Bone bone)
        {
            if (bone == null || bone.collisionProxies == null) return false;

            for (int i = 0; i < bone.collisionProxies.Length; i++)
            {
                EZSoftBoneBoneCollider proxy = bone.collisionProxies[i];
                if (proxy == null) continue;
                if (!proxy.enabled || !proxy.gameObject.activeInHierarchy) continue;
                if (proxy.radius <= 0f || proxy.weight <= 0f) continue;
                return true;
            }

            return false;
        }

        private void ResolveCompoundCollision(Bone bone, ref Vector3 newWorldPosition)
        {
            EZSoftBoneBoneCollider[] proxies = bone.collisionProxies;
            if (proxies == null || proxies.Length == 0) return;

            Vector3 candidateBonePosition = newWorldPosition;

            for (int proxyIndex = 0; proxyIndex < proxies.Length; proxyIndex++)
            {
                EZSoftBoneBoneCollider proxy = proxies[proxyIndex];
                if (proxy == null) continue;
                if (!proxy.enabled || !proxy.gameObject.activeInHierarchy) continue;
                if (proxy.radius <= 0f || proxy.weight <= 0f) continue;

                int sampleCount = proxy.shape == EZSoftBoneBoneCollider.Shape.Capsule
                    ? Mathf.Max(2, proxy.capsuleSamples)
                    : 1;

                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    float sampleT = sampleCount <= 1 ? 0.5f : sampleIndex / (float)(sampleCount - 1);
                    float sampleRadius;
                    Vector3 samplePosition = GetProxySamplePosition(
                        bone,
                        proxy,
                        candidateBonePosition,
                        sampleT,
                        out sampleRadius);

                    if (sampleRadius <= 0f) continue;

                    Vector3 resolvedPosition = samplePosition;
                    ResolveCollisionPoint(bone, ref resolvedPosition, sampleRadius);

                    Vector3 correction = (resolvedPosition - samplePosition) * proxy.weight;
                    if (correction.sqrMagnitude <= 1e-10f) continue;

                    // Sequential projection keeps all proxies attached to the same simulated bone
                    // and lets later samples account for corrections already produced this frame.
                    candidateBonePosition += correction;
                }
            }

            newWorldPosition = candidateBonePosition;
        }

        private Vector3 GetProxySamplePosition(
            Bone bone,
            EZSoftBoneBoneCollider proxy,
            Vector3 candidateBonePosition,
            float sampleT,
            out float worldRadius)
        {
            Quaternion simulationDelta = GetBoneSimulationDelta(bone, candidateBonePosition);
            Vector3 centerOffset = simulationDelta * bone.transform.TransformVector(proxy.localCenter);
            Vector3 center = candidateBonePosition + centerOffset;

            float maxScale = bone.transform.lossyScale.Abs().Max();
            worldRadius = Mathf.Max(0f, proxy.radius) * maxScale;

            if (proxy.shape != EZSoftBoneBoneCollider.Shape.Capsule)
                return center;

            Vector3 axis = GetProxyWorldAxis(bone, proxy, candidateBonePosition, simulationDelta);
            if (axis.sqrMagnitude <= 1e-10f)
                axis = Vector3.up;
            else
                axis.Normalize();

            float worldHeight = Mathf.Max(proxy.height * maxScale, worldRadius * 2f);
            float halfLineLength = Mathf.Max(0f, worldHeight * 0.5f - worldRadius);

            Vector3 endpoint0 = center - axis * halfLineLength;
            Vector3 endpoint1 = center + axis * halfLineLength;
            return Vector3.Lerp(endpoint0, endpoint1, Mathf.Clamp01(sampleT));
        }

        private Quaternion GetBoneSimulationDelta(Bone bone, Vector3 candidateBonePosition)
        {
            if (bone == null || bone.parentBone == null)
                return Quaternion.identity;

            Vector3 restDirection = bone.parentBone.transform.TransformVector(bone.localPosition);
            Vector3 simulatedDirection = candidateBonePosition - bone.parentBone.worldPosition;

            if (restDirection.sqrMagnitude <= 1e-10f || simulatedDirection.sqrMagnitude <= 1e-10f)
                return Quaternion.identity;

            return Quaternion.FromToRotation(restDirection, simulatedDirection);
        }

        private Vector3 GetProxyWorldAxis(
            Bone bone,
            EZSoftBoneBoneCollider proxy,
            Vector3 candidateBonePosition,
            Quaternion simulationDelta)
        {
            if (proxy.axis == EZSoftBoneBoneCollider.Axis.AutoBone)
            {
                // Prefer the bone-to-child direction because that is the visual direction of
                // most imported bone chains. Fall back to parent-to-bone for terminal bones.
                if (bone.childBones.Count > 0)
                {
                    Vector3 childDirection = bone.childBones[0].worldPosition - candidateBonePosition;
                    if (childDirection.sqrMagnitude > 1e-10f)
                        return childDirection.normalized;
                }

                if (bone.parentBone != null)
                {
                    Vector3 parentDirection = candidateBonePosition - bone.parentBone.worldPosition;
                    if (parentDirection.sqrMagnitude > 1e-10f)
                        return parentDirection.normalized;
                }

                return simulationDelta * bone.transform.up;
            }

            Vector3 localAxis;
            switch (proxy.axis)
            {
                case EZSoftBoneBoneCollider.Axis.X:
                    localAxis = Vector3.right;
                    break;
                case EZSoftBoneBoneCollider.Axis.Z:
                    localAxis = Vector3.forward;
                    break;
                default:
                    localAxis = Vector3.up;
                    break;
            }

            return simulationDelta * bone.transform.TransformDirection(localAxis);
        }

        private void ResolveCollisionPoint(Bone bone, ref Vector3 position, float spacing)
        {
            if (spacing <= 0f) return;

            foreach (EZSoftBoneColliderBase collider in EZSoftBoneColliderBase.EnabledColliders)
            {
                if (collider == null || !collider.isActiveAndEnabled) continue;
                if (bone.transform == collider.transform) continue;
                if (!collisionLayers.Contains(collider.gameObject.layer)) continue;

                collider.Collide(ref position, spacing);
            }

            if (extraColliders != null)
            {
                for (int i = 0; i < extraColliders.Count; i++)
                {
                    Collider collider = extraColliders[i];
                    if (collider == null || !collider.enabled) continue;
                    if (bone.transform == collider.transform) continue;

                    EZSoftBoneUtility.PointOutsideCollider(ref position, collider, spacing);
                }
            }

            ResolveNativeColliderCollision(bone, ref position, spacing);
        }

        private void ResolveNativeColliderCollision(Bone bone, ref Vector3 position, float spacing)
        {
            if (!m_CollideWithNativeColliders) return;
            if (spacing <= 0f) return;

            EnsureNativeColliderBuffer();

            int hitCount = Physics.OverlapSphereNonAlloc(
                position,
                spacing,
                m_NativeColliderBuffer,
                collisionLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = m_NativeColliderBuffer[i];

                if (collider == null) continue;
                if (!collider.enabled || collider.isTrigger) continue;
                if (bone.transform == collider.transform) continue;
                if (extraColliders != null && extraColliders.Contains(collider)) continue;

                //EZSoftBoneUtility.PointOutsideCollider(ref position, collider, spacing);
                ResolveNativeColliderPoint(ref position, collider, spacing);
            }

            for (int i = 0; i < hitCount; i++)
            {
                m_NativeColliderBuffer[i] = null;
            }
        }

        private void EnsureNativeColliderBuffer()
        {
            int requiredSize = Mathf.Clamp(m_NativeColliderBufferSize, 1, 256);
            if (m_NativeColliderBuffer == null || m_NativeColliderBuffer.Length != requiredSize)
            {
                m_NativeColliderBuffer = new Collider[requiredSize];
            }
        }

        private void EnsureRequiredCollisionLayers()
        {
            // Upgrade untouched legacy components without overwriting custom masks.
            if (m_CollisionLayers.value == LegacyDefaultCollisionMask)
            {
                m_CollisionLayers = DefaultCollisionMask;
            }
        }

        private Vector3 GetStiffnessExpectedPosition(Bone bone)
        {
            Bone parent = bone.parentBone;

            Vector3 parentMovement = parent.worldPosition - parent.transform.position;
            Vector3 restExpectedPosition = parent.transform.TransformPoint(bone.localPosition) + parentMovement;

            if (m_ChildOrientationFollow <= 0f)
                return restExpectedPosition;

            if (parent.parentBone == null)
                return restExpectedPosition;

            Vector3 parentRestDirection = parent.parentBone.transform.TransformVector(parent.localPosition);
            Vector3 parentCurrentDirection = parent.worldPosition - parent.parentBone.worldPosition;

            if (parentRestDirection.sqrMagnitude <= 1e-10f || parentCurrentDirection.sqrMagnitude <= 1e-10f)
                return restExpectedPosition;

            Quaternion parentRotationDelta = Quaternion.FromToRotation(parentRestDirection, parentCurrentDirection);
            Vector3 childRestOffset = parent.transform.TransformVector(bone.localPosition);
            Vector3 inheritedExpectedPosition = parent.worldPosition + parentRotationDelta * childRestOffset;

            return Vector3.Lerp(restExpectedPosition, inheritedExpectedPosition, m_ChildOrientationFollow);
        }

        private void ApplyCollisionFeedbackToParents(Bone bone, Vector3 correction, float deltaTime)
        {
            if (correction.sqrMagnitude < 1e-10f) return;
            if (m_CollisionFeedback <= 0f || m_CollisionFeedbackDepth <= 0) return;

            Bone parent = bone.parentBone;
            float weight = m_CollisionFeedback;
            int depth = 0;

            while (parent != null && parent.depth > startDepth && depth < m_CollisionFeedbackDepth)
            {
                Vector3 parentCorrection = correction * weight;
                parent.worldPosition += parentCorrection;

                if (deltaTime > DeltaTime_Min)
                {
                    parent.speed += parentCorrection / deltaTime;
                }

                weight *= m_CollisionFeedbackFalloff;
                parent = parent.parentBone;
                depth++;
            }
        }

        private void ApplyCollisionOrientationToChildren(Bone bone, Vector3 positionBeforeCollision, Vector3 positionAfterCollision, float deltaTime)
        {
            if (m_CollisionChildOrientationFollow <= 0f) return;
            if (m_CollisionChildOrientationDepth <= 0) return;
            if (bone.parentBone == null) return;
            if (bone.childBones.Count == 0) return;

            Vector3 oldSegment = positionBeforeCollision - bone.parentBone.worldPosition;
            Vector3 newSegment = positionAfterCollision - bone.parentBone.worldPosition;

            if (oldSegment.sqrMagnitude <= 1e-10f || newSegment.sqrMagnitude <= 1e-10f) return;

            Quaternion collisionRotationDelta = Quaternion.FromToRotation(oldSegment, newSegment);

            for (int i = 0; i < bone.childBones.Count; i++)
            {
                ApplyCollisionOrientationToChildRecursive(
                    bone.childBones[i],
                    positionBeforeCollision,
                    positionAfterCollision,
                    collisionRotationDelta,
                    m_CollisionChildOrientationFollow,
                    0,
                    deltaTime);
            }
        }

        private void ApplyCollisionOrientationToChildRecursive(Bone child, Vector3 oldPivot, Vector3 newPivot, Quaternion rotationDelta, float weight, int depth, float deltaTime)
        {
            if (child == null) return;
            if (weight <= 0f) return;
            if (depth >= m_CollisionChildOrientationDepth) return;

            Vector3 desiredPosition = newPivot + rotationDelta * (child.worldPosition - oldPivot);
            Vector3 correction = (desiredPosition - child.worldPosition) * weight;

            child.worldPosition += correction;

            if (deltaTime > DeltaTime_Min)
            {
                child.speed += correction / deltaTime;
            }

            float childWeight = weight * m_CollisionChildOrientationFalloff;
            for (int i = 0; i < child.childBones.Count; i++)
            {
                ApplyCollisionOrientationToChildRecursive(child.childBones[i], oldPivot, newPivot, rotationDelta, childWeight, depth + 1, deltaTime);
            }
        }

        private void SolveBackwardConstraints(Bone bone, float deltaTime)
        {
            for (int i = 0; i < bone.childBones.Count; i++)
            {
                SolveBackwardConstraints(bone.childBones[i], deltaTime);
            }

            if (m_BackwardConstraintStrength <= 0f) return;
            if (bone.parentBone == null) return;
            if (bone.parentBone.depth <= startDepth) return;

            Bone parent = bone.parentBone;
            Vector3 parentToChild = bone.worldPosition - parent.worldPosition;
            float currentLength = parentToChild.magnitude;
            if (currentLength <= 1e-6f) return;

            float restLength = parent.transform.TransformVector(bone.localPosition).magnitude;
            Vector3 desiredParentPosition = bone.worldPosition - parentToChild / currentLength * restLength;
            Vector3 correction = (desiredParentPosition - parent.worldPosition) * m_BackwardConstraintStrength;

            parent.worldPosition += correction;

            if (deltaTime > DeltaTime_Min)
            {
                parent.speed += correction / deltaTime;
            }
        }

        private void UpdateTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].UpdateTransform(siblingRotationConstraints, startDepth);
            }
            CacheSolvedTransforms();
        }

        private static void ResolveNativeColliderPoint(ref Vector3 position, Collider collider, float spacing)
        {
            SphereCollider sphereCollider = collider as SphereCollider;
            if (sphereCollider != null)
            {
                EZSoftBoneUtility.PointOutsideSphere(
                    ref position,
                    sphereCollider,
                    spacing);

                return;
            }

            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            if (capsuleCollider != null)
            {
                EZSoftBoneUtility.PointOutsideCapsule(
                    ref position,
                    capsuleCollider,
                    spacing);

                return;
            }

            BoxCollider boxCollider = collider as BoxCollider;
            if (boxCollider != null)
            {
                EZSoftBoneUtility.PointOutsideBox(
                    ref position,
                    boxCollider,
                    spacing);

                return;
            }

            // MeshCollider and other unsupported collider types.
            EZSoftBoneUtility.PointOutsideCollider(
                ref position,
                collider,
                spacing);
        }

        private void SyncNativeColliderTransforms()
        {
            if (!m_CollideWithNativeColliders)
                return;

            if (!m_SyncNativeColliderTransforms)
                return;

            if (s_LastPhysicsSyncFrame == Time.frameCount)
                return;

            Physics.SyncTransforms();
            s_LastPhysicsSyncFrame = Time.frameCount;
        }

        private void CacheSolvedTransforms()
        {
            m_HasSolvedPose = false;

            for (int i = 0; i < m_Structures.Count; i++)
            {
                if (m_Structures[i].CacheSolvedPose(startDepth))
                {
                    m_HasSolvedPose = true;
                }
            }
        }

        private void BackupRenderTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].BackupRenderPose(startDepth);
            }
        }

        private void ApplySolvedRenderTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].ApplySolvedPose(startDepth);
            }
        }

        private void RestoreRenderTransforms()
        {
            for (int i = 0; i < m_Structures.Count; i++)
            {
                m_Structures[i].RestoreRenderPose(startDepth);
            }
        }
        private void RegisterRenderPoseCallbacks()
        {
            // Remove first to prevent accidental duplicate registration.
            Camera.onPreCull -= OnCameraPreCull;
            Camera.onPreRender -= OnCameraPreRender;
            Camera.onPostRender -= OnCameraPostRender;

            Camera.onPreCull += OnCameraPreCull;
            Camera.onPreRender += OnCameraPreRender;
            Camera.onPostRender += OnCameraPostRender;
        }

        private void UnregisterRenderPoseCallbacks()
        {
            Camera.onPreCull -= OnCameraPreCull;
            Camera.onPreRender -= OnCameraPreRender;
            Camera.onPostRender -= OnCameraPostRender;
        }
        private void OnCameraPreCull(Camera camera)
        {
            if (!m_RenderPoseLatch) return;
            if (!isActiveAndEnabled) return;
            if (!m_HasSolvedPose) return;

            // Only save the normal simulation pose once.
            // Nested cameras should not overwrite the original backup.
            if (m_RenderLatchDepth == 0)
            {
                BackupRenderTransforms();
            }

            m_RenderLatchDepth++;

            // Apply the last fully solved pose before camera culling.
            ApplySolvedRenderTransforms();
        }

        private void OnCameraPreRender(Camera camera)
        {
            if (m_RenderLatchDepth <= 0) return;

            // Reapply in case another callback changed the transforms
            // between camera culling and rendering.
            ApplySolvedRenderTransforms();
        }

        private void OnCameraPostRender(Camera camera)
        {
            if (m_RenderLatchDepth <= 0) return;

            m_RenderLatchDepth--;

            // Restore only after the outermost camera has finished.
            if (m_RenderLatchDepth == 0)
            {
                RestoreRenderTransforms();
            }
        }

        private void ForceReleaseRenderPoseLatch()
        {
            if (m_RenderLatchDepth > 0)
            {
                m_RenderLatchDepth = 0;
                RestoreRenderTransforms();
            }
        }
    }
}
