using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;

    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

    [SerializeField] private int entitesToDisplay = 8;

    private NetworkList<leaderboardEntityState> leaderboardEntities;

    private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

    private void Awake()
    {
        leaderboardEntities = new NetworkList<leaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach(leaderboardEntityState entity in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<leaderboardEntityState>
                {
                    Type = NetworkListEvent<leaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if(IsServer)
        {
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach(TankPlayer player in players)
            {
                HandlerPlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlerPlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        if(!IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlerPlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<leaderboardEntityState> changeEvent)
    {
        if(!gameObject.scene.isLoaded){return;}
        
        switch(changeEvent.Type)
        {
            case NetworkListEvent<leaderboardEntityState>.EventType.Add:
                if(!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    leaderboardEntity.Initialise(
                        changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<leaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<leaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }

        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));

        for(int i = 0; i < entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            entityDisplays[i].gameObject.SetActive(i <= entitesToDisplay - 1);
        }

        LeaderboardEntityDisplay myDisplay = entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if(myDisplay != null)
        {
            if(myDisplay.transform.GetSiblingIndex() >= entitesToDisplay)
            {
                leaderboardEntityHolder.GetChild(entitesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandlerPlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new leaderboardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        if(leaderboardEntities == null){return;}

        foreach(leaderboardEntityState entity in leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId){continue;}

            leaderboardEntities.Remove(entity);
            break;
        }

        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) => HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        for(int i = 0; i < leaderboardEntities.Count; i++)
        {
            if(leaderboardEntities[i].ClientId != clientId){continue;}

            leaderboardEntities[i] = new leaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName, 
                Coins = newCoins
            };

            return;
        }
    }
}
