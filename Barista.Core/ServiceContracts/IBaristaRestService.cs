namespace Barista
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
    public interface IBaristaRestService
    {
        /// <summary>
        /// Execute and don't return a response.
        /// </summary>
        [OperationContract(Name = "Exec")]
        [WebInvoke(Method = "*", UriTemplate = "exec")]
        void Exec(Stream requestBody);

        /// <summary>
        /// Execute and don't return a response with wildcard
        /// </summary>
        [OperationContract(Name = "ExecWild")]
        [WebInvoke(Method = "*", UriTemplate = "exec/*")]
        void ExecWild(Stream requestBody);

        /// <summary>
        /// Evaluate and return a response
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "Eval")]
        [WebInvoke(Method = "*", UriTemplate = "eval")]
        Message Eval(Stream requestBody);

        /// <summary>
        /// Evaluate and return a response with wildcard
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "EvalWild")]
        [WebInvoke(Method = "*", UriTemplate = "eval/*")]
        Message EvalWild(Stream requestBody);

        /// <summary>
        /// Gets status about the current environment
        /// </summary>
        [OperationContract(Name = "Status")]
        [WebInvoke(Method = "GET", UriTemplate = "status")]
        Message Status();

        /// <summary>
        /// Gets a list of all installed bundles
        /// </summary>
        [OperationContract(Name = "ListBundles")]
        [WebInvoke(Method = "GET", UriTemplate = "listBundles")]
        Message ListBundles();

        /// <summary>
        /// Deploys a bundle to the hosting environment
        /// </summary>
        [OperationContract(Name = "DeployBundle")]
        [WebInvoke(Method = "POST", UriTemplate = "deployBundle")]
        Message DeployBundle(Stream stream);
    }
}
