using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BlankStat", menuName = "Grandeur/New Stat")]
public class StatSO : ScriptableObject
{
   [Header("General")]
   public new string name;
   public Sprite icon;
   public bool isExclusive;
}

[CustomEditor(typeof(StatSO))]
public class SkillSOEditor : Editor
{
   public override void OnInspectorGUI()
   {
      var script = target as StatSO;
        
      EditorGUI.BeginChangeCheck();
      
      GUILayout.Label("General Settings", EditorStyles.boldLabel);
      script.name = EditorGUILayout.TextField("Name", script.name);
      script.icon = (Sprite)EditorGUILayout.ObjectField("Icon", script.icon, typeof(Sprite), false);
      script.isExclusive = EditorGUILayout.Toggle("Is Exclusive", script.isExclusive);
      
      if (EditorGUI.EndChangeCheck()) {
         EditorUtility.SetDirty(script);
      }
   }
}