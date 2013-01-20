namespace Barista.SocketBase.Command
{
  using System.Collections.Generic;
  using System.Linq;

  public class CommandInfo<TCommand>
      where TCommand : ICommand
  {
    public TCommand Command { get; private set; }

    public CommandFilterAttribute[] Filters { get; private set; }

    public CommandInfo(TCommand command, IEnumerable<CommandFilterAttribute> globalFilters)
    {
      Command = command;

      var allFilters = new List<CommandFilterAttribute>();

      var commandFilterAttributes = globalFilters as IList<CommandFilterAttribute> ?? globalFilters.ToList();

      if (globalFilters != null && commandFilterAttributes.Any())
      {
        allFilters.AddRange(commandFilterAttributes);
      }

      var filters = AppServer.GetCommandFilterAttributes(command.GetType());

      if (filters.Any())
      {
        allFilters.AddRange(filters);
      }

      if (allFilters.Any())
      {
        Filters = allFilters.OrderBy(f => f.Order).ToArray();
      }
    }
  }
}
