using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameMechanics.GlobalSystem;
using Character.Control;
using Character.Combat;
using Character.Movement;

namespace Character.Core
{

    public class Health : MonoBehaviour
    {
        [SerializeField] DamageTextSpawner damageSpawner = null;
        //[SerializeField] UnityEvent dieSound = null;
        //[SerializeField] UnityEvent takeDamageSound = null;

        [Header("Sounds")]
        [SerializeField] AudioSource hitSound = null;
        [SerializeField] AudioSource deathSound = null;
        [Header("Particle Systems")]
        [SerializeField] ParticleSystem buildingParticleSystem = null;
        [SerializeField] ParticleSystem healParticleSystem = null;

        [Header("Debuff")]
        [SerializeField] GameObject debuff_HealReduct = null;

        CharacterStats characterStats;
        Animator anim;
        ActionSchedular actionSchedular;
        PlayerAI playerAI;
        Mover playerMover;
        CombatController combatController;
        bool isDead = false;
        float reductedDamage = 0;

        bool healReduct = false;
        float healReductTime = 1f;
        float healReductAmount = 0f;

        private void Start()
        {
            characterStats = GetComponent<CharacterStats>();
            anim = GetComponent<Animator>();
            actionSchedular = GetComponent<ActionSchedular>();
            playerMover = GetComponent<Mover>();
            combatController = GetComponent<CombatController>();
            debuff_HealReduct.SetActive(false);
            if (transform.tag == "Alliance")
            {
                playerAI = GetComponent<PlayerAI>();
            }
        }

        private void Update()
        {
            if (healReduct)
            {
                healReductTime -= Time.deltaTime;
                if (healReductTime <= 0)
                {
                    healReduct = false;
                    debuff_HealReduct.SetActive(false);
                }
            }
        }

        public bool IsDead()
        {
            return isDead;
        }
        public void TakeDamage(float damage, bool isCrit, int damageType, int unitType)
        {
            var reductedDamage = DamageReductCalculator(damage, damageType, unitType);
            var health = Mathf.Clamp(characterStats.currentHealth - reductedDamage, 0, characterStats.startHealth);
            characterStats.currentHealth = health;
            if (characterStats.currentHealth == 0)
            {
                //dieSound.Invoke();
                if (characterStats.characterType < 3000) PlayDeathSound();
                Die();
            }
            else
            {
                if (damageType == (int)Enums.DamageType.Heal)
                {
                    damageSpawner.Spawn(reductedDamage * -1, isCrit, damageType);
                    healParticleSystem.Play();
                }
                else
                {
                    damageSpawner.Spawn(reductedDamage, isCrit, damageType);
                }

                if (characterStats.characterType < 3000) combatController.ShieldSound();

                if (damageType != (int)Enums.DamageType.Heal)
                {
                    if (characterStats.characterType < 3000) PlayHitSound();
                    if (damageType == (int)Enums.DamageType.Frost)
                    {
                        if (characterStats.characterType < 3000) playerMover.MovementSlowed();
                        if (characterStats.characterType < 3000) combatController.AttackSpeedSlowed();
                    }
                }

            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (characterStats.characterType >= 3000) // Tower
            {
                anim.SetBool("Destroyed", true);

                GlobalStats.AddMoney(characterStats.characterValue / 2, GlobalVariables.playerTag);
                GlobalStats.AddScore(characterStats.characterScore, GlobalVariables.playerTag);
                StartCoroutine(WaitForBuildingDisappear(transform.gameObject));
                if (buildingParticleSystem != null)
                {
                    buildingParticleSystem.Play();
                }
                return;
            }

            anim.SetTrigger("die");
            actionSchedular.CancelCurrentAction();

            if (transform.tag == GlobalVariables.enemyTag)
            {
                GlobalStats.AddMoney(characterStats.characterValue / 2, GlobalVariables.playerTag);
                GlobalStats.AddScore(characterStats.characterScore, GlobalVariables.playerTag);

                StartCoroutine(ReturnToPool());
                GlobalArmyCounter.CounterRemoveEnemyUnit();
                if (GlobalVariables.gamePlayMode == 1)
                {
                    UIGame.Instance.UpdateUIVariables();
                }
            }
            if (transform.tag == GlobalVariables.playerTag)
            {
                if (GlobalVariables.gamePlayMode == 2)
                {
                    GlobalStats.AddMoney(characterStats.characterValue / 2, GlobalVariables.enemyTag);
                    GlobalStats.AddScore(characterStats.characterScore, GlobalVariables.enemyTag);
                }
                StartCoroutine(ReturnToPool());
                playerAI.BreakNodeConnection();
                GlobalArmyCounter.CounterRemoveUnit(characterStats.characterType);
            }
        }

        IEnumerator ReturnToPool()
        {
            yield return new WaitForSeconds(5);
            if (transform.tag == GlobalVariables.enemyTag)
            {
                EnemyArmyPooler.Instance.ReturnToPool(characterStats.characterType, gameObject);
            }
            if (transform.tag == GlobalVariables.playerTag)
            {
                ArmyPooler.Instance.ReturnToPool(characterStats.characterType, gameObject);
            }
        }

        float DamageReductCalculator(float takenDamage, int damageType, int unitType)
        {
            float damageReductionConst = 1f;
            healReductAmount = takenDamage / 2;
            switch (damageType)
            {
                case (int)Enums.DamageType.Physical:
                    {
                        damageReductionConst = (100f - characterStats.armor) / 100f;
                        reductedDamage = (float)(takenDamage * damageReductionConst);
                    }
                    break;
                case (int)Enums.DamageType.Magical:
                    {
                        damageReductionConst = (100f - characterStats.GetSpellDRF()) / 100f;
                        reductedDamage = (float)(takenDamage * damageReductionConst);
                    }
                    break;
                case (int)Enums.DamageType.Heal:
                    {
                        if (healReduct)
                        {
                            reductedDamage = (takenDamage - healReductAmount) * -1;
                        }
                        else
                        {
                            reductedDamage = takenDamage * -1;
                        }

                    }
                    break;
                case (int)Enums.DamageType.Frost:
                    {
                        damageReductionConst = (100f - characterStats.GetSpellDRF()) / 100f;
                        reductedDamage = (float)(takenDamage * damageReductionConst);
                    }
                    break;
            }



            if (unitType != characterStats.unitType)
            {

            }

            return reductedDamage;
        }

        public float GetHealthValue()
        {
            return characterStats.currentHealth;
        }

        public void CharacterReborn()
        {
            isDead = false;
            characterStats.currentHealth = characterStats.startHealth;
            anim.SetTrigger("Reborn");
        }

        public void PlayHitSound()
        {
            if (hitSound.isPlaying)
            {
                return;
            }
            //footSound.Invoke();
            int r = Random.Range(0, AudioManager.instance.HitSounds.Length);
            AudioClip clip = AudioManager.instance.HitSounds[r];
            hitSound.PlayOneShot(clip);
        }
        public void PlayHealSound()
        {
            //footSound.Invoke();
            int r = Random.Range(0, AudioManager.instance.HealSounds.Length);
            AudioClip clip = AudioManager.instance.HealSounds[r];
            hitSound.PlayOneShot(clip);
        }
        public void PlayDeathSound()
        {
            //footSound.Invoke();
            int r = Random.Range(0, AudioManager.instance.DeathSounds.Length);
            AudioClip clip = AudioManager.instance.DeathSounds[r];
            deathSound.PlayOneShot(clip);
        }

        public void HealReduct()
        {
            healReduct = true;
            debuff_HealReduct.SetActive(true);
            healReductTime = 2;
        }

        IEnumerator WaitForBuildingDisappear(GameObject building)
        {
            yield return new WaitForSeconds(2);

            building.SetActive(false);
        }
    }
}