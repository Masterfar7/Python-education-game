using UnityEngine;

/// <summary>
/// Универсальная заготовка анимации ходьбы для персонажа / гида.
/// Просто вешай на объект с Rigidbody2D и Animator
/// и используй параметры в аниматоре:
/// - Float  Speed
/// - Bool   IsMoving
/// - Float  MoveX, MoveY
/// - Float  LastMoveX, LastMoveY
/// </summary>
public class CharacterAnimator2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float minMoveThreshold = 0.05f;

    private Vector2 lastMoveDir = Vector2.down;

    // Кэш наличия параметров в аниматоре, чтобы не падать, если их нет
    [Header("Parameter Names")]
    [SerializeField] private string isMovingParamName = "IsMoving";
    [SerializeField] private string speedParamName = "Speed";
    [SerializeField] private string moveXParamName = "MoveX";
    [SerializeField] private string moveYParamName = "MoveY";
    [SerializeField] private string lastMoveXParamName = "LastMoveX";
    [SerializeField] private string lastMoveYParamName = "LastMoveY";

    private bool hasIsMoving;
    private bool hasSpeed;
    private bool hasMoveX;
    private bool hasMoveY;
    private bool hasLastMoveX;
    private bool hasLastMoveY;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator != null)
        {
            foreach (var p in animator.parameters)
            {
                switch (p.name)
                {
                    // Поддерживаем как правильное имя, так и возможный вариант с опечаткой "IsMooving"
                    case "IsMoving":
                    case "IsMooving":
                        if (p.type == AnimatorControllerParameterType.Bool)
                        {
                            hasIsMoving = true;
                            isMovingParamName = p.name;
                        }
                        break;
                    case "Speed":
                        if (p.type == AnimatorControllerParameterType.Float)
                        {
                            hasSpeed = true;
                            speedParamName = p.name;
                        }
                        break;
                    case "MoveX":
                        if (p.type == AnimatorControllerParameterType.Float)
                        {
                            hasMoveX = true;
                            moveXParamName = p.name;
                        }
                        break;
                    case "MoveY":
                        if (p.type == AnimatorControllerParameterType.Float)
                        {
                            hasMoveY = true;
                            moveYParamName = p.name;
                        }
                        break;
                    case "LastMoveX":
                        if (p.type == AnimatorControllerParameterType.Float)
                        {
                            hasLastMoveX = true;
                            lastMoveXParamName = p.name;
                        }
                        break;
                    case "LastMoveY":
                        if (p.type == AnimatorControllerParameterType.Float)
                        {
                            hasLastMoveY = true;
                            lastMoveYParamName = p.name;
                        }
                        break;
                }
            }
        }
    }

    private void Update()
    {
        if (animator == null || rb == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        bool isMoving = speed > minMoveThreshold;

        if (hasIsMoving)
            animator.SetBool(isMovingParamName, isMoving);
        if (hasSpeed)
            animator.SetFloat(speedParamName, speed);

        if (isMoving)
        {
            Vector2 dir = velocity.normalized;
            lastMoveDir = dir;

            if (hasMoveX)
                animator.SetFloat(moveXParamName, dir.x);
            if (hasMoveY)
                animator.SetFloat(moveYParamName, dir.y);
            if (hasLastMoveX)
                animator.SetFloat(lastMoveXParamName, dir.x);
            if (hasLastMoveY)
                animator.SetFloat(lastMoveYParamName, dir.y);
        }
        else
        {
            if (hasMoveX)
                animator.SetFloat(moveXParamName, 0f);
            if (hasMoveY)
                animator.SetFloat(moveYParamName, 0f);
            if (hasLastMoveX)
                animator.SetFloat(lastMoveXParamName, lastMoveDir.x);
            if (hasLastMoveY)
                animator.SetFloat(lastMoveYParamName, lastMoveDir.y);
        }
    }
}

