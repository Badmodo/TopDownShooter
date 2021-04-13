using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float health = 10;
    public float currentHealth;

    public virtual void TakeDamage(float damage)
    {
        currentHealth = health -= damage;

        if(health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
