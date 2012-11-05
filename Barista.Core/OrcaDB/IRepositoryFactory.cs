using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.OrcaDB
{
  public interface IRepositoryFactory
  {
    Repository CreateRepository();

    Repository CreateRepository(IDocumentStore documentStore);
  }
}
