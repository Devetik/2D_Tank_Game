using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ParticleSystem dustCloudLeft;
    [SerializeField] private ParticleSystem dustCloudRight;
    [Header("Serrings")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float particleEmmisionValue = 10f;

    private ParticleSystem.EmissionModule emissionModuleLeft;
    private ParticleSystem.EmissionModule emissionModuleRight;

    private Vector2 previousMovementInput;

    private Vector3 previousPos;

    private const float ParticleStopThreshold = 0.005f;

    private void Awake()
    {
        emissionModuleLeft = dustCloudLeft.emission;

        emissionModuleRight = dustCloudRight.emission; // A supprimer ou créer un second emissionModule
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner){return;}

        inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner){return;}

        inputReader.MoveEvent -= HandleMove;
    }

    private void Update()
    {
        if(!IsOwner){return;}

        float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
        bodyTransform.Rotate(0f, 0f, zRotation);
    }

    private void FixedUpdate()
    {
        if((transform.position - previousPos).sqrMagnitude > ParticleStopThreshold)
        {
            emissionModuleLeft.rateOverTime = particleEmmisionValue;
            emissionModuleRight.rateOverTime = particleEmmisionValue;
        }
        else
        {
            emissionModuleLeft.rateOverTime = 0f;
            emissionModuleRight.rateOverTime = 0f;
        }
        previousPos = transform.position;

        if(!IsOwner){return;}

        rb.velocity = (Vector2)bodyTransform.up * previousMovementInput.y * movementSpeed;
    }

    private void HandleMove(Vector2 movementInput)
    {
        previousMovementInput = movementInput;
    }
}
