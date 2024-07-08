using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Core;
using Character.Movement;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] float startSpeed = 1f;
    [SerializeField] float projectileDamage = 0f;

    [Header("AOE")]
    [SerializeField] bool isAOE = false;
    [SerializeField] int explosionRadius = 3;

    [Header("Misc")]
    [SerializeField] float lifeAfterImpact = 0.5f;
    [SerializeField] UnityEvent onHitSound = null;
    public GameObject[] destroyOnHit = null;

    [Header("Projectile Effects")]
    [SerializeField] ParticleSystem impactEffect = null;
    bool healReduction = false;

    int projectileLife = 10;
    float totalDamage = 0f;
    bool isCrit = false;
    int damageType = 0;
    Health target = null;
    float speed = 1f;

    private void Start()
    {
        speed = startSpeed;
    }

    private void Update()
    {
        if (target == null) return;

        if (!target.IsDead())
        {
            transform.LookAt(GetAimLocation());
        }
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetTarget(Health target, float damage,bool isCrit, int damageType, bool healReduction)
    {
        this.target = target;
        this.totalDamage = damage + projectileDamage;
        this.isCrit = isCrit;
        this.damageType = damageType;
        this.healReduction = healReduction;
        StartCoroutine(ReturnToPool(projectileLife)); //eğer arrow hedefe vuramaz ve ileriye doğru devam eder ise verilen süre sonunda oyundan silinecek.
    }

    private Vector3 GetAimLocation()
    {
        CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
        if (targetCapsule == null)
        {
            return target.transform.position;
        }
        return target.transform.position + Vector3.up * targetCapsule.height / 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Health>() != target) return;
        if (target.IsDead()) return;

        if (isAOE)
        {
            Explode();
        }
        else
        {
            target.TakeDamage(totalDamage, isCrit, damageType, 0);
            if (healReduction)
            {
                target.HealReduct();
            }
        }
        speed = 0f; // ok veya büyünün hızını sıfıra çekiyoruz ki eğer gecikmeli yok olan bir efek var ise olduğu yerde kalsın objenin arkasına doğru devam etmesin.

        onHitSound.Invoke();

        if (impactEffect != null)
        {
            impactEffect.Play();
        }

        if (destroyOnHit != null)
        {
            foreach (GameObject obj in destroyOnHit)
            {
                obj.SetActive(false);
            }
        }
        
        StopCoroutine(ReturnToPool(lifeAfterImpact));
        StartCoroutine(ReturnToPool(lifeAfterImpact));
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.tag == target.tag && !collider.GetComponent<Health>().IsDead())
            {
                collider.GetComponent<Health>().TakeDamage(totalDamage, isCrit, damageType, 0);
            }
        }
    }

    IEnumerator ReturnToPool(float time)
    {
        yield return new WaitForSeconds(time);
        speed = startSpeed;
        ProjectilePooler.Instance.ReturnToPool(gameObject.name, this);
    }

}
