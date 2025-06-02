using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    private float m_moveSpeed = 5f;
    private float m_sitSpeed = 2f;
    private float m_jumpForce = 8f;

    private Rigidbody2D m_rb;
    private CapsuleCollider2D m_capsule;
    private PlayerInputActions m_inputActions;

    private Vector2 m_moveInput;
    private bool m_isGrounded = false;
    private bool m_isSitting = false;

    private Vector2 m_originalSize;
    private Vector2 m_sitSize;
    private Vector2 m_originalOffset;
    private Vector2 m_sitOffset;
    
    private Transform m_skeletalTransform;

    private Animator m_animator;
    
    private void Awake()
    {
        m_skeletalTransform ??= transform.Find("Skeletal");
        
        m_rb = GetComponent<Rigidbody2D>();
        m_capsule = GetComponent<CapsuleCollider2D>();

        m_animator = GetComponent<Animator>();
        
        m_originalSize = m_capsule.size;
        m_sitSize = new Vector2(m_originalSize.x, m_originalSize.y * 0.5f);
        m_originalOffset = m_capsule.offset;
        m_sitOffset = new Vector2(m_originalOffset.x, m_originalOffset.y - m_originalSize.y * 0.25f);

        m_inputActions = new PlayerInputActions();
    }

    private void OnEnable() => m_inputActions.Enable();
    private void OnDisable() => m_inputActions.Disable();

    private void Update()
    {
        m_moveInput = m_inputActions.Player.Move.ReadValue<Vector2>();

        // 이동 애니매이션
        bool isRun = Mathf.Abs(m_moveInput.x) > 0.1f;
        
        // 점프
        if (m_inputActions.Player.Jump.WasPressedThisFrame() && m_isGrounded && !m_isSitting)
        {
            m_rb.velocity = new Vector2(m_rb.velocity.x, m_jumpForce);
            m_isGrounded = false;
            m_animator.SetBool("isJump", true);
        }
        
        // 점프 중이면 걷기 상태로 안 바꾸게
        if (!m_animator.GetBool("isJump"))
        {
            m_animator.SetBool("isRun", isRun);
        }
        else
        {
            m_animator.SetBool("isRun", false); // 점프 중엔 걷기 false
        } 

       
       
        // 앉기
        if (m_inputActions.Player.Sit.IsPressed())
        {
            if (!m_isSitting)
            {
                StartSit();
            }
        }
        else
        {
            if (m_isSitting)
            {
                StopSit();
            }
        }
        
    }

    private void FixedUpdate()
    {
        // 이동
        float speed = m_isSitting ? m_sitSpeed : m_moveSpeed;
        m_rb.velocity = new Vector2(m_moveInput.x * speed, m_rb.velocity.y);
        
        // 반전 
        if (m_moveInput.x != 0)
        {
            Vector3 scale = m_skeletalTransform.localScale;
            scale.x = Mathf.Abs(scale.x) * (m_moveInput.x < 0 ? -1 : 1);
            m_skeletalTransform.localScale = scale;
        }
    }

    private void StartSit()
    {
        m_isSitting = true;
        m_capsule.size = m_sitSize;
        m_capsule.offset = m_sitOffset;
    }

    private void StopSit()
    {
        m_isSitting = false;
        m_capsule.size = m_originalSize;
        m_capsule.offset = m_originalOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                m_isGrounded = true;
                m_animator.SetBool("isJump", false);
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_isGrounded = false;
    }
}
