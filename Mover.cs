using UnityEngine;
using UnityEngine.AI;
using Character.Core;
using UnityEngine.Events;

namespace Character.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        NavMeshAgent agent;
        ActionSchedular actionSchedular;
        Animator anim;
        Health health;
        bool animationWalkingState;

        [SerializeField] UnityEvent footSound = null;
        [SerializeField] AudioSource footAudioSource = null;

        [Header("Debuff")]
        [SerializeField] GameObject debuff_Slow = null;

        bool movementSlowed = false;
        float slowTime = 1f;

        private void Start()
        {
            debuff_Slow.SetActive(false);
            agent = GetComponent<NavMeshAgent>();
            actionSchedular = GetComponent<ActionSchedular>();
            anim = GetComponent<Animator>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            agent.enabled = !health.IsDead();
            if (!agent.enabled) return;
            UpdateAnimator();

            if (movementSlowed)
            {
                slowTime -= Time.deltaTime;
                if (slowTime <= 0)
                {
                    debuff_Slow.SetActive(false);
                    movementSlowed = false;
                }
            }
        }

        public void StartMoveAction(Vector3 destination, bool isWalking)
        {
            actionSchedular.StartAction(this);
            if (!agent.enabled) return;
            MoveTo(destination);
            animationWalkingState = isWalking;
        }

        private void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
            agent.isStopped = false;
        }

        public void WarpTo(Vector3 warpPosition)
        {
            agent.Warp(warpPosition);
        }

        //Interface Metodu
        public void Cancel()
        {
            agent.isStopped = true;
        }

        private void UpdateAnimator()
        {
            // Hızlı Koşma = 3.808
            // Yürüme = 1.449
            // Bilgiler animasyonun kendisinden geliyor.

            if (animationWalkingState && !movementSlowed)
            {
                agent.speed = 2f; //Normal Yürüme biraz yavaş.!
                Vector3 velocity = agent.velocity;
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                float speed = localVelocity.z;
                anim.SetFloat("forwardSpeed", speed);
            }
            if(!animationWalkingState && !movementSlowed)
            {
                agent.speed = 3.808f;
                Vector3 velocity = agent.velocity;
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                float speed = localVelocity.z;
                anim.SetFloat("forwardSpeed", speed);
            }
            if (movementSlowed)
            {
                agent.speed = 1.5f;
                Vector3 velocity = agent.velocity;
                Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                float speed = localVelocity.z;
                anim.SetFloat("forwardSpeed", speed);
            }

            

            // Önemli : InverseTransformDirection = World bilgisini local'e çevirmek için kullanılıyor. 
            // Agent.velocity normalde world bilgisi içeriyor. Yani karakter Z ekseninde sağa doğru 5 hızıyla gider ise Agent.velocity = 5 , sola doğru gider ise Agent.velocity = -5 oluyor. 
            // Ancak karakterin localinde yüzü sürekli olarak koştuğu yöne baktığı için Z hep +5 olarak kalıyor. Bu yüzden agent.velocity değerini local değere çevirip ondan sonra animasyon forwardSpeed'e atıyoruz.
            // Hatta karakter oyun ekseninde yukarı ve aşağı hareket eder ise z ekseninde 5 olarak gösterilen hız 0 a yaklaşır.

            //Debug.Log("speed" + speed);
            //Debug.Log("velocity" + velocity.z);
        }

        public void PlayFootSound()
        {
            if (footAudioSource.isPlaying)
            {
                return;
            }
            //footSound.Invoke();
            int r = Random.Range(0, AudioManager.instance.footSteps.Length);
            AudioClip clip = AudioManager.instance.footSteps[r];
            int p = Random.Range(1, 3);
            footAudioSource.clip = clip;
            footAudioSource.pitch = p;
            footAudioSource.Play();

        }

        public void MovementSlowed()
        {
            movementSlowed = true;
            debuff_Slow.SetActive(true);
            slowTime = 2;
        }
    }
}
