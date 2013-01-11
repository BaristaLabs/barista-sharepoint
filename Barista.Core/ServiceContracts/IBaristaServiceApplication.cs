namespace Barista
{
  using System.ServiceModel;
  using System.Text;

  [ServiceContract(Namespace = Constants.ServiceNamespace)]
  [ServiceKnownType(typeof(UTF8Encoding))]
  [ServiceKnownType(typeof(EncoderReplacementFallback))]
  [ServiceKnownType(typeof(DecoderReplacementFallback))]
  public interface IBaristaServiceApplication
  {
    [OperationContract]
    BrewResponse Eval(BrewRequest request);

    [OperationContract]
    void Exec(BrewRequest request);

    [OperationContract]
    void AddObjectToIndex(string indexUrl, bool createIndex, string json);
  }
}