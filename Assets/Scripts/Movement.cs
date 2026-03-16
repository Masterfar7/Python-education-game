using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Movement Tuning")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;

    [Header("Collision")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float rayOffsetSize = 0.4f;
    [SerializeField] private float raySkin = 0.05f; // небольшой запас до стены

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = true;
    }

    private void Update()
    {
        // Считываем вход
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Нормализуем, чтобы по диагонали скорость не была выше
        moveInput = moveInput.normalized;

        // Отражение спрайта
        if (moveInput.x < 0f)
            spriteRenderer.flipX = false;
        else if (moveInput.x > 0f)
            spriteRenderer.flipX = true;
    }

    private void FixedUpdate()
    {
        // Целевая скорость по вводу
        Vector2 targetVelocity = moveInput * moveSpeed;

        // Разные ускорения для разгона и торможения
        float accel = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

        // Плавно приближаем текущую скорость к целевой
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 newVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, accel * Time.fixedDeltaTime);

        // Проверяем коллизии по каждой оси, учитывая расстояние за кадр
        Vector2 adjustedVelocity = newVelocity;

        // Горизонталь
        if (!Mathf.Approximately(newVelocity.x, 0f))
        {
            float dirX = Mathf.Sign(newVelocity.x);
            float distanceX = Mathf.Abs(newVelocity.x) * Time.fixedDeltaTime + raySkin;

            if (IsBlocked(Vector2.right * dirX, distanceX))
                adjustedVelocity.x = 0f;
        }

        // Вертикаль
        if (!Mathf.Approximately(newVelocity.y, 0f))
        {
            float dirY = Mathf.Sign(newVelocity.y);
            float distanceY = Mathf.Abs(newVelocity.y) * Time.fixedDeltaTime + raySkin;

            if (IsBlocked(Vector2.up * dirY, distanceY))
                adjustedVelocity.y = 0f;
        }

        rb.linearVelocity = adjustedVelocity;
    }

    private bool IsBlocked(Vector2 direction, float distance)
    {
        Vector2 origin = rb.position;
        Vector2 dirNormalized = direction.normalized;
        Vector2 perpendicular = Vector2.Perpendicular(dirNormalized).normalized * rayOffsetSize;

        bool hit1 = Physics2D.Raycast(origin + perpendicular, dirNormalized, distance, wallLayer);
        bool hit2 = Physics2D.Raycast(origin - perpendicular, dirNormalized, distance, wallLayer);

        return hit1 || hit2;
    }
}