﻿using FishNet.Managing.Server;
using FishNet.Object.Helping;
using FishNet.Serializing;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FishNet.Object
{

    public abstract partial class NetworkBehaviour : MonoBehaviour
    {
        #region Types.
        private struct RpcLinkType
        {
            /// <summary>
            /// Index of link.
            /// </summary>
            public ushort LinkIndex;
            /// <summary>
            /// Type of Rpc link is for.
            /// </summary>
            public RpcType RpcType;

            public RpcLinkType(ushort linkIndex, RpcType rpcType)
            {
                LinkIndex = linkIndex;
                RpcType = rpcType;
            }
        }

        #endregion

        #region Private.        
        /// <summary>
        /// Link indexes for RPCs.
        /// </summary>
        private Dictionary<uint, RpcLinkType> _rpcLinks = new Dictionary<uint, RpcLinkType>();
        #endregion

        /// <summary>
        /// Initializes RpcLinks. This will only call once even as host.
        /// </summary>
        private void InitializeOnceRpcLinks()
        {
            if (IsServer)
            {
                /* Link only data from server to clients. While it is
                 * just as easy to link client to server it's usually
                 * not needed because server out data is more valuable
                 * than server in data. */
                /* Links will be stored in the NetworkBehaviour so that
                 * when the object is destroyed they can be added back
                 * into availableRpcLinks, within the ServerManager. */

                ServerManager serverManager = NetworkManager.ServerManager;
                //ObserverRpcs.
                foreach (uint rpcHash in _observersRpcDelegates.Keys)
                {
                    if (!MakeLink(rpcHash, RpcType.Observers))
                        return;
                }
                //TargetRpcs.
                foreach (uint rpcHash in _targetRpcDelegates.Keys)
                {
                    if (!MakeLink(rpcHash, RpcType.Target))
                        return;
                }
                //ReconcileRpcs.
                foreach (uint rpcHash in _reconcileRpcDelegates.Keys)
                {
                    if (!MakeLink(rpcHash, RpcType.Reconcile))
                        return;
                }

                /* Tries to make a link and returns if
                 * successful. When a link cannot be made the method
                 * should exit as no other links will be possible. */
                bool MakeLink(uint rpcHash, RpcType rpcType)
                {
                    if (serverManager.GetRpcLink(out ushort linkIndex))
                    {
                        _rpcLinks[rpcHash] = new RpcLinkType(linkIndex, rpcType);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a PooledWriter and writes the header for a rpc.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="rpcHash"></param>
        /// <param name="packetId"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PooledWriter CreateLinkedRpc(RpcLinkType link, PooledWriter methodWriter, Channel channel)
        {
            PooledWriter writer = WriterPool.GetWriter();
            writer.WriteUInt16(link.LinkIndex);
            ////Write length only if unreliable.
            //if (channel == Channel.Unreliable)
            //    WriteUnreliableLength(writer, methodWriter.Length);
            //Data.
            writer.WriteArraySegment(methodWriter.GetArraySegment());

            return writer;
        }


        /// <summary>
        /// Writes rpcLinks to writer.
        /// </summary>
        internal void WriteRpcLinks(PooledWriter writer)
        {
            PooledWriter rpcLinkWriter = WriterPool.GetWriter();
            foreach (KeyValuePair<uint, RpcLinkType> item in _rpcLinks)
            {
                //RpcLink index.
                rpcLinkWriter.WriteUInt16(item.Value.LinkIndex);
                //Hash.
                rpcLinkWriter.WriteUInt16((ushort)item.Key);
                //True/false if observersRpc.
                rpcLinkWriter.WriteByte((byte)item.Value.RpcType);
            }

            writer.WriteBytesAndSize(rpcLinkWriter.GetBuffer(), 0, rpcLinkWriter.Length);
            rpcLinkWriter.Dispose();
        }
    }
}

