namespace Barista.DocumentStore
{
  using System;
  using System.Threading.Tasks;

  public interface IAsyncExecDocumentStore
  {
    Task ExecAsync(Action action);
  }
}
