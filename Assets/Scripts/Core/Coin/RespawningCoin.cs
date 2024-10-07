using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RespawningCoin : Coin
{
    public event Action<RespawningCoin> OnCollected;

    private Vector3 previsousPosition;

    private void Update()
    {
        if(previsousPosition != transform.position)
        {
            Show(true);
        }

        previsousPosition = transform.position;
    }

    public override int Collect()
    {
        if(!IsServer)
        {
            Show(false);
            return 0;
        }

        if(alreadyCollected){return 0;}
        alreadyCollected = true;

        OnCollected?.Invoke(this);

        return coinValue;
    }

    public void Reset()
    {
        alreadyCollected = false;
    }
}
