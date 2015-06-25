namespace Barista
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
    public interface IBaristaWebService
    {
        /// <summary>
        /// Executes the specified script and does not return a result.
        /// </summary>
        [OperationContract(Name = "ExecRest")]
        [WebGet(UriTemplate = "exec", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        void Coffee();

        /// <summary>
        /// Overload for coffee to allow http POSTS.
        /// </summary>
        [OperationContract(Name = "Exec")]
        [WebInvoke(Method = "POST", UriTemplate = "exec", BodyStyle = WebMessageBodyStyle.WrappedRequest,
          RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CoffeeAuLait(Stream stream);

        /// <summary>
        /// Evaluates the specified script.
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "EvalRest")]
        [WebGet(UriTemplate = "eval", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        Stream Espresso();

        /// <summary>
        /// Expresso overload to support having code contained in the body of a http POST.
        /// </summary>
        /// <returns></returns>
        [OperationContract(Name = "Eval")]
        [WebInvoke(Method = "POST", UriTemplate = "eval", BodyStyle = WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Latte(Stream stream);

        #region Wildcard variations
        [OperationContract(Name = "ExecRestWild")]
        [WebGet(UriTemplate = "exec/*", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        void CoffeeWild();

        [OperationContract(Name = "ExecWild")]
        [WebInvoke(Method = "POST", UriTemplate = "exec/*", BodyStyle = WebMessageBodyStyle.WrappedRequest,
          RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        void CoffeeAuLaitWild(Stream stream);

        [OperationContract(Name = "EvalRestWild")]
        [WebGet(UriTemplate = "eval/*", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        Stream EspressoWild();

        [OperationContract(Name = "EvalWild")]
        [WebInvoke(Method = "POST", UriTemplate = "eval/*", BodyStyle = WebMessageBodyStyle.WrappedRequest,
          RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream LatteWild(Stream stream);
        #endregion
    }
}
