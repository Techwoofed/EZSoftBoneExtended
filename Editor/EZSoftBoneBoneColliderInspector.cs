using UnityEditor;
using UnityEngine;

namespace VAMEZSoftBones
{
    [CustomEditor(typeof(EZSoftBoneBoneCollider))]
    [CanEditMultipleObjects]
    public class EZSoftBoneBoneColliderInspector : Editor
    {
        private SerializedProperty m_Shape;
        private SerializedProperty m_Axis;
        private SerializedProperty m_LocalCenter;
        private SerializedProperty m_Radius;
        private SerializedProperty m_Height;
        private SerializedProperty m_CapsuleSamples;
        private SerializedProperty m_Weight;

        private void OnEnable()
        {
            m_Shape = serializedObject.FindProperty("m_Shape");
            m_Axis = serializedObject.FindProperty("m_Axis");
            m_LocalCenter = serializedObject.FindProperty("m_LocalCenter");
            m_Radius = serializedObject.FindProperty("m_Radius");
            m_Height = serializedObject.FindProperty("m_Height");
            m_CapsuleSamples = serializedObject.FindProperty("m_CapsuleSamples");
            m_Weight = serializedObject.FindProperty("m_Weight");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Shape);
            EditorGUILayout.PropertyField(m_LocalCenter);
            EditorGUILayout.PropertyField(m_Radius);
            EditorGUILayout.PropertyField(m_Weight, new GUIContent(
                "Collision Weight",
                "How much of each collision correction is applied to the simulated bone."));

            if (m_Shape.intValue == (int)EZSoftBoneBoneCollider.Shape.Capsule)
            {
                EditorGUILayout.PropertyField(m_Axis);
                EditorGUILayout.PropertyField(m_Height);
                EditorGUILayout.PropertyField(
                    m_CapsuleSamples,
                    new GUIContent(
                        "Capsule Samples",
                        "Number of sphere samples used along the capsule. Three is usually enough; use more for long capsules."));
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Attach multiple Bone Collision Proxy components to the same bone for compound shapes. " +
                "Auto Bone follows the first child direction, then falls back to the parent segment.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
