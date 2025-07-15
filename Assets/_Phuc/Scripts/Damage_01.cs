using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage_01 : MonoBehaviour
{
    public float damage = 1f;

    /* private void OnTriggerEnter2D(Collider2D other)
     {
         if (other.CompareTag("Player"))
         {
             playerHealth pHealth = other.GetComponent<playerHealth>();
             if (pHealth != null)
             {
                 pHealth.TakeDamage(damage);
             }
         }
     }*/

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth pHealth = other.GetComponent<playerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);
            }
        }
    }
}
