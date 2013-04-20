﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// An item can be started and stopped
    /// </summary>
    public interface IWorkItem
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Setups with the specified root config.
        /// </summary>
        /// <param name="bootstrap">The bootstrap.</param>
        /// <param name="config">The socket server instance config.</param>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        bool Setup(IBootstrap bootstrap, IServerConfig config, ProviderFactoryInfo[] factories);


        /// <summary>
        /// Starts this server instance.
        /// </summary>
        /// <returns>return true if start successfull, else false</returns>
        bool Start();


        /// <summary>
        /// Gets the current state of the work item.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        ServerState State { get; }

        /// <summary>
        /// Stops this server instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the total session count.
        /// </summary>
        int SessionCount { get; }


        /// <summary>
        /// Gets the state of the server.
        /// </summary>
        /// <value>
        /// The state of the server.
        /// </value>
        ServerSummary Summary { get; }


        /// <summary>
        /// Collects the server summary.
        /// </summary>
        /// <param name="nodeSummary">The node summary.</param>
        /// <returns></returns>
        ServerSummary CollectServerSummary(NodeSummary nodeSummary);
    }
}
