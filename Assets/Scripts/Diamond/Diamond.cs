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
    public float InitialVelocity = 10;
    public float availableCountDown = 2;
    private float startTime;
    private void OnCollisionEnter(Collision collision)
    {
        if(Time.time - startTime < availableCountDown)
            return;
        if (collision.gameObject.CompareTag("Player"))
            GameManager.Instance.LoadNextLevel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(Time.time - startTime < availableCountDown)
            return;
        if(other.GameObject().CompareTag("player"))
            GameManager.Instance.LoadNextLevel();
    }

    private void Update()
    {
        if(Time.time - startTime < availableCountDown)
            return;
        var collider = Physics2D.OverlapCircleAll(transform.position, radius);
        if (Physics2D.OverlapCircleAll(transform.position, radius).Any(collider => collider.CompareTag("Player")))
        {
            GameManager.Instance.LoadNextLevel();
        }
    }

    private void Start()
    {
        startTime = Time.time;
    }
}
