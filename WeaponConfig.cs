using UnityEngine;
using Character.Core;


namespace Character.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Character Weapon Type Animation Overrider")]
        [SerializeField] AnimatorOverrideController animatorOverride = null;

        [Header("Weapon/Shield/Projectile Prefabs")]
        [SerializeField] Weapon equippedWeaponPrefab = null;
        [SerializeField] Weapon equippedShieldPrefab = null;
        [SerializeField] Projectile projectile = null;

        [Header("Weapon Positioning")]
        [SerializeField] bool isRightHanded = true;
        [SerializeField] bool isShielded = false;

        [Header("Weapon Stats")]
        [SerializeField] public int damageType = (int)Enums.DamageType.Physical;
        [SerializeField] public int critStrikeChance = Enums.BaseCriticalStrikeChancePercantage;
        [SerializeField] public int lifeSteal = Enums.BaseLifeStealPercantage;
        [SerializeField] float attackRange = 2f;
        [SerializeField] float weaponDamageMin = 10f;
        [SerializeField] float weaponDamageMax = 20f;
        [SerializeField] float attackSpeed = 2f;

        [Header("Special Effects")]
        [SerializeField] public bool healReduction = false;


        Animator weaponanimator = null;
        AudioSource weaponHitAudio = null;
        AudioSource shieldHitAudio = null;
        ParticleSystem weaponParticleSys = null;

        public void Spawn(Transform rightHand, Transform leftHand, Animator animator)
        {
            if (equippedWeaponPrefab != null)
            {
                Transform handTransform = GetTransform(rightHand, leftHand);

                Weapon iWeapon = Instantiate(equippedWeaponPrefab, handTransform);

                if (iWeapon.GetWeaponHitAudio() != null)
                {
                    weaponHitAudio = iWeapon.GetWeaponHitAudio();
                }
                if (iWeapon.GetWeaponAnimator() != null)
                {
                    weaponanimator = iWeapon.GetWeaponAnimator();
                }
                if (iWeapon.GetWeaponParticleSystem() != null)
                {
                    weaponParticleSys = iWeapon.GetWeaponParticleSystem();
                }
                if (isShielded == true && equippedShieldPrefab != null)
                {
                    Weapon iShield = Instantiate(equippedShieldPrefab, leftHand);

                    if (iShield.GetWeaponHitAudio() != null)
                    {
                        shieldHitAudio = iShield.GetWeaponHitAudio();
                    }
                }
            }
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
        }

        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform;
            if (isRightHanded) handTransform = rightHand;
            else handTransform = leftHand;
            return handTransform;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(Transform righthand, Transform leftHand, Health target, float calculatedDamage, bool isCrit)
        {
            //Projectile projectileInstance = Instantiate(projectile, GetTransform(righthand, leftHand).position, Quaternion.identity);
            Projectile projectileInstance = ProjectilePooler.Instance.SpawnFromPool(projectile.name + "(Clone)", GetTransform(righthand, leftHand).position, leftHand.parent.transform.rotation);
            projectileInstance.SetTarget(target, calculatedDamage, isCrit, damageType, healReduction);
        }

        public void GetDamage(out float minDamage, out float maxDamage)
        {
            minDamage = weaponDamageMin;
            maxDamage = weaponDamageMax;
        }
        public float GetAttackSpeed()
        {
            return attackSpeed;
        }
        public float GetRange()
        {
            return attackRange;
        }
        //public float GetTimeBetweenAttack()
        //{
        //    return timeBetweenAttacks;
        //}
        public AudioSource GetWeaponHitAudio()
        {
            return weaponHitAudio;
        }
        public Animator GetWeaponAnimator()
        {
            return weaponanimator;
        }
        public ParticleSystem GetWeaponParticleSys()
        {
            return weaponParticleSys;
        }
        public AudioSource GetShieldHitAudio()
        {
            return shieldHitAudio;
        }
    }
}
