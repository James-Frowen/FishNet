﻿using FishNet.Documenting;

namespace FishNet.Transporting
{

    /// <summary>
    /// PacketIds to indicate the type of packet which is being sent or arriving.
    /// </summary>
    [APIExclude]
    public enum PacketId : ushort
    {
        Unset = 0,
        Authenticated = 1,
        Split = 2,
        ObjectSpawn = 3,
        ObjectDespawn = 4,
        Event = 5,
        SyncVar = 6,
        ServerRpc = 7,
        ObserversRpc = 8,
        TargetRpc = 10,
        OwnershipChange = 11,
        Broadcast = 12,
        SyncObject = 13,
        PingPong = 14,
        Replicate = 15,
        Reconcile = 16,
        Disconnect = 17
    }

}