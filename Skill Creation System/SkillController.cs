// TODO: Overall
//     Make the material also get the opacity

using System.Collections;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    // Public Variables
    public GameObject skillOrigin;
    
    // Private Variables
    private GameObject origin;
    
    // Sightline Variables
    private GameObject sightlineRAN;
    private GameObject sightlinePRO;
    private GameObject sightlineAOE;
    
    // Raycasting Variables
    private Ray ray;
    private RaycastHit hitInfo;
    private GameObject hitObject = null;
    
    private IEnumerator Cooldown(SkillSO skill)
    {
        Debug.Log("Cooldown started");
        yield return new WaitForSeconds(skill.cooldown);
        skill.canUse = true;
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
        //     Make sightline only show when skill.canUse is true
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
        
        if (!skill.canUse) { return; }
        skill.canUse = false;
        StartCoroutine(Cooldown(skill));
        
        if (skill.customOrigin != null) { origin = skill.customOrigin; }
        else { origin = skillOrigin; }
        
        PlayerEffects(skill);
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
                if (hitInfo.transform != null) {
                    obj.transform.LookAt(hitInfo.point);
                    obj.transform.rotation = Quaternion.Euler(obj.transform.rotation.eulerAngles + new Vector3(90, 0, 0));
                    obj.GetComponent<Rigidbody>().linearVelocity = (hitInfo.point - obj.transform.position).normalized * skill.ranSpeed;
                }
                else {
                    Debug.Log(skillOrigin.transform.rotation.eulerAngles);
                    obj.transform.rotation = Quaternion.Euler(skillOrigin.transform.rotation.eulerAngles + new Vector3(90, 0, 0));
                    obj.GetComponent<Rigidbody>().linearVelocity = origin.transform.forward * skill.ranSpeed;
                }
                break;
            case SkillSO.objectTypes.Sphere:
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.localScale = Vector3.one * skill.visRadius;
                break;
            case SkillSO.objectTypes.Custom:
                obj = new GameObject();
                obj.AddComponent<MeshRenderer>();
                obj.AddComponent<MeshFilter>().mesh = skill.visMesh;
                obj.transform.localScale = skill.visScale;
                switch (skill.visCollider) {
                    case SkillSO.colliderTypes.Capsule:
                        obj.AddComponent<CapsuleCollider>();
                        obj.GetComponent<CapsuleCollider>().radius = skill.visColliderRadius;
                        obj.GetComponent<CapsuleCollider>().height = skill.visColliderHeight;
                        obj.GetComponent<CapsuleCollider>().center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Box:
                        obj.AddComponent<BoxCollider>();
                        obj.GetComponent<BoxCollider>().size = skill.visColliderSize;
                        obj.GetComponent<BoxCollider>().center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Sphere:
                        obj.AddComponent<SphereCollider>();
                        obj.GetComponent<SphereCollider>().radius = skill.visColliderRadius;
                        obj.GetComponent<SphereCollider>().center = skill.visColliderCenter;
                        break;
                    case SkillSO.colliderTypes.Mesh:
                        obj.AddComponent<MeshCollider>();
                        obj.GetComponent<MeshCollider>().sharedMesh = skill.visMesh;
                        break;
                }
                break;
        }
        obj.name = skill.name;
        obj.layer = 11;
            
        // Visuals
        obj.GetComponent<Renderer>().material = skill.visMaterial;
        if (skill.visColor.a != 0) {obj.GetComponent<Renderer>().material.color = skill.visColor;}
            
        // Collider
        obj.GetComponent<Collider>().excludeLayers = skill.excludeLayers;
        
        // Add Components
        obj.AddComponent<Rigidbody>();
        obj.AddComponent<SkillActive>();
        if (skill.customScript != null) { obj.AddComponent(skill.customScript.GetType()); }
        
        return obj;
    }
    
    void PlayerEffects(SkillSO skill)
    {
        Debug.Log("Player Effects");
        
        // BUG: BROKEN UNTIL STATS ADDED
        /*playerController.statHealth.AddModifier(new StatModifier(skill.modHealth, StatModifierType.PercentAdd, skill, skill.modDuration));
        playerController.statSpeed.AddModifier(new StatModifier(skill.modSpeed, StatModifierType.PercentAdd, skill, skill.modDuration));
        playerController.statdamage.AddModifier(new StatModifier(skill.modDamage, StatModifierType.PercentAdd, skill, skill.modDuration));
        playerController.statKnockback.AddModifier(new StatModifier(skill.modKnockback, StatModifierType.PercentAdd, skill, skill.modDuration));*/
    }
    
    void MeleeEffects(SkillSO skill)
    {
        // TODO: Melee Effects
        //     Ideally do everything without spawning anything in
        //     Knockback enemies
        //     Damage enemies
        
        Debug.Log("Melee Effects");
    }
    
    void RangedEffects(SkillSO skill)
    {
        // TODO: Ranged Effects
        //     Ran Instant
        //     Line
        
        Debug.Log("Ranged Effects");
        if (!skill.ranInstant) {
            //Fires ray
            ray = new Ray(origin.transform.position, origin.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * skill.ranRange, Color.red);
            Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~2048);
            
            GameObject ranged = ObjectSetup(skill);
            
            // Rigidbody
            ranged.GetComponent<Rigidbody>().useGravity = false;
            ranged.GetComponent<Rigidbody>().freezeRotation = true;
            
            // Skill Script
            ranged.GetComponent<SkillActive>().skill = skill;
            ranged.GetComponent<SkillActive>().damage = skill.ranDamage;
            ranged.GetComponent<SkillActive>().range = skill.ranRange;
            ranged.GetComponent<SkillActive>().hitCount = skill.ranPiercingCount;
            ranged.GetComponent<SkillActive>().targetLayer  = skill.targetLayers;

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
        
        Debug.Log("Projectile Effects");
        GameObject projectile = ObjectSetup(skill);
        
        // Skill Script
        projectile.GetComponent<SkillActive>().damage = skill.proDamage;
        projectile.GetComponent<SkillActive>().hitCount = skill.proBounce;
        
        //Fires a ray from the camera
        ray = Camera.main.ViewportPointToRay(new Vector3 (0.5f, 0.5f, 0));
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~2048);
            
        // Point towards the hit point
        projectile.transform.position = transform.position;
        projectile.transform.LookAt(hitInfo.point);
        projectile.transform.rotation = Quaternion.Euler(projectile.transform.rotation.eulerAngles);
        projectile.GetComponent<Rigidbody>().linearVelocity = (hitInfo.point - projectile.transform.position).normalized * skill.ranSpeed;
    }
    
    void AOEEffects(SkillSO skill)
    {
        // TODO: AOE Effects
        //     Spawn on enemy death
        //     Spawn on enemy hit
        //     Spawn on impact
        //     Keeps projectile alive if enabled
        
        Debug.Log("AOE Effects");
        GameObject aoe = ObjectSetup(skill);
        aoe.transform.localScale = new Vector3(skill.visRadius, skill.visHeight, skill.visRadius);
        
        // Rigidbody
        aoe.GetComponent<Rigidbody>().useGravity = false;
        aoe.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        aoe.transform.position = transform.position - new Vector3(0, 1f, 0);
        
        // Skill Script
        aoe.GetComponent<SkillActive>().skill = skill;
        aoe.GetComponent<SkillActive>().damage = skill.aoeDamage;
        aoe.GetComponent<SkillActive>().duration = skill.aoeDuration;
    }
}
