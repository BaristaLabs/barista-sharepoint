﻿namespace Barista.SocketBase
{
  using Barista.SocketBase.Command;
  using Barista.SocketBase.Protocol;

  /// <summary>
  /// Command Executing Context
  /// </summary>
  public class CommandExecutingContext
  {
    /// <summary>
    /// Gets the session.
    /// </summary>
    public IAppSession Session { get; private set; }

    /// <summary>
    /// Gets the request info.
    /// </summary>
    public IRequestInfo RequestInfo { get; private set; }

    /// <summary>
    /// Gets the current command.
    /// </summary>
    public ICommand CurrentCommand { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this command executing is cancelled.
    /// </summary>
    /// <value>
    ///   <c>true</c> if cancel; otherwise, <c>false</c>.
    /// </value>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandExecutingContext"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    /// <param name="command">The command.</param>
    public CommandExecutingContext(IAppSession session, IRequestInfo requestInfo, ICommand command)
    {
      Session = session;
      RequestInfo = requestInfo;
      CurrentCommand = command;
    }
  }
}
