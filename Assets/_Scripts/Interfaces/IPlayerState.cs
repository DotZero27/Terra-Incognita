public interface IPlayerState : IDamageable
{
    bool isAttacking { get; set; }
    bool isDashing { get; set; }
}
