using System;
using Unity.Collections;
using Unity.Netcode;

public struct leaderboardEntityState : INetworkSerializable, IEquatable<leaderboardEntityState>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int Coins;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Coins);
    }

    public bool Equals(leaderboardEntityState other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && Coins == other.Coins;
    }
}