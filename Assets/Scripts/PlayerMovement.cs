using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;

    [Header("Animation")]
    public Animator anim;
    public bool canMove = true;
    public bool isPlayingCutscene = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip knockbackSound;

    private Vector2 moveInput;
    private Rigidbody rb;
    private float stunTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (playerInput != null && spawnPoints.Length > 0)
        {
            int index = playerInput.playerIndex % spawnPoints.Length;
            transform.position = spawnPoints[index].transform.position;
        }

        if (MatchFlowManager.Instance != null)
        {
            isPlayingCutscene = !MatchFlowManager.Instance.matchHasStarted;
        }
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        if (transform.parent != null && transform.parent.CompareTag("KaritonSeat"))
        {
            KaritonController kariton = GetComponentInParent<KaritonController>();
            if (kariton != null)
            {
                kariton.SetMoveInput(input);
                moveInput = Vector2.zero;
                return;
            }
        }

        if (canMove && !isPlayingCutscene) moveInput = input;
        else moveInput = Vector2.zero;
    }

    void OnRide()
    {
        // EXIT LOGIC: If we have a parent, we are likely in the cart
        if (transform.parent != null)
        {
            KaritonController currentKariton = GetComponentInParent<KaritonController>();
            if (currentKariton != null)
            {
                currentKariton.RequestRide(this.gameObject); // This will trigger ExitCart()
                return;
            }
        }

        // ENTER LOGIC: Look for a nearby cart
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out KaritonController kariton))
            {
                kariton.RequestRide(this.gameObject);
                break;
            }
        }
    }

    void OnJump(InputValue value)
    {
        // BUG FIX: Stop error when in cart
        if (rb.isKinematic) return;

        if (canMove && !isPlayingCutscene && value.isPressed && stunTimer <= 0 && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (anim != null) anim.SetTrigger("Jump");
            if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
        }
    }

    void FixedUpdate()
    {
        // BUG FIX: Stop "Setting velocity of kinematic body" warning
        if (rb.isKinematic) return;

        if (stunTimer > 0)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0) canMove = true;
            return;
        }

        if (canMove && !isPlayingCutscene)
        {
            Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
            movement.y = rb.velocity.y;
            rb.velocity = movement;

            if (moveInput != Vector2.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0f, moveInput.y));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
            }
            if (anim != null) anim.SetFloat("Speed", moveInput.magnitude);
        }
        else if (anim != null) anim.SetFloat("Speed", 0f);
    }

    public void ApplyKnockback(Vector3 force, float stunDuration)
    {
        // BUG FIX: Don't apply physics if we are in the cart!
        if (rb.isKinematic) return;

        stunTimer = stunDuration;
        canMove = false;
        moveInput = Vector2.zero;
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        if (anim != null) anim.SetTrigger("Knockback");
        if (audioSource != null && knockbackSound != null) audioSource.PlayOneShot(knockbackSound);
    }

    bool IsGrounded() => Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, groundCheckDistance, groundLayer);
    public void SetActiveAnimator(Animator newAnim) => anim = newAnim;
    public void ResetInput() => moveInput = Vector2.zero;
    public void SetSpeed(float newSpeed) => moveSpeed = newSpeed;
}