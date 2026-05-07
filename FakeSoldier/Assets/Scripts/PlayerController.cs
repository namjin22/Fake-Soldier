using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;

    // 외부(대화/선택지)에서 이동 잠금
    public bool IsMovementLocked { get; set; } = false;

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;

    // 마지막 방향 (Idle 시 방향 유지용)
    Vector2 lastDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (IsMovementLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 대각 이동 속도 정규화
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput != Vector2.zero)
            lastDirection = moveInput;

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void UpdateAnimation()
    {
        bool isMoving = moveInput != Vector2.zero;

        // Animator 파라미터 이름은 애니메이터 설정 시 맞춰서 수정
        anim.SetBool("IsMoving", isMoving);
        anim.SetFloat("DirX", lastDirection.x);
        anim.SetFloat("DirY", lastDirection.y);
    }

    // 이동 잠금/해제 (DialogueManager, ChoiceSystem 등에서 호출)
    public void LockMovement()  => IsMovementLocked = true;
    public void UnlockMovement() => IsMovementLocked = false;
}
