namespace Barista.OrcaDB
{
  using System;
  using System.Threading.Tasks;

  public interface IAsyncExecDocumentStore
  {
    Task ExecAsync(Action action);
  }
}
