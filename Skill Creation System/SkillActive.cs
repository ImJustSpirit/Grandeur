// TODO: Overall
//     Stop skill from colliding when it should be piercing
//     Test SFX
//     Knockback enemies
//     Fix the code so AOE hits more than one at once
//     Destroy after range is reached
//     Damage?
//     Ran piercing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActive : MonoBehaviour
{
    public SkillSO skill;
    public float damage;
    public float range = -1;    // ranRange,                   | Anything above 0 has a range, -1 is infinite
    public int hitCount = -1;   // ranPiercingCount, proBounce | Anything above 0 has limited hits, -1 is infinite
    public float duration = -1; // aoeDuration                 | Anything above 0 is timed, -1 is infinite
    public LayerMask targetLayer;
    private AudioSource audioData;
    private List<Collision> dotCooldown = new List<Collision>();
    private Vector3 startPosition;
    
    private IEnumerator dotTimer(Collision collision)
    {
        Debug.Log($"Hit: {collision.collider.gameObject.name}");
        // BUG: BROKEN UNTIL STATS ADDED
        /*if (collision.gameObject.TryGetComponent(out Health health))
        {
            health?.ApplyDamage(new DamageInfo(damage, collision.transform.position, gameObject));
            audioData.clip = skill.sndHit;
            audioData.Play();
        }*/
        yield return new WaitForSeconds(1);
        dotCooldown.Remove(collision);
    }

    private void Start()
    {
        startPosition = transform.position;
        audioData = gameObject.AddComponent<AudioSource>();
        audioData.loop = true;
        audioData.clip = skill.sndRunning;
        audioData.Play();
    }

    void Update()
    {
        if (range > 0 && Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
        if (duration > 0) { duration -= Time.deltaTime; }
        else if (duration <= 0 && duration > -1) { Destroy(gameObject); }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Hit: {collision.collider.gameObject.name}");
        if (collision.gameObject.layer == targetLayer) // Hit target
        {
            audioData.clip = skill.sndHit;
            audioData.Play();
            if (hitCount > 0) { hitCount--; } // Reduce hit count
            else if (hitCount == 0) { Destroy(gameObject); } // Destroy skill if out of hits
            DoDamage(collision.gameObject);
        }
        else if (!skill.isAOE) // Impact non-target
        {
            audioData.clip = skill.sndImpact;
            audioData.Play();
            Destroy(gameObject);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (duration > 0)
        {
            if (dotCooldown.Find(x => x == collision) != collision)
            { 
                StartCoroutine(dotTimer(collision));
                dotCooldown.Add(collision);
            }
            foreach (var x in dotCooldown)
            {
                Debug.Log($"Found: {x.gameObject.name}");
                DoDamage(collision.gameObject);
            }
        }
    }

    void DoDamage(GameObject target)
    {
        switch (SkillManager.damageMethod) {
            case SkillManager.methods.destroy:
                Destroy(target);
                break;
            case SkillManager.methods.deactivate:
                target.gameObject.SetActive(false);
                break;
            case SkillManager.methods.custom: // User defined damage method
                break;
        }
    }
}