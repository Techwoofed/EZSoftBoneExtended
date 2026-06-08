/* Author:          ezhex1991@outlook.com
 * CreateTime:      2019-12-12 10:54:28
 * Organization:    #ORGANIZATION#
 * Description:     
 */
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VAMEZSoftBones
{
    [CustomEditor(typeof(EZSoftBone))]
    public class EZSoftBoneInspector : Editor
    {
        private EZSoftBone softBone;

        private SerializedProperty m_RootBones;

        private SerializedProperty m_EndBones;
        private SerializedProperty m_Material;

        private SerializedProperty m_StartDepth;
        private SerializedProperty m_SiblingConstraints;
        private SerializedProperty m_ClosedSiblings;
        private SerializedProperty m_SiblingRotationConstraints;
        private SerializedProperty m_LengthUnification;

        private SerializedProperty m_CollisionLayers;
        private SerializedProperty m_ExtraColliders;
        private SerializedProperty m_Radius;
        private SerializedProperty m_RadiusCurve;
        private SerializedProperty m_UseCompoundBoneColliders;
        private SerializedProperty m_CollideWithNativeColliders;
        private SerializedProperty m_NativeColliderBufferSize;
        private SerializedProperty m_CollisionFeedback;
        private SerializedProperty m_CollisionFeedbackFalloff;
        private SerializedProperty m_CollisionFeedbackDepth;
        private SerializedProperty m_BackwardConstraintStrength;
        private SerializedProperty m_BackwardConstraintPasses;
        private SerializedProperty m_ChildOrientationFollow;
        private SerializedProperty m_CollisionChildOrientationFollow;
        private SerializedProperty m_CollisionChildOrientationFalloff;
        private SerializedProperty m_CollisionChildOrientationDepth;

        private SerializedProperty m_DeltaTimeMode;
        private SerializedProperty m_ConstantDeltaTime;
        private SerializedProperty m_Iterations;
        private SerializedProperty m_SleepThreshold;

        private SerializedProperty m_Gravity;
        private SerializedProperty m_GravityAligner;

        private SerializedProperty m_ForceModule;
        private SerializedProperty m_ForceScale;

        private SerializedProperty m_SimulateSpace;

        private void OnEnable()
        {
            softBone = target as EZSoftBone;

            m_RootBones = serializedObject.FindProperty("m_RootBones");

            m_EndBones = serializedObject.FindProperty("m_EndBones");
            m_Material = serializedObject.FindProperty("m_Material");

            m_StartDepth = serializedObject.FindProperty("m_StartDepth");
            m_SiblingConstraints = serializedObject.FindProperty("m_SiblingConstraints");
            m_ClosedSiblings = serializedObject.FindProperty("m_ClosedSiblings");
            m_SiblingRotationConstraints = serializedObject.FindProperty("m_SiblingRotationConstraints");
            m_LengthUnification = serializedObject.FindProperty("m_LengthUnification");

            m_CollisionLayers = serializedObject.FindProperty("m_CollisionLayers");
            m_ExtraColliders = serializedObject.FindProperty("m_ExtraColliders");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_RadiusCurve = serializedObject.FindProperty("m_RadiusCurve");
            m_UseCompoundBoneColliders = serializedObject.FindProperty("m_UseCompoundBoneColliders");
            m_CollideWithNativeColliders = serializedObject.FindProperty("m_CollideWithNativeColliders");
            m_NativeColliderBufferSize = serializedObject.FindProperty("m_NativeColliderBufferSize");
            m_CollisionFeedback = serializedObject.FindProperty("m_CollisionFeedback");
            m_CollisionFeedbackFalloff = serializedObject.FindProperty("m_CollisionFeedbackFalloff");
            m_CollisionFeedbackDepth = serializedObject.FindProperty("m_CollisionFeedbackDepth");
            m_BackwardConstraintStrength = serializedObject.FindProperty("m_BackwardConstraintStrength");
            m_BackwardConstraintPasses = serializedObject.FindProperty("m_BackwardConstraintPasses");
            m_ChildOrientationFollow = serializedObject.FindProperty("m_ChildOrientationFollow");
            m_CollisionChildOrientationFollow = serializedObject.FindProperty("m_CollisionChildOrientationFollow");
            m_CollisionChildOrientationFalloff = serializedObject.FindProperty("m_CollisionChildOrientationFalloff");
            m_CollisionChildOrientationDepth = serializedObject.FindProperty("m_CollisionChildOrientationDepth");

            m_DeltaTimeMode = serializedObject.FindProperty("m_DeltaTimeMode");
            m_ConstantDeltaTime = serializedObject.FindProperty("m_ConstantDeltaTime");
            m_Iterations = serializedObject.FindProperty("m_Iterations");
            m_SleepThreshold = serializedObject.FindProperty("m_SleepThreshold");

            m_Gravity = serializedObject.FindProperty("m_Gravity");
            m_GravityAligner = serializedObject.FindProperty("m_GravityAligner");

            m_ForceModule = serializedObject.FindProperty("m_ForceModule");
            m_ForceScale = serializedObject.FindProperty("m_ForceScale");

            m_SimulateSpace = serializedObject.FindProperty("m_SimulateSpace");
        }

        private void DrawRootBonesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty bone = m_RootBones.GetArrayElementAtIndex(index);
            float labelWidth = 25;
            rect.y += 1;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), index.ToString("00"));
            rect.x += labelWidth; rect.width -= labelWidth;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), bone, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as MonoBehaviour), typeof(MonoScript), false);
            GUI.enabled = true;
            serializedObject.Update();
            bool initRequired = false;

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(m_RootBones, true);
                EditorGUILayout.PropertyField(m_EndBones, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                initRequired = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_Material);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Structure", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(m_StartDepth);
                EditorGUILayout.PropertyField(m_SiblingConstraints);
                EditorGUILayout.PropertyField(m_ClosedSiblings);
                EditorGUILayout.PropertyField(m_SiblingRotationConstraints);
                EditorGUILayout.PropertyField(m_LengthUnification);
            }
            if (EditorGUI.EndChangeCheck())
            {
                initRequired = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collision", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_CollisionLayers);
            EditorGUILayout.PropertyField(m_ExtraColliders, true);

            EditorGUILayout.PropertyField(
                m_CollideWithNativeColliders,
                new GUIContent(
                    "Native Colliders",
                    "Automatically collide with enabled, non-trigger Unity colliders on Collision Layers."));

            if (m_CollideWithNativeColliders.boolValue)
            {
                EditorGUILayout.PropertyField(
                    m_NativeColliderBufferSize,
                    new GUIContent(
                        "Native Collider Buffer",
                        "Maximum native colliders returned by each sphere-sample query. This is a capacity/performance limit, not collision strength."));
            }

            EditorGUILayout.PropertyField(
                m_UseCompoundBoneColliders,
                new GUIContent(
                    "Compound Bone Colliders",
                    "Use EZSoftBoneBoneCollider components attached to bone Transforms. Multiple sphere/capsule components may be attached to the same bone. Bones without proxies keep the legacy radius sphere."));

            if (m_UseCompoundBoneColliders.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Add one or more 'EZSoftBone/Bone Collision Proxy' components to a bone Transform. " +
                    "Capsules are resolved as stable sphere samples.",
                    MessageType.Info);

                if (GUILayout.Button("Refresh Collision Proxies"))
                {
                    softBone.RefreshCollisionProxies();
                    SceneView.RepaintAll();
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_RadiusCurve);
            if (EditorGUI.EndChangeCheck())
            {
                softBone.RefreshRadius();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Collision Feedback", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_CollisionFeedback, new GUIContent("Feedback", "How much of a collision correction is pushed into parent bones."));
            EditorGUILayout.PropertyField(m_CollisionFeedbackFalloff, new GUIContent("Feedback Falloff", "Multiplier applied to each successive parent up the chain."));
            EditorGUILayout.PropertyField(m_CollisionFeedbackDepth, new GUIContent("Feedback Depth", "Maximum number of parent bones affected by a collided child."));
            EditorGUILayout.PropertyField(m_BackwardConstraintStrength, new GUIContent("Backward Constraint", "How strongly children pull parents after collision/constraint solving."));
            EditorGUILayout.PropertyField(m_BackwardConstraintPasses, new GUIContent("Backward Passes", "Extra parent-chain constraint passes per iteration."));
            EditorGUILayout.PropertyField(m_ChildOrientationFollow, new GUIContent("Child Orientation Follow", "How much stiffness targets inherit the simulated parent-segment orientation instead of the original rest-pose direction. Use high values for rigid ears/horns/tails."));
            EditorGUILayout.PropertyField(m_CollisionChildOrientationFollow, new GUIContent("Collision Child Orientation", "Immediately rotates child bones with a collided parent segment so rigid chains do not wrap around colliders."));
            EditorGUILayout.PropertyField(m_CollisionChildOrientationFalloff, new GUIContent("Child Orientation Falloff", "Multiplier applied to each deeper child during collision orientation propagation."));
            EditorGUILayout.PropertyField(m_CollisionChildOrientationDepth, new GUIContent("Child Orientation Depth", "Maximum number of descendant bones affected by collision orientation propagation."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_DeltaTimeMode);
            if (m_DeltaTimeMode.intValue == (int)EZSoftBone.DeltaTimeMode.Constant)
            {
                EditorGUILayout.PropertyField(m_ConstantDeltaTime);
            }
            EditorGUILayout.PropertyField(m_Iterations);
            EditorGUILayout.PropertyField(m_SleepThreshold);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Gravity);
            EditorGUILayout.PropertyField(m_GravityAligner);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Force", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ForceModule);
            EditorGUILayout.PropertyField(m_ForceScale);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SimulateSpace);
            if (EditorGUI.EndChangeCheck())
            {
                initRequired = true;
            }

            serializedObject.ApplyModifiedProperties();

            if (initRequired)
            {
                if (Application.isPlaying)
                {
                    softBone.RevertTransforms();
                    softBone.InitStructures();
                }
                else
                {
                    softBone.InitStructures();
                }
            }
        }
    }
}

