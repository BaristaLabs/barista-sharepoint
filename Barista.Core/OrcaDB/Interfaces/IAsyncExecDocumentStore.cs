namespace OFS.OrcaDB.Core
{
  using System;
  using System.Threading.Tasks;

  public interface IAsyncExecDocumentStore
  {
    Task ExecAsync(Action action);
  }
}
