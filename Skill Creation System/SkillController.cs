// TODO: Overall
//     Make the material also get the opacity

using System.Collections;
using UnityEditor;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    // Public Variables
    public GameObject skillOrigin;
    
    // Private Variables
    private GameObject origin;
    private bool canUse = true;
    
    // Sightline Variables
    private GameObject sightlineRAN;
    private GameObject sightlinePRO;
    private GameObject sightlineAOE;
    
    // Raycasting Variables
    private Ray ray;
    private RaycastHit hitInfo;
    private GameObject hitObject = null;
    
    // Components
    private CapsuleCollider refCapsuleCollider;
    private BoxCollider refBoxCollider;
    private SphereCollider refSphereCollider;
    private MeshCollider refMeshCollider;
    private Rigidbody refRigidbody;
    private MeshRenderer refMeshRenderer;
    private MeshFilter refMeshFilter;
    private SkillActive refSkillActive;
    private Component refCustomScript;
    private Renderer refRenderer;
    private Collider refCollider;
    
    private IEnumerator Cooldown(SkillSO skill)
    {
        Debug.Log("Cooldown started");
        yield return new WaitForSeconds(skill.cooldown);
        canUse = true;
        Debug.Log("Cooldown finished");
    }
    
    private IEnumerator CastTime(SkillSO skill)
    {
        Debug.Log("Casting started");
        yield return new WaitForSeconds(skill.castTime);
        Debug.Log("Casting finished");
    }
    
    void Start() { origin = skillOrigin; }
    
    public void Sightline(SkillSO skill)
    {
        // TODO: Sightline
        //     Make sightline only show when canUse is true
        //     Change ray from camera to player
        
        ray = new Ray(origin.transform.position, origin.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * skill.ranRange, Color.red);
        
        if (skill.isRanged) { // Ranged Sightline
            // TODO: Ranged Sightline
            //     Show line on floor for line attack
            
            if (sightlineRAN == null) {
                sightlineRAN = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Spawn Sphere
                DestroyImmediate(sightlineRAN.GetComponent<SphereCollider>());
                sightlineRAN.layer = 2;
                sightlineRAN.name = "sightlineRAN";
                sightlineRAN.transform.localScale = new Vector3(1, 1, 1);
                sightlineRAN.GetComponent<Renderer>().material.color = Color.red;
                if (Physics.Raycast(ray, out hitInfo, skill.ranRange, ~1<<11) && hitInfo.collider.gameObject.layer != 11) { sightlineRAN.transform.position = hitInfo.point; }
                else if (!Physics.Raycast(ray, out hitInfo, skill.ranRange)) { sightlineRAN.transform.position = ray.GetPoint(skill.ranRange); }
            }
            else {
                if (Physics.Raycast(ray, out hitInfo, skill.ranRange)) { sightlineRAN.transform.position = hitInfo.point; }
                else if (!Physics.Raycast(ray, out hitInfo, skill.ranRange)) { sightlineRAN.transform.position = ray.GetPoint(skill.ranRange); }
            }
        }
        
        if (skill.isAOE) { // AOE Sightline 
            // TODO: AOE Sightline
            //     Make AOE Sightline spawn on the ground not just below player
            //     Add OnImpact sightline for when it hits anything, not just an enemy
            
            switch (skill.aoeSpawn) {
                case SkillSO.aoeSpawnTypes.OnImpact:
                    break;
                case SkillSO.aoeSpawnTypes.OnDeath:
                case SkillSO.aoeSpawnTypes.OnHit:
                    if (Physics.Raycast(ray, out hitInfo, skill.ranRange, 1<<13)) {
                        if (hitInfo.collider.gameObject != hitObject) { Destroy(sightlineAOE); }
                        if (sightlineAOE == null) {
                            sightlineAOE = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Spawn Sphere
                            DestroyImmediate(sightlineAOE.GetComponent<SphereCollider>());
                            sightlineAOE.layer = 11;
                            sightlineAOE.name = "sightlineAOE";
                            sightlineAOE.transform.localScale = new Vector3(skill.aoeRadius, 0.01f, skill.aoeRadius);
                            sightlineAOE.GetComponent<Renderer>().material.color = Color.red;
                            sightlineAOE.transform.position = new Vector3(hitInfo.transform.position.x, hitInfo.transform.position.y - 1, hitInfo.transform.position.z);
                            hitObject = hitInfo.collider.gameObject;
                        }
                    }
                    else {
                        Destroy(sightlineAOE);
                    }
                    break;
                case SkillSO.aoeSpawnTypes.Instant:
                    if (sightlineAOE == null) {
                        sightlineAOE = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Spawn Sphere
                        DestroyImmediate(sightlineAOE.GetComponent<SphereCollider>());
                        sightlineAOE.layer = 11;
                        sightlineAOE.name = "SightlineAOE";
                        sightlineAOE.transform.localScale = new Vector3(skill.aoeRadius, 0.01f, skill.aoeRadius);
                        sightlineAOE.GetComponent<Renderer>().material.color = Color.red;
                        sightlineAOE.transform.position = new Vector3(transform.position.x, transform.position.y-1, transform.position.z);
                    }
                    else {
                        sightlineAOE.transform.position = new Vector3(transform.position.x, transform.position.y-1, transform.position.z);
                    }
                    break;
            }
        }
    }
    
    public void UseSkill(SkillSO skill)
    {
        Debug.Log("Skill Used");
        
        Destroy(sightlineRAN);
        Destroy(sightlinePRO);
        Destroy(sightlineAOE);
        hitObject = null;
        
        if (!canUse) { return; }
        canUse = false;
        StartCoroutine(Cooldown(skill));
        
        if (skill.customOrigin != null) { origin = skill.customOrigin; }
        else { origin = skillOrigin; }
        
        if (skill.isMelee) { MeleeEffects(skill); }
        if (skill.isRanged) { RangedEffects(skill); }
        if (skill.isProjectile) { ProjectileEffects(skill); }
        if (skill.isAOE) { AOEEffects(skill); }
    }

    GameObject ObjectSetup(SkillSO skill)
    {
        // TODO: Object Setup
        //     Fix ranged being wrong direction when firing from extreme angles

        GameObject obj = null;
        switch (skill.visObjectType) {
            case SkillSO.objectTypes.Capsule:
                obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                obj.transform.localScale = new Vector3(skill.visRadius, skill.visHeight, skill.visRadius);
                obj.transform.position = origin.transform.position;
                break;
            case SkillSO.objectTypes.Sphere:
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.localScale = Vector3.one * skill.visRadius;
                obj.transform.position = origin.transform.position;
                break;
            case SkillSO.objectTypes.Custom:
                obj = new GameObject();
                refMeshRenderer = obj.AddComponent<MeshRenderer>();
                refMeshFilter = obj.AddComponent<MeshFilter>();
                refMeshFilter.mesh = skill.visMesh;
                obj.transform.localScale = skill.visScale;
                switch (skill.visCollider) {
                    case SkillSO.colliderTypes.Capsule:
                        refCapsuleCollider = obj.AddComponent<CapsuleCollider>();
                        refCapsuleCollider.radius = skill.visColliderRadius;
                        refCapsuleCollider.height = skill.visColliderHeight;
                        refCapsuleCollider.center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Box:
                        refBoxCollider = obj.AddComponent<BoxCollider>();
                        refBoxCollider.size = skill.visColliderSize;
                        refBoxCollider.center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Sphere:
                        refSphereCollider = obj.AddComponent<SphereCollider>();
                        refSphereCollider.radius = skill.visColliderRadius;
                        refSphereCollider.center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Mesh:
                        refMeshCollider = obj.AddComponent<MeshCollider>();
                        refMeshCollider.sharedMesh = skill.visMesh;
                        break;
                }
                break;
        }

        obj.name = skill.name;
        obj.layer = 11;

        // Visuals
        refRenderer = obj.GetComponent<Renderer>();
        refRenderer.material = skill.visMaterial;
        if (skill.visColor.a != 0) {
            refRenderer.material.color = skill.visColor;
        }

        // Collider
        refCollider = obj.GetComponent<Collider>();
        refCollider.excludeLayers = skill.excludeLayers;

        // Add Components
        refRigidbody = obj.AddComponent<Rigidbody>();
        refSkillActive = obj.AddComponent<SkillActive>();
        refSkillActive.targetLayer = skill.targetLayers;
        refSkillActive.damageMethod = skill.damageMethod;
        if (skill.customScript != null) {
            refCustomScript = obj.AddComponent(skill.customScript.GetType());
        }

        return obj;
    }

    void MeleeEffects(SkillSO skill)
    {
        // TODO: Melee Effects
        //     Ideally do everything without spawning anything in
        //     Knockback enemies
        //     Damage enemies
        
        //Debug.Log("Melee Effects");
    }
    
    void RangedEffects(SkillSO skill)
    {
        // TODO: Ranged Effects
        //     Ran Instant
        //     Line
        
        //Debug.Log("Ranged Effects");
        if (!skill.ranInstant) {
            //Fires ray
            ray = new Ray(origin.transform.position, origin.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * skill.ranRange, Color.red);
            Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~2048);
            
            GameObject ranged = ObjectSetup(skill);
            
            // Rigidbody
            refRigidbody.useGravity = false;
            refRigidbody.freezeRotation = true;
            
            // Skill Script
            refSkillActive.skill = skill;
            refSkillActive.damage = skill.ranDamage;
            refSkillActive.range = skill.ranRange;
            refSkillActive.hitCount = skill.ranPiercingCount;
            
            if (hitInfo.transform != null) {
                ranged.transform.LookAt(hitInfo.point);
                ranged.transform.rotation = Quaternion.Euler(ranged.transform.rotation.eulerAngles + new Vector3(90, 0, 0));
                refRigidbody.linearVelocity = (hitInfo.point - ranged.transform.position).normalized * skill.ranSpeed;
            }
            else {
                ranged.transform.rotation = Quaternion.Euler(skillOrigin.transform.rotation.eulerAngles + new Vector3(90, 0, 0));
                refRigidbody.linearVelocity = origin.transform.forward * skill.ranSpeed;
            }

            if (skill.ranAttackStyle == SkillSO.ranAttackTypes.Line) {
                // TODO: Ranged Effects
                //     Make it possible to spawn stuff at points on the line
                //    Get all enemies in the line
            }
        }
    }
    
    void ProjectileEffects(SkillSO skill)
    {
        // TODO: Projectile Effects
        //     Make the projectile launch upwards with more oomph
        
        //Debug.Log("Projectile Effects");
        GameObject projectile = ObjectSetup(skill);
        
        // Skill Script
        refSkillActive.damage = skill.proDamage;
        refSkillActive.hitCount = skill.proBounce;
        
        //Fires a ray from the camera
        ray = Camera.main.ViewportPointToRay(new Vector3 (0.5f, 0.5f, 0));
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~2048);
            
        // Point towards the hit point
        projectile.transform.position = transform.position;
        projectile.transform.LookAt(hitInfo.point);
        projectile.transform.rotation = Quaternion.Euler(projectile.transform.rotation.eulerAngles);
        refRigidbody.linearVelocity = (hitInfo.point - projectile.transform.position).normalized * skill.ranSpeed;
    }
    
    void AOEEffects(SkillSO skill)
    {
        // TODO: AOE Effects
        //     Spawn on enemy death
        //     Spawn on enemy hit
        //     Spawn on impact
        //     Keeps projectile alive if enabled
        
        //Debug.Log("AOE Effects");
        GameObject aoe = ObjectSetup(skill);
        aoe.transform.localScale = new Vector3(skill.visRadius, skill.visHeight, skill.visRadius);
        
        // Rigidbody
        refRigidbody.useGravity = false;
        refRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        aoe.transform.position = transform.position - new Vector3(0, 1f, 0);
        
        // Skill Script
        refSkillActive.skill = skill;
        refSkillActive.damage = skill.aoeDamage;
        refSkillActive.duration = skill.aoeDuration;
    }
}
