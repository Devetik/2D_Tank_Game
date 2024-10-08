using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private AudioSource tankThreadSound;

    [SerializeField] private AudioSource canonShootSound;

    [SerializeField] private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tankThreadSound.loop = true;
        tankThreadSound.Play();
    }

    void Update()
    {
        if (rb.velocity.magnitude > 0.01f)
        {
            tankThreadSound.volume = 0.15f;
        }
        else
        {
            tankThreadSound.volume = 0.1f;
        }
    }

    public void ShootCanon(Vector3 position)
    {
        canonShootSound.transform.position = position;
        canonShootSound.Play();
    }
}
