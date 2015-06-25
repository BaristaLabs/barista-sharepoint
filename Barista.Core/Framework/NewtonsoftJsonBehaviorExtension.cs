namespace Barista.Framework
{
  using System;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Configuration;

  public class NewtonsoftJsonBehaviorExtension : BehaviorExtensionElement
  {
    public override Type BehaviorType
    {
      get { return typeof(NewtonsoftJsonBehavior); }
    }

    protected override object CreateBehavior()
    {
      return new NewtonsoftJsonBehavior();
    }
  }

  public class NewtonsoftJsonContentTypeMapper : WebContentTypeMapper
  {
    public override WebContentFormat GetMessageFormatForContentType(string contentType)
    {
      return WebContentFormat.Raw;
    }
  }
}
