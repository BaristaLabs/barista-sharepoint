namespace Barista.EdgeJs.SharePoint
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class SPSiteInstance
    {
        public async Task<object> GetSubwebs(IDictionary<string, object> input)
        {
            //var auth = new ProjectServerAuth();
            //ExpandoMapper<ProjectServerAuth>.Map((ExpandoObject)input["auth"], auth);

            return await Task.Run(() => "");
        }
    }
}
