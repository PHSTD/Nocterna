using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    public LayerMask enemyLayer;

    private Animator animator;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputActions = new PlayerInputActions();
        inputActions.Player.Attack.performed += _ => PerformAttack();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void PerformAttack()
    {
        animator.SetTrigger("attack");

        // 플레이어 중심에서 반경 내 모든 적 찾기
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Monster"))
            {
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
