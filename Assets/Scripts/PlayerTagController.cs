using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTagController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tagSfx;

    // Statics to track across all players
    private static int globalTayaCount = 0;
    private static float sceneStartTime = 0f;

    [Header("Player State")]
    public bool isIt = false;

    [Header("Score System")]
    public int score = 0;
    public string playerName = "Player";

    [Header("3D Models")]
    public GameObject normalModel;
    public GameObject itModel;

    [Header("Speed Settings")]
    public float normalSpeed = 6f;
    public float itSpeed = 8f;

    [Header("Tag & Shove Mechanics")]
    public float tagRadius = 2.5f;
    public float knockbackForce = 15f;
    public float tagCooldown = 2f;
    public float stunDuration = 3f;

    private float currentCooldown = 0f;
    private Rigidbody rb;
    private PlayerMovement movementScript;
    private PlayerAppearance appearanceScript;

    void Awake()
    {
        // Reset the counter ONLY if this is a fresh scene load
        if (Time.timeSinceLevelLoad < 0.1f)
        {
            globalTayaCount = 0;
            sceneStartTime = Time.time;
        }

        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerMovement>();
        appearanceScript = GetComponent<PlayerAppearance>();
    }

    void Start()
    {
        playerName = "Player " + (GetComponent<PlayerInput>().playerIndex + 1);
        UpdateForm();

        if (movementScript != null)
            movementScript.SetSpeed(isIt ? itSpeed : normalSpeed);

        if (DynamicCamera.Instance != null)
            DynamicCamera.Instance.AddPlayer(this.transform);
    }

    void Update()
    {
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
    }

    // --- RESTORED THIS METHOD ---
    public void AddScore(int amount)
    {
        score += amount;
        if (appearanceScript != null)
        {
            appearanceScript.AddScoreToHUD(score);
        }
    }

    public void BecomeIt(Vector3 knockbackAmount)
    {
        globalTayaCount++;

        // Sound Logic: Skip sound if it's the very first assignment OR during intro cutscene
        bool isIntroSelection = (Time.time - sceneStartTime) < 5.0f;

        if (globalTayaCount > 1 && !isIntroSelection)
        {
            if (audioSource != null && tagSfx != null)
            {
                audioSource.PlayOneShot(tagSfx);
            }
        }

        isIt = true;
        currentCooldown = tagCooldown;
        UpdateForm();

        if (appearanceScript != null) appearanceScript.UpdateVisuals();
        if (movementScript != null) movementScript.SetSpeed(itSpeed);
        if (movementScript != null && knockbackAmount != Vector3.zero)
            movementScript.ApplyKnockback(knockbackAmount, stunDuration);
    }

    public void BecomeNormal()
    {
        isIt = false;
        UpdateForm();
        if (appearanceScript != null) appearanceScript.UpdateVisuals();
        if (movementScript != null) movementScript.SetSpeed(normalSpeed);
    }

    void OnTag()
    {
        if (currentCooldown > 0) return;
        if (movementScript != null && movementScript.anim != null)
            movementScript.anim.SetTrigger("Tag");

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, tagRadius);
        foreach (Collider hit in hitColliders)
        {
            PlayerTagController targetPlayer = hit.GetComponent<PlayerTagController>();
            if (targetPlayer != null && targetPlayer.gameObject != this.gameObject)
            {
                PlayerMovement targetMovement = targetPlayer.GetComponent<PlayerMovement>();
                Vector3 pushDirection = (targetPlayer.transform.position - transform.position).normalized;
                pushDirection.y = 0.5f;

                if (isIt)
                {
                    if (!targetPlayer.isIt)
                    {
                        this.BecomeNormal();
                        targetPlayer.BecomeIt(pushDirection * knockbackForce);
                        break;
                    }
                }
                else
                {
                    if (!targetPlayer.isIt)
                    {
                        targetMovement.ApplyKnockback(pushDirection * knockbackForce, stunDuration);
                        currentCooldown = tagCooldown;
                        break;
                    }
                }
            }
        }
    }

    void UpdateForm()
    {
        Animator activeAnim = null;
        if (isIt)
        {
            normalModel.SetActive(false);
            itModel.SetActive(true);
            activeAnim = itModel.GetComponent<Animator>();
        }
        else
        {
            itModel.SetActive(false);
            normalModel.SetActive(true);
            activeAnim = normalModel.GetComponent<Animator>();
        }
        if (movementScript != null && activeAnim != null)
            movementScript.SetActiveAnimator(activeAnim);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tagRadius);
    }
}