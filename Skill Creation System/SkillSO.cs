using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu(fileName = "BlankSkill", menuName = "Grandeur/New Skill")]
public class SkillSO : ScriptableObject
{
    public enum ranAttackTypes {Point, Line};
    public enum aoeSpawnTypes {Instant, OnDeath, OnImpact, OnHit};
    public enum objectTypes {None, Sphere, Capsule, Custom};
    public enum colliderTypes {Capsule, Box, Sphere, Mesh};
    
    public bool canUse = true;
    
    [Header("General")]
    public new string name;
    public Sprite icon;
    public float cooldown;
    public float castTime;
    public int cost;
    public bool heldSightline;
    
    [Header("Player")]
    public float modHealth;
    public float modSpeed;
    public float modDamage;
    public float modKnockback;
    public float modDuration;

    [Header("Melee")]
    public bool isMelee;
    public float melRange;
    public float melDamage;
    public float melKnockback;

    [Header("Ranged")]
    public bool isRanged;
    public bool ranInstant;
    public float ranRange;
    public float ranSpeed;
    public float ranDamage;
    public float ranKnockback;
    public int ranPiercingCount;
    public ranAttackTypes ranAttackStyle;

    [Header("Projectile")]
    public bool isProjectile;
    public float proSpeed;
    public float proDamage;
    public float proKnockback;
    public int proBounce;

    [Header("Area of Effect")]
    public bool isAOE;
    public float aoeRadius;
    public float aoeDamage;
    public float aoeDuration;
    public aoeSpawnTypes aoeSpawn;
    
    [Header("Visuals")]
    public objectTypes visObjectType;
    public float visRadius;
    public float visHeight;
    public Mesh visMesh;
    public colliderTypes visCollider;
    public Vector3 visColliderCenter;
    public float visColliderRadius;
    public float visColliderHeight;
    public Vector3 visColliderSize = Vector3.one;
    public Vector3 visScale = Vector3.one;
    public Vector3 visRotation;
    public Material visMaterial;
    public Color visColor;
    public ParticleSystem visParticles;

    [Header("Audio")]
    public AudioClip sndActivate;
    public AudioClip sndHit;
    public AudioClip sndImpact;
    public AudioClip sndRunning;

    [Header("Extras")]
    public MonoScript customScript;
    public GameObject customOrigin;
    public LayerMask targetLayers;
    public LayerMask excludeLayers = 2048;
}

[CustomEditor(typeof(SkillSO))]
public class SkillSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = target as SkillSO;
        
        EditorGUI.BeginChangeCheck();
        
        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        script.name = EditorGUILayout.TextField("Name", script.name);
        script.icon = (Sprite)EditorGUILayout.ObjectField("Icon", script.icon, typeof(Sprite), false);
        script.cooldown = EditorGUILayout.FloatField("Cooldown", script.cooldown);
        script.castTime = EditorGUILayout.FloatField("[UNUSED] Cast Time", script.castTime);
        script.cost = EditorGUILayout.IntField("Cost", script.cost);
        script.heldSightline = GUILayout.Toggle(script.heldSightline, "Held Sightline");
        
        GUILayout.Space(10);
        GUILayout.Label("Ability Settings", EditorStyles.boldLabel);
        
        if (script.isMelee) { script.isMelee = GUILayout.Toggle(script.isMelee, "Melee Enabled", EditorStyles.miniButton); }
        else { script.isMelee = GUILayout.Toggle(script.isMelee, "Melee", EditorStyles.miniButton); }
        if (script.isMelee) {
            EditorGUILayout.BeginVertical("Box");
            script.melRange = EditorGUILayout.FloatField("[UNUSED] Range", script.melRange);
            script.melDamage = EditorGUILayout.FloatField("[UNUSED] Damage", script.melDamage);
            script.melKnockback = EditorGUILayout.FloatField("[UNUSED] Knockback", script.melKnockback);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        if (script.isRanged) { script.isRanged = GUILayout.Toggle(script.isRanged, "Ranged Enabled", EditorStyles.miniButton); }
        else { script.isRanged = GUILayout.Toggle(script.isRanged, "Ranged", EditorStyles.miniButton); }
        if (script.isRanged) {
            EditorGUILayout.BeginVertical("Box");
            script.ranInstant = GUILayout.Toggle(script.ranInstant, "[UNUSED] Instant Hit");
            if (!script.ranInstant) { script.ranSpeed = EditorGUILayout.FloatField("Speed", script.ranSpeed); }
            script.ranRange = EditorGUILayout.FloatField("Range", script.ranRange);
            script.ranDamage = EditorGUILayout.FloatField("[UNUSED] Damage", script.ranDamage);
            script.ranKnockback = EditorGUILayout.FloatField("[UNUSED] Knockback", script.ranKnockback);
            script.ranPiercingCount = EditorGUILayout.IntField("[UNUSED] Piercing Count", script.ranPiercingCount);
            script.ranAttackStyle = (SkillSO.ranAttackTypes)EditorGUILayout.EnumPopup("[UNUSED] Attack Style", script.ranAttackStyle);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        if (script.isProjectile) { script.isProjectile = GUILayout.Toggle(script.isProjectile, "Projectile Enabled", EditorStyles.miniButton); }
        else { script.isProjectile = GUILayout.Toggle(script.isProjectile, "Projectile", EditorStyles.miniButton); }
        if (script.isProjectile) {
            EditorGUILayout.BeginVertical("Box");
            script.proSpeed = EditorGUILayout.FloatField("[UNUSED] Speed", script.proSpeed);
            script.proDamage = EditorGUILayout.FloatField("[UNUSED] Damage", script.proDamage);
            script.proKnockback = EditorGUILayout.FloatField("[UNUSED] Knockback", script.proKnockback);
            script.proBounce = EditorGUILayout.IntField("[UNUSED] Bounce Count", script.proBounce);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        if (script.isAOE) { script.isAOE = GUILayout.Toggle(script.isAOE, "Area of Effect Enabled", EditorStyles.miniButton); }
        else { script.isAOE = GUILayout.Toggle(script.isAOE, "Area of Effect", EditorStyles.miniButton); }
        if (script.isAOE) {
            EditorGUILayout.BeginVertical("Box");
            script.aoeRadius = EditorGUILayout.FloatField("[UNUSED] Radius", script.aoeRadius);
            script.aoeDamage = EditorGUILayout.FloatField("[UNUSED] Damage", script.aoeDamage);
            script.aoeDuration = EditorGUILayout.FloatField("[UNUSED] Duration", script.aoeDuration);
            script.aoeSpawn = (SkillSO.aoeSpawnTypes)EditorGUILayout.EnumPopup("[UNUSED] Spawn Type", script.aoeSpawn);
            if ((script.aoeSpawn == SkillSO.aoeSpawnTypes.OnDeath || script.aoeSpawn == SkillSO.aoeSpawnTypes.OnImpact ||
                script.aoeSpawn == SkillSO.aoeSpawnTypes.OnHit) && !script.isMelee && !script.isRanged &&
                !script.isProjectile) {
                EditorGUILayout.HelpBox("The selected Spawn Type requires either Melee, Ranged or Projectile to be enable in order for it to be triggered.", MessageType.Error);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        if (!script.isMelee && !script.isRanged && !script.isProjectile && !script.isAOE) {
            EditorGUILayout.HelpBox("You currently don't have any of the skill categories enabled so this skill won't do anything.", MessageType.Error);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Visual Settings", EditorStyles.boldLabel);
        script.visObjectType = (SkillSO.objectTypes)EditorGUILayout.EnumPopup("Object Type", script.visObjectType);
        switch (script.visObjectType) {
            case SkillSO.objectTypes.Sphere:
                EditorGUILayout.BeginVertical("Box");
                script.visRadius = EditorGUILayout.FloatField("Radius", script.visRadius);
                script.visMaterial = (Material)EditorGUILayout.ObjectField("Material", script.visMaterial, typeof(Material), false);
                script.visColor = EditorGUILayout.ColorField("Color", script.visColor);
                EditorGUILayout.EndVertical();
                break;
            case SkillSO.objectTypes.Capsule:
                EditorGUILayout.BeginVertical("Box");
                script.visRadius = EditorGUILayout.FloatField("Radius", script.visRadius);
                script.visHeight = EditorGUILayout.FloatField("Height", script.visHeight);
                script.visMaterial = (Material)EditorGUILayout.ObjectField("Material", script.visMaterial, typeof(Material), false);
                script.visColor = EditorGUILayout.ColorField("Color", script.visColor);
                EditorGUILayout.EndVertical();
                break;
            case SkillSO.objectTypes.Custom:
                EditorGUILayout.BeginVertical("Box");
                script.visMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", script.visMesh, typeof(Mesh), false);
                script.visScale = EditorGUILayout.Vector3Field("Scale", script.visScale);
                script.visRotation = EditorGUILayout.Vector3Field("Rotation", script.visRotation);
                script.visCollider = (SkillSO.colliderTypes)EditorGUILayout.EnumPopup("Collider Type", script.visCollider);
                switch (script.visCollider) {
                    case SkillSO.colliderTypes.Capsule:
                        script.visColliderCenter = EditorGUILayout.Vector3Field("Collider Center", script.visColliderCenter);
                        script.visRadius = EditorGUILayout.FloatField("Collider Radius", script.visRadius);
                        script.visHeight = EditorGUILayout.FloatField("Collider Height", script.visHeight);
                        break;
                    case SkillSO.colliderTypes.Box:
                        script.visColliderCenter = EditorGUILayout.Vector3Field("Collider Center", script.visColliderCenter);
                        script.visColliderSize = EditorGUILayout.Vector3Field("Collider Size", script.visColliderSize);
                        break;
                    case SkillSO.colliderTypes.Sphere:
                        script.visColliderCenter = EditorGUILayout.Vector3Field("Collider Center", script.visColliderCenter);
                        script.visColliderRadius = EditorGUILayout.FloatField("Collider Radius", script.visColliderRadius);
                        break;
                }
                script.visMaterial = (Material)EditorGUILayout.ObjectField("Material", script.visMaterial, typeof(Material), false);
                script.visColor = EditorGUILayout.ColorField("Color", script.visColor);
                EditorGUILayout.EndVertical();
                break;
        }
        script.visParticles = (ParticleSystem)EditorGUILayout.ObjectField("Particles", script.visParticles, typeof(ParticleSystem), false);
        
        GUILayout.Space(10);
        GUILayout.Label("Audio Settings", EditorStyles.boldLabel);
        script.sndActivate = (AudioClip)EditorGUILayout.ObjectField("Activate Sound", script.sndActivate, typeof(AudioClip), false);
        script.sndHit = (AudioClip)EditorGUILayout.ObjectField("Hit Sound", script.sndHit, typeof(AudioClip), false);
        script.sndImpact = (AudioClip)EditorGUILayout.ObjectField("Impact Sound", script.sndImpact, typeof(AudioClip), false);
        script.sndRunning = (AudioClip)EditorGUILayout.ObjectField("Running Sound", script.sndRunning, typeof(AudioClip), false);
        
        GUILayout.Space(10);
        GUILayout.Label("Extra Settings", EditorStyles.boldLabel);
        script.customScript = (MonoScript)EditorGUILayout.ObjectField("Custom Script", script.customScript, typeof(MonoScript), false);
        script.customOrigin = (GameObject)EditorGUILayout.ObjectField("Custom Origin", script.customOrigin, typeof(GameObject), false);
        string[] layerOptions = new string[32];
        for (int i = 0; i < 32; i++) { layerOptions[i] = LayerMask.LayerToName(i); }
        while (layerOptions[layerOptions.Length - 1] == "") {
            System.Array.Resize(ref layerOptions, layerOptions.Length - 1);
        }
        script.targetLayers = EditorGUILayout.MaskField("Target Layers", script.targetLayers, layerOptions);
        script.excludeLayers = EditorGUILayout.MaskField("Exclude Layers", script.excludeLayers, layerOptions);
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(script);
        }
    }
}