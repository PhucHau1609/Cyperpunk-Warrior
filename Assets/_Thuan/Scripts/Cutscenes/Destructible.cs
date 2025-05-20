using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private string targetTag = "FireBall"; 
    [SerializeField] private float delay = 0.5f; 

    private bool isDestroyed = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed) return;

        if (collision.gameObject.CompareTag(targetTag))
        {
            isDestroyed = true;
            Destroy(gameObject, delay);
        }
    }
}
