using System.Collections;
using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;
using Unity.Netcode;
using UnityEngine;
using System;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinWallet wallet;
    [SerializeField] private Transform projectilSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;

    [SerializeField] private GameObject muzzleFlash; 
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectilSpeed;
    [SerializeField] private float fireRare;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;
    private float timer;
    private float muzzleFlashTimer;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner){return;}

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner){return;}

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void Update()
    {
        if(muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if(muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if(!IsOwner)    {return;}
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if(!shouldFire) {return;}
        if(timer > 0 )  {return;}

        if(wallet.TotalCoins.Value < costToFire) {return;}

        PrimaryFireServerRpc(projectilSpawnPoint.position, projectilSpawnPoint.up);

        SpawnDummyProjectile(projectilSpawnPoint.position, projectilSpawnPoint.up);

        timer = 1 / fireRare;
    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if(wallet.TotalCoins.Value < costToFire) {return;}

        wallet.SpendCoins(costToFire);

        GameObject projectilInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);

        projectilInstance.transform.up = direction; 

        Physics2D.IgnoreCollision(playerCollider, projectilInstance.GetComponent<Collider2D>());

        if(projectilInstance.TryGetComponent<DealDomageOnContact>(out DealDomageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if(projectilInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectilSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if(IsOwner){return;}

        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectilInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);

        projectilInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectilInstance.GetComponent<Collider2D>());

        if(projectilInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectilSpeed;
        }
    }
}
