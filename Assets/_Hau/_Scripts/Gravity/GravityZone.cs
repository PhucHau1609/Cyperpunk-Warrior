using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerGravityController gravityController = collision.GetComponent<PlayerGravityController>();
            if (gravityController != null)
            {
                gravityController.InvertGravity();
            }
        }
    }
}

