using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpLogic : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Police"))
        {
            Debug.Log("Power-up collected! (trigger)");
            PowerUpSpawner.PlaceSinglePowerUp();
            Destroy(gameObject); 
        }
    }
}
