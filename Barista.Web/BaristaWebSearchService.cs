namespace Barista.Web
{
    using Barista.Search;
    using System.ServiceModel;

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class BaristaWebSearchService : BaristaSearchService
    {

    }
}
