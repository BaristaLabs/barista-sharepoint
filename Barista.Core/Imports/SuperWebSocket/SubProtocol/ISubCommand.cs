using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Barista.SuperSocket.SocketBase;
using Barista.SuperSocket.SocketBase.Command;
using Barista.SuperSocket.SocketBase.Protocol;

namespace Barista.SuperWebSocket.SubProtocol
{
    /// <summary>
    /// SubCommand interface
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public interface ISubCommand<TWebSocketSession> : ICommand
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo);
    }
}
