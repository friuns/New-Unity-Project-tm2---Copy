#pragma warning disable 618
// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;
using Curvy.Utils;

//[CustomEditor(typeof(CurvySplineSegment)), CanEditMultipleObjects]
public class CurvySplineSegmentInspector : Editor {
    public static float ConstraintSplineLength;
    public static bool ConstrainXAxis;
    public static bool ConstrainYAxis;
    public static bool ConstrainZAxis;

    CurvySplineSegment Target { get { return target as CurvySplineSegment; } }

    
    bool mValid;
    SerializedProperty tSmoothTangent;
    SerializedProperty tSyncStartEnd;
    SerializedProperty tT0;
    SerializedProperty tB0;
    SerializedProperty tC0;
    SerializedProperty tT1;
    SerializedProperty tB1;
    SerializedProperty tC1;
    SerializedProperty tOT;
    SerializedProperty tOB;
    SerializedProperty tOC;

    Vector3 constraints;
    Texture2D mTexSetFirstCP;
    Texture2D mTexConstraints;


    void OnEnable()
    {
        CurvyPreferences.Get();
        constraints = Target.Position;

        tSmoothTangent = serializedObject.FindProperty("SmoothEdgeTangent");
        tSyncStartEnd = serializedObject.FindProperty("SynchronizeTCB");
        tT0 = serializedObject.FindProperty("StartTension");
        tC0 = serializedObject.FindProperty("StartContinuity");
        tB0 = serializedObject.FindProperty("StartBias");
        tT1 = serializedObject.FindProperty("EndTension");
        tC1 = serializedObject.FindProperty("EndContinuity");
        tB1 = serializedObject.FindProperty("EndBias");
        tOT = serializedObject.FindProperty("OverrideGlobalTension");
        tOC = serializedObject.FindProperty("OverrideGlobalContinuity");
        tOB = serializedObject.FindProperty("OverrideGlobalBias");

        mTexSetFirstCP = Resources.Load("curvysetfirstcp") as Texture2D;
        mTexConstraints = Resources.Load("curvyconstraints") as Texture2D;
    }
 
    void OnSceneGUI()
    {
        Handles.color = Color.yellow;
        Handles.ArrowCap(0, Target.Transform.position, Quaternion.LookRotation(Target.Transform.up), HandleUtility.GetHandleSize(Target.Transform.position)*0.7f);
        Handles.BeginGUI();
        GUILayout.Window(Target.GetInstanceID(), new Rect(10, 40, 150, 20), DoWin, Target.name);
        Handles.EndGUI();
        
        bool refreshSpline = false;

        // Position constraints
        if (ConstrainXAxis || ConstrainYAxis || ConstrainZAxis) {
            Vector3 pos = Target.Position;
            
            if (pos != constraints) {
                refreshSpline = true;
                if (ConstrainXAxis)
                    pos.x = constraints.x;
                if (ConstrainYAxis)
                    pos.y = constraints.y;
                if (ConstrainZAxis)
                    pos.z = constraints.z;

                Target.Position = pos;
                Target.Spline.Refresh(true, true, false);
            }
        }
        
        if (ConstraintSplineLength>0 && Target.Spline.Length > ConstraintSplineLength) {
            Target.Position = constraints;
            refreshSpline = true;
        }
        if (refreshSpline)
            Target.Spline.Refresh(true, true, false);

        constraints = Target.Position;

        if (Event.current.type == EventType.KeyDown) {
            if (Event.current.keyCode == KeyCode.T) {
                if (Event.current.shift && Target.PreviousControlPoint)
                    Selection.activeObject = Target.PreviousControlPoint;
                else
                    if (Target.NextControlPoint)
                        Selection.activeObject = Target.NextControlPoint;
            }
            else if (Event.current.keyCode == KeyCode.G) {
                if (Event.current.shift)
                    InsBefore();
                else
                    InsAfter();
            }
            else if (Event.current.keyCode == KeyCode.H) {
                Delete();
            }

        }
    }

    public override void OnInspectorGUI()
    {
        if (Event.current.type == EventType.Layout)
            mValid = Target.IsValidSegment;

        if (mValid && (Target.Spline.Closed || !Target.IsFirstSegment)) 
            EditorGUILayout.PropertyField(tSmoothTangent, new GUIContent("Smooth End Tangent", "Smooth end tangent?"));
        

        if (mValid && Target.Spline.Interpolation == CurvyInterpolation.TCB) {
            EditorGUILayout.PropertyField(tSyncStartEnd, new GUIContent("Synchronize TCB","Synchronize Start and End Values"));
            EditorGUILayout.PropertyField(tOT, new GUIContent("Local Tension","Override Spline Tension?"));
            if (tOT.boolValue) {
                EditorGUILayout.PropertyField(tT0, Target.SynchronizeTCB ? new GUIContent("Tension","Tension") : new GUIContent("Start Tension","Start Tension"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tT1, new GUIContent("End Tension", "End Tension"));
                else
                    tT1.floatValue = tT0.floatValue;
            }
            EditorGUILayout.PropertyField(tOC, new GUIContent("Local Continuity","Override Spline Continuity?"));
            if (tOC.boolValue) {
                EditorGUILayout.PropertyField(tC0, Target.SynchronizeTCB ? new GUIContent("Continuity", "Continuity") : new GUIContent("Start Continuity", "Start Continuity"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tC1, new GUIContent("End Continuity","End Continuity"));
                else
                    tC1.floatValue = tC0.floatValue;
            }
            EditorGUILayout.PropertyField(tOB, new GUIContent("Local Bias","Override Spline Bias?"));
            if (tOB.boolValue) {
                EditorGUILayout.PropertyField(tB0, Target.SynchronizeTCB ? new GUIContent("Bias", "Bias") : new GUIContent("Start Bias", "Start Bias"));
                if (!Target.SynchronizeTCB)
                    EditorGUILayout.PropertyField(tB1, new GUIContent("End Bias","End Bias"));
                else
                    tB1.floatValue = tB0.floatValue;
            }

            if (tOT.boolValue || tOC.boolValue || tOB.boolValue) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Set Catmul")) {
                    tT0.floatValue = 0; tC0.floatValue = 0; tB0.floatValue = 0;
                    tT1.floatValue = 0; tC1.floatValue = 0; tB1.floatValue = 0;
                }
                if (GUILayout.Button("Set Cubic")) {
                    tT0.floatValue = -1; tC0.floatValue = 0; tB0.floatValue = 0;
                    tT1.floatValue = -1; tC1.floatValue = 0; tB1.floatValue = 0;
                }
                if (GUILayout.Button("Set Linear")) {
                    tT0.floatValue = 0; tC0.floatValue = -1; tB0.floatValue = 0;
                    tT1.floatValue = 0; tC1.floatValue = -1; tB1.floatValue = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        

        if (Target.UserValues != null && Target.UserValues.Length > 0) {
            EditorGUILayout.LabelField("User Values", EditorStyles.boldLabel);
            ArrayGUI(serializedObject, "UserValues", false);
        }

        GUILayout.Label("Tools", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent(mTexConstraints, "Constraints Tools"), GUILayout.ExpandWidth(false))) {
            CurvyConstraintsWin.Create();
        }
        GUI.enabled = Target.ControlPointIndex > 0;
        if (GUILayout.Button(new GUIContent(mTexSetFirstCP, "Set as first Control Point"), GUILayout.ExpandWidth(false))) {
            Undo.RegisterSceneUndo("Set First Control Point");
            CurvyUtility.setFirstCP(Target);
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        
        if (serializedObject.targetObject && serializedObject.ApplyModifiedProperties()) {
            Target.Spline.Refresh(true,true,false);
            SceneView.RepaintAll();
        }

        if (mValid) {
            EditorGUILayout.LabelField("Segment Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Distance: " + Target.Distance);
                EditorGUILayout.LabelField("Length: " + Target.Length);
                EditorGUILayout.LabelField("Spline Length: " + Target.Spline.Length);
        }
        
    }

    void InsBefore()
    {
        Undo.RegisterSceneUndo("Insert Control Point");
        Selection.activeTransform = Target.Spline.Add(true, Target).transform;
    }

    void InsAfter()
    {
        Undo.RegisterSceneUndo("Insert Control Point");
        Selection.activeTransform = Target.Spline.Add(Target).transform;
    }

    void Delete()
    {
        Undo.RegisterSceneUndo("Delete Control Point");
        Selection.activeTransform = Target.PreviousTransform;
        Target.Delete();
    }

    void ArrayGUI(SerializedObject obj, string name, bool resizeable)
    {

        int size = obj.FindProperty(name + ".Array.size").intValue;
        int newSize=size;
        if (resizeable) {
            newSize = EditorGUILayout.IntField(" Size", size);
            if (newSize != size)
                obj.FindProperty(name + ".Array.size").intValue = newSize;
        }
        EditorGUI.indentLevel = 3;
        for (int i = 0; i < newSize; i++) {
            var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", name, i));
            EditorGUILayout.PropertyField(prop);
        }
        EditorGUI.indentLevel = 0;
    }

    void DoWin(int id)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Ins Before", "Shift-G")))
            InsBefore();

        if (GUILayout.Button(new GUIContent("Ins After", "G")))
            InsAfter();

        if (GUILayout.Button(new GUIContent("Delete", "H")))
            Delete();

        if (Target) {
            if (Target.PreviousTransform && GUILayout.Button(new GUIContent("Prev", "Shift-T")))
                Selection.activeTransform = Target.PreviousTransform;
            if (Target.NextTransform && GUILayout.Button(new GUIContent("Next", "T")))
                Selection.activeTransform = Target.NextTransform;
            if (GUILayout.Button("Spline"))
                Selection.activeTransform = Target.Spline.transform;
        }
        GUILayout.EndHorizontal();
        // TCB
        if (Target.Spline.Interpolation == CurvyInterpolation.TCB) {
            GUILayout.BeginHorizontal();
            Target.OverrideGlobalTension = GUILayout.Toggle(Target.OverrideGlobalTension, "T", GUILayout.ExpandWidth(false));
            if (Target.OverrideGlobalTension) {
                Target.StartTension = GUILayout.HorizontalSlider(Target.StartTension, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndTension = GUILayout.HorizontalSlider(Target.EndTension, -1, 1);
                else
                    Target.EndTension = Target.StartTension;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            
            Target.OverrideGlobalContinuity = GUILayout.Toggle(Target.OverrideGlobalContinuity, "C", GUILayout.ExpandWidth(false));
            
            if (Target.OverrideGlobalContinuity) {
                
                Target.StartContinuity = GUILayout.HorizontalSlider(Target.StartContinuity, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndContinuity = GUILayout.HorizontalSlider(Target.EndContinuity, -1, 1);
                else
                    Target.EndContinuity = Target.StartContinuity;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Target.OverrideGlobalBias = GUILayout.Toggle(Target.OverrideGlobalBias, "B", GUILayout.ExpandWidth(false));
            if (Target.OverrideGlobalBias) {
                Target.StartBias = GUILayout.HorizontalSlider(Target.StartBias, -1, 1);
                if (!Target.SynchronizeTCB)
                    Target.EndBias = GUILayout.HorizontalSlider(Target.EndBias, -1, 1);
                else
                    Target.EndBias = Target.StartBias;
            }
            GUILayout.EndHorizontal();
        }

        if (GUI.changed) {
            EditorUtility.SetDirty(Target);
            Target.Spline.Refresh(true, true,false);
            SceneView.RepaintAll();
            serializedObject.UpdateIfDirtyOrScript();
        }
   
    }

    
}
