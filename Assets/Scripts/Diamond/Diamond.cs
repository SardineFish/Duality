using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duality;
using Unity.VisualScripting;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    public float radius = 0.5f;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            GameManager.Instance.LoadNextLevel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GameObject().CompareTag("player"))
            GameManager.Instance.LoadNextLevel();
    }

    private void Update()
    {
        var collider = Physics2D.OverlapCircleAll(transform.position, radius);
        if (Physics2D.OverlapCircleAll(transform.position, radius).Any(collider => collider.CompareTag("Player")))
        {
            GameManager.Instance.LoadNextLevel();
        }
    }
}
