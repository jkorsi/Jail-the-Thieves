using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    int health = 10;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("I'm dead!");
    }

    public void ResetHealth()
    {
        health = 10;
    }

    public int GetHealth()
    {
        return health;
    }

}
