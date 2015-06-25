namespace Barista.Framework
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.ServiceModel.Description;
  using System.Web.Services.Description;
  using System.Xml;

  public class DynamicResponseTypeAttribute : Attribute, IOperationBehavior, IWsdlExportExtension
  {
    private const string WsdlPolicyNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";
    private const string WsSecurityUtilityNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
    private readonly List<string> m_operationsToRemove = new List<string>();

    /// <summary>
    /// Gets or sets a value that, if true, does not include the service operation in the generated WSDL.
    /// </summary>
    public bool RestOnly
    {
      get;
      set;
    }

    public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyClientBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
    {
    }

    public void ApplyDispatchBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation)
    {
      if (this.RestOnly)
      {
        m_operationsToRemove.Add(operationDescription.Name);
      }
    }

    public void Validate(OperationDescription operationDescription)
    {
    }

    public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
    {
      //Do Nothing.
    }

    public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
    {
      foreach (var wsdl in exporter.GeneratedWsdlDocuments.OfType<System.Web.Services.Description.ServiceDescription>())
      {
        List<string> messageNamesToRemove;
        RemoveOperationsFromPortTypes(wsdl, out messageNamesToRemove);
        List<string> policiesToRemove;
        RemoveOperationsFromBindings(wsdl, out policiesToRemove);
        RemoveWsdlMessages(wsdl, messageNamesToRemove);
        RemoveOperationRelatedPolicies(wsdl, policiesToRemove);
      }
    }

    private static void RemoveWsdlMessages(System.Web.Services.Description.ServiceDescription wsdl, List<string> messageNamesToRemove)
    {
      for (int i = wsdl.Messages.Count - 1; i >= 0; i--)
      {
        if (messageNamesToRemove.Contains(wsdl.Messages[i].Name))
        {
          wsdl.Messages.RemoveAt(i);
        }
      }
    }

    private void RemoveOperationsFromBindings(System.Web.Services.Description.ServiceDescription wsdl, out List<string> policiesToRemove)
    {
      policiesToRemove = new List<string>();
      foreach (System.Web.Services.Description.Binding binding in wsdl.Bindings)
      {
        for (int i = binding.Operations.Count - 1; i >= 0; i--)
        {
          if (m_operationsToRemove.Contains(binding.Operations[i].Name))
          {
            string inputPolicy = GetPolicyReferences(binding.Operations[i].Input);
            if (inputPolicy != null)
            {
              policiesToRemove.Add(inputPolicy);
            }

            string outputPolicy = GetPolicyReferences(binding.Operations[i].Output);
            if (outputPolicy != null)
            {
              policiesToRemove.Add(outputPolicy);
            }

            binding.Operations.RemoveAt(i);
          }
        }
      }
    }

    private void RemoveOperationsFromPortTypes(System.Web.Services.Description.ServiceDescription wsdl, out List<string> messageNamesToRemove)
    {
      messageNamesToRemove = new List<string>();
      foreach (System.Web.Services.Description.PortType portType in wsdl.PortTypes)
      {
        for (int i = portType.Operations.Count - 1; i >= 0; i--)
        {
          if (m_operationsToRemove.Contains(portType.Operations[i].Name))
          {
            messageNamesToRemove.AddRange(from OperationMessage operationMessage in portType.Operations[i].Messages select operationMessage.Message.Name);

            portType.Operations.RemoveAt(i);
          }
        }
      }
    }

    private static void RemoveOperationRelatedPolicies(System.Web.Services.Description.ServiceDescription wsdl, List<string> policiesToRemove)
    {
      for (int i = wsdl.Extensions.Count - 1; i >= 0; i--)
      {
        XmlElement extension = wsdl.Extensions[i] as XmlElement;
        if (extension != null && extension.LocalName == "Policy"
            && extension.NamespaceURI == WsdlPolicyNamespace)
        {
          XmlAttribute id = extension.Attributes["Id", WsSecurityUtilityNamespace];
          if (id != null && policiesToRemove.Contains(id.Value))
          {
            wsdl.Extensions.RemoveAt(i);
          }
        }
      }
    }

    private static string GetPolicyReferences(System.Web.Services.Description.MessageBinding binding)
    {
      if (binding != null)
      {
        var extensions = binding.Extensions;
        if (extensions.Count > 0)
        {
          XmlElement extensionElement = extensions[0] as XmlElement;
          if (extensionElement != null)
          {
            if (extensionElement.LocalName == "PolicyReference"
                && extensionElement.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy")
            {
              XmlAttribute uri = extensionElement.Attributes["URI"];
              if (uri != null)
              {
                string uriValue = uri.Value;
                if (uriValue.StartsWith("#"))
                {
                  return uriValue.Substring(1);
                }
              }
            }
          }
        }
      }

      return null;
    }
  }
}
