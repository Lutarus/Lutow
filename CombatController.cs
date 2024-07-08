using UnityEngine;
using Character.Core;
using Character.Movement;
using System;
using System.Collections;

namespace Character.Combat
{
    public class CombatController : MonoBehaviour, IAction
    {
        [HideInInspector]
        public Transform target;
        Animator anim;
        Animator weaponAnim;
        ParticleSystem weaponParticleSys;
        ActionSchedular actionSchedular;
        Mover mover;
        Health targetHealth;
        CharacterStats characterStats;
        Vector3 rotatePosition;

        [SerializeField] Transform rightHand = null;
        [SerializeField] Transform leftHand = null;
        [SerializeField] WeaponConfig defaultWeapon = null;

        float timeSinceLastAttack = Mathf.Infinity;

        WeaponConfig currentWeaponConfig = null;
        AudioSource weaponAudio = null;
        AudioSource shieldAudio = null;


        float minDamage = 0;
        float maxDamage = 0;
        bool isCritDamage = false;
        float calculatedDamage = 0;
        float weaponAnimTime = 0f;
        float charAnimTime = 0f;
        float attackSpeed = 1f; //animasyon hızı ile değişmesi lazım!
        bool castingHeal = false;
        ParticleSystem.EmissionModule emission;

        bool attackSpeedSlowed = false;
        float slowTime = 1f;

        int mana = 100;
        int manaReductionValue = 16;
        bool manaRefillActive = false;

        private void Start()
        {
            if (mana < 100)
            {
                mana = 100;
            }
            actionSchedular = GetComponent<ActionSchedular>();
            mover = GetComponent<Mover>();
            anim = GetComponent<Animator>();
            characterStats = GetComponent<CharacterStats>();

            EquippedWeapon(defaultWeapon);

            currentWeaponConfig.GetDamage(out minDamage, out maxDamage);
            charAnimTime = anim.runtimeAnimatorController.animationClips[3].length;

        }
        private void OnEnable()
        {
            manaRefillActive = false;
        }
        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (attackSpeedSlowed)
            {
                slowTime -= Time.deltaTime;
                if (slowTime <= 0)
                {
                    attackSpeedSlowed = false;
                }
            }
            if (mana < 100 && !manaRefillActive)
            {
                StartCoroutine(RefillMana());
            }
        }

        public void EquippedWeapon(WeaponConfig weaponConfig)
        {
            currentWeaponConfig = weaponConfig;
            weaponConfig.Spawn(rightHand, leftHand, anim);

            if (weaponConfig.GetWeaponAnimator() != null)
            {
                weaponAnim = weaponConfig.GetWeaponAnimator();
                weaponAnimTime = anim.runtimeAnimatorController.animationClips[3].events[1].time - anim.runtimeAnimatorController.animationClips[3].events[0].time;
            }

            if (weaponConfig.GetWeaponHitAudio() != null)
            {
                weaponAudio = weaponConfig.GetWeaponHitAudio();
            }

            if (weaponConfig.GetShieldHitAudio() != null)
            {
                shieldAudio = weaponConfig.GetShieldHitAudio();
            }

            if (weaponConfig.GetWeaponParticleSys() != null)
            {
                weaponParticleSys = weaponConfig.GetWeaponParticleSys();
                emission = weaponParticleSys.emission;
                emission.enabled = false;
            }
        }

        public void StartAttackAction(Transform destination)
        {
            if (destination == null)
            {
                target = destination;
                return;
            }
            target = destination;
            if (Vector3.Distance(transform.position, destination.position) > currentWeaponConfig.GetRange() && !castingHeal)
            {
                mover.StartMoveAction(destination.position, false);
            }
            else
            {
                actionSchedular.StartAction(this);
                AttackBehaviour();
            }
        }

        private void AttackBehaviour()
        {
            rotatePosition = new Vector3(target.position.x,
                                        this.transform.position.y,
                                        target.position.z);
            transform.LookAt(rotatePosition);



            if (attackSpeedSlowed)
            {
                attackSpeed = (currentWeaponConfig.GetAttackSpeed() / charAnimTime) * 2;
            }
            else
            {
                attackSpeed = currentWeaponConfig.GetAttackSpeed() / charAnimTime;
            }
            anim.SetFloat("AttackSpeedMtp", 1 / attackSpeed);

            TriggerAttack();
            //if (timeSinceLastAttack >= currentWeaponConfig.GetTimeBetweenAttack())
            //{
            //    TriggerAttack();
            //    timeSinceLastAttack = 0;
            //}
        }

        private void TriggerAttack()
        {
            if (target.GetComponent<Health>().IsDead())
            {
                return;
            }
            anim.ResetTrigger("stopAttack");
            //Bu attack animasyonunu başlatıyor. Attack animasyonu içerisinde Hit() event i mevcut. Oda bir alttaki Hit() method unu çalıştırıyor.
            if (characterStats.characterType == 2800)
            {
                if (mana < manaReductionValue)
                {
                    return;
                }
            }

            anim.SetTrigger("attack");
            if (weaponParticleSys != null) emission.enabled = true;
        }


        //Animation Event - Vurma animasyonunun eventi 11. snye geldiğinde yumruk atıyor ve hit i çağırıyor.
        void Hit()
        {
            anim.ResetTrigger("attack");
            if (weaponAudio != null)
            {
                weaponAudio.Play();
            }
            if (target == null) return;

            targetHealth = target.GetComponent<Health>();

            if (targetHealth == null) return;

            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHand, leftHand, targetHealth, DamageCalculator(), isCritDamage);
            }
            else
            {
                targetHealth.TakeDamage(DamageCalculator(), isCritDamage, currentWeaponConfig.damageType, characterStats.unitType);
                if (currentWeaponConfig.healReduction)
                {
                    targetHealth.HealReduct();
                }
            }

        }

        float DamageCalculator()
        {
            int rCritChance = UnityEngine.Random.Range(0, 100);
            float rDamage = UnityEngine.Random.Range(minDamage, maxDamage);

            if (rCritChance < characterStats.GetCritStrikeChance() + currentWeaponConfig.critStrikeChance)
            {
                isCritDamage = true;
                calculatedDamage = rDamage * characterStats.GetCritStrikeMod();
            }
            else
            {
                isCritDamage = false;
                calculatedDamage = rDamage;
            }

            return calculatedDamage;
        }



        void Shoot()
        {
            weaponAnim.speed = 1 / (weaponAnimTime * attackSpeed);
            if (weaponAnim != null) weaponAnim.ResetTrigger("weaponHitStop");
            if (weaponAnim != null) weaponAnim.SetTrigger("weaponHit");
        }

        void Cast()
        {
            Hit();
        }
        void Heal()
        {
            mana = Mathf.Clamp(mana - manaReductionValue, 0, 100);

            Debug.Log(mana + " : " + Time.time);
            Hit();
        }
        void CastingHeal()
        {
            var x = castingHeal;

            castingHeal = !x;
        }
        public void Cancel()
        {
            StopAttack();
        }

        private void StopAttack()
        {
            anim.ResetTrigger("attack");
            anim.SetTrigger("stopAttack");
            if (weaponParticleSys != null) emission.enabled = false;
            if (weaponAnim != null) weaponAnim.ResetTrigger("weaponHit");
            if (weaponAnim != null) weaponAnim.SetTrigger("weaponHitStop");
        }

        public void ShieldSound()
        {
            if (shieldAudio != null)
            {
                shieldAudio.Play();
            }
        }

        public void AttackSpeedSlowed()
        {
            attackSpeedSlowed = true;
            slowTime = 2;
        }

        IEnumerator RefillMana()
        {
            manaRefillActive = true;
            yield return new WaitForSeconds(currentWeaponConfig.GetAttackSpeed() / 2);
            if (target != null)
            {
                mana = Mathf.Clamp(mana + (manaReductionValue / 8), 0, 100);
            }
            else
            {
                mana = Mathf.Clamp(mana + 20, 0, 100);
            }
            Debug.Log(mana + " : " + Time.time);
            manaRefillActive = false;
        }

        public int GetManaValue()
        {
            return mana;
        }
    }
}

