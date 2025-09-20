using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.spawnPoint = transform; // Set checkpoint as respawn
            Debug.Log("Checkpoint reached by Player " + player.playerID);
        }
    }
}