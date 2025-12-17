using System;
using UnityEngine;

public class Sprites2D : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }}
