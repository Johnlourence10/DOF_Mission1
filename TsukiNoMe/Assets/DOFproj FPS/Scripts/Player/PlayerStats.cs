/// Deacon of Freedom Development (2020) v1
/// If you have any questions feel free to write me at email --- Phil-James_Lapuz@outlook.com ---

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DOFprojFPS
{
    /// <summary>
    /// It's very simple class so I think that there is no need in deep explanation of it
    /// When health less than or equal to zero -> Death() || if respawn -> Respawn()
    /// Death and Respawn methods works in same way but in oposite direction
    /// </summary>

    public class PlayerStats : MonoBehaviour
    {
        [Header("Health")]
        public bool isGod = false;
        [Tooltip("Player's health")]
        public int health = 100;
        [Tooltip("UI element to draw health as number")]
        public Text healthUIText;
        [Tooltip("UI element to draw health as slider")]
        public Slider healthUISlider;

        public Slider hungerSlider;
        public Text hungerUIText;

        public Slider thirstSlider;
        public Text thirstUIText;

        [Header("Damage effect")]
        [Tooltip("UI Image with fullscreen hit fx")]
        public Image damageScreenFX;
        [Tooltip("UI Image color to change on hit")]
        public Color damageScreenColor;
        [Tooltip("UI Image fade speed after hit")]
        public float damageScreenFadeSpeed = 1.4f;

        [Header("Consume stats")]
        public bool useConsumeSystem = true;
        
        public int hydratation = 100;
        public float hydratationSubstractionRate = 3f;
        public int thirstDamage = 1;
        [HideInInspector]
        public float hydratationTimer;

        public int satiety = 100;
        public float satietySubstractionRate = 5f;
        public int hungerDamage = 1;
        [HideInInspector]
        public float satietyTimer;

        public Text playerStats;

        private Color damageScreenColor_temp;

        public static bool isPlayerDead = false;

        [HideInInspector]
        public Vector3 playerPosition;
        [HideInInspector]
        public Quaternion playerRotation;

        private GameObject playerBody;
        
        #region utility objects
        private Rigidbody playerRigidbody;
        private FPSController controller;
        private CapsuleCollider playerCollider;
        private Sway sway;

        private Transform cameraHolder;

        private WeaponManager weaponManager;
        //Don't create any rigidbody here
        private Rigidbody rigidbody_temp;
        #endregion
        
        private void Start()
        {
            cameraHolder = GameObject.Find("Camera Holder").GetComponent<Transform>();

            isPlayerDead = false;

            playerRigidbody = GetComponent<Rigidbody>();
            controller = GetComponent<FPSController>();
            playerCollider = GetComponent<CapsuleCollider>();
            weaponManager = FindObjectOfType<WeaponManager>();
            sway = FindObjectOfType<Sway>();
            
            if(!InputManager.useMobileInput)
            playerBody = FindObjectOfType<Body>().gameObject;
        }
        
        void Update()
        {
            if (isPlayerDead)
            {
                weaponManager.gameObject.SetActive(false);

                if (cameraHolder.transform.eulerAngles.x >= 90 || cameraHolder.transform.eulerAngles.x <= -90 || cameraHolder.transform.eulerAngles.z >= 90 || cameraHolder.transform.eulerAngles.z <= -90)
                {
                    if (rigidbody_temp)
                        rigidbody_temp.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }

            if (health == 0 && !isPlayerDead && !isGod)
            {
                PlayerDeath();
            }

            if (health < 0)
            {
                health = 0;
            }

            if(health >  100)
            {
                health = 100;
            }

            WritePlayerTransform();
            ConsumableManager(useConsumeSystem);
            DrawHealthStats();
            DrawPlayerStats();
        }
        
        public void ConsumableManager(bool useSystem)
        {
            if (!useSystem)
                return;

            if (Time.time > satietyTimer + satietySubstractionRate)
            {
                if (satiety <= 0)
                {
                    satiety = 0;
                    health -= hungerDamage;
                }

                satiety -= 1;
                satietyTimer = Time.time;
                
            }

            if (Time.time > hydratationTimer + hydratationSubstractionRate)
            {
                if (hydratation <= 0)
                {
                    hydratation = 0;
                    health -= thirstDamage;
                }
                hydratation -= 1;
                hydratationTimer = Time.time;
            }

            if(hydratation > 100)
            {
                hydratation = 100;
            }
            if(satiety > 100)
            {
                satiety = 100;
            }
        }

        public void DrawPlayerStats()
        {
            if(playerStats != null)
                playerStats.text = string.Format("--- Player statistic ---\n\n\n - Health: {0}\n\n - Hydratation: {1}\n\n - Satiety: {2}\n\n", health, hydratation, satiety);
        }

        public void ApplyDamage(int damage)
        {
            if (damage > 0)
            {
                health -= damage;
                damageScreenFX.color = damageScreenColor;
                damageScreenColor_temp = damageScreenColor;
                StartCoroutine(HitFX());
            }
        }

        public void AddSatiety(int points)
        {
            satiety += points;
        }

        public void AddHydratation(int points)
        {
            hydratation += points;
        }

        public void AddHealth(int hp)
        {
            health += hp;
        }

        IEnumerator HitFX()
        {
            while (damageScreenFX.color.a > 0)
            {
                damageScreenColor_temp = new Color(damageScreenColor_temp.r, damageScreenColor_temp.g, damageScreenColor_temp.b, damageScreenColor_temp.a -= damageScreenFadeSpeed * Time.deltaTime);
                damageScreenFX.color = damageScreenColor_temp;

                yield return new WaitForEndOfFrame();
            }
        }

        public void PlayerDeath()
        {
            if (!isPlayerDead)
            {
                var cameraTarget = GameObject.FindGameObjectWithTag("MainCamera").tag = "Untagged";

                sway.enabled = false;
                var leanController = FindObjectOfType<Lean>().enabled = false;
                controller.enabled = false;
                playerCollider.enabled = false;
                playerRigidbody.isKinematic = true;
                
                if(playerBody != null && !InputManager.useMobileInput)
                {
                    playerBody.SetActive(false);
                }

                if (!rigidbody_temp)
                {
                    rigidbody_temp = cameraHolder.gameObject.AddComponent<Rigidbody>();
                    rigidbody_temp.mass = 20;
                    cameraHolder.gameObject.AddComponent<SphereCollider>().radius = 0.3f;
                }
                else
                {
                    cameraHolder.GetComponent<SphereCollider>().enabled = true;
                }


                cameraHolder.transform.parent = null;

                rigidbody_temp.isKinematic = false;
                rigidbody_temp.AddTorque(cameraHolder.transform.forward * 10, ForceMode.Impulse);

                rigidbody_temp.constraints = RigidbodyConstraints.FreezeRotation;

                weaponManager.HideWeaponOnDeath();
                
                controller.lockCursor = false;

                isPlayerDead = true;

                Destroy(this);
            }
            else
                return;
        }
        
        void WritePlayerTransform()
        {
            playerPosition = gameObject.transform.position;
            playerRotation = gameObject.transform.rotation;
        }

        void DrawHealthStats()
        {
            if (healthUIText != null)
                healthUIText.text = health.ToString();

            if (healthUISlider != null)
                healthUISlider.value = health;

            if (hungerUIText != null)
                hungerUIText.text = satiety.ToString();

            if (hungerSlider != null) hungerSlider.value = satiety;

            if (thirstUIText != null)
                thirstUIText.text = hydratation.ToString();

            if (thirstSlider != null) thirstSlider.value = hydratation;


        }
    }

}