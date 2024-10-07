using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;

    [SerializeField] private LobbyItem lobbyItemPrefab;

    [SerializeField] private Transform LobbyItemParent;

    private bool isRefreshing;
    
    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if(isRefreshing) {return;}

        isRefreshing = true;

        QueryLobbiesOptions options = new QueryLobbiesOptions();
        options.Count = 25;
        try
        {
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);

            foreach(Transform child in LobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Lobby lobby in lobbies.Results)
            {
                LobbyItem lobbyItem = Instantiate(lobbyItemPrefab, LobbyItemParent);
                lobbyItem.Initialise(this, lobby);
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isRefreshing = false;
    }

    public void JoinAsync(Lobby lobby)
    {
        mainMenu.JoinAsync(lobby);
    }
}
