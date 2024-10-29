using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Normal Atk")]
public class AttackSO : ScriptableObject
{
    public AnimatorOverrideController animatorOV;
    public float damage;
    public float attackSpeed = 1f;
}