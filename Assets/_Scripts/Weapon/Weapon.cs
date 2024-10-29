using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    BoxCollider triggerBox;

    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Damage(damage, transform.position);
            DisableTriggerBox();
        }
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }
    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
}
