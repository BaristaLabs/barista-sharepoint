namespace Barista.SharePoint.Library
{
  using System.Linq;
  using System.Reflection;
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System;
  using System.IO;

  [Serializable]
  public class SPExcelDocumentConstructor : ClrFunction
  {
    public SPExcelDocumentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelDocument", new SPExcelDocumentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPExcelDocumentInstance Construct(params object[] parameters)
    {
      var firstArg = parameters.FirstOrDefault();

      SPExcelDocumentInstance documentInstance;
      if (firstArg is Base64EncodedByteArrayInstance)
      {
        //Create the excel document instance from a byte array.
        var byteArray = firstArg as Base64EncodedByteArrayInstance;
        using (var ms = new MemoryStream(byteArray.Data))
        {
          var secondArg = parameters.ElementAtOrDefault(1);

          if (secondArg != null && TypeUtilities.IsString(secondArg))
          {
            var passParam = TypeConverter.ToString(parameters[1]);
            documentInstance = new SPExcelDocumentInstance(this.InstancePrototype, ms, passParam);
          }
          else
          {
            documentInstance = new SPExcelDocumentInstance(this.InstancePrototype, ms);
          }
        }
      }
      else if (TypeUtilities.IsString(firstArg))
      {
        SPFile fileFromUrl;
        var fileUrl = firstArg as string;
        if (SPHelper.TryGetSPFile(fileUrl, out fileFromUrl) == false)
          throw new JavaScriptException(this.Engine, "Error", "A file with the specified url does not exist.");

        documentInstance = new SPExcelDocumentInstance(this.InstancePrototype, fileFromUrl);
      }
      else if (firstArg is SPFileInstance)
      {
        var fileFromArg = (firstArg as SPFileInstance).File;
        documentInstance = new SPExcelDocumentInstance(this.InstancePrototype, fileFromArg);
      }
      else
      {
        throw new JavaScriptException(this.Engine, "Error",
                                      "An Excel Document cannot be constructed with the specified argument.");
      }

      return documentInstance;
    }
  }

  [Serializable]
  public class SPExcelDocumentInstance : ExcelDocumentInstance
  {
    public SPExcelDocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    public SPExcelDocumentInstance(ObjectInstance prototype, Stream stream)
      : base(prototype, stream)
    {
    }

    public SPExcelDocumentInstance(ObjectInstance prototype, Stream stream, string password)
      : base(prototype, stream, password)
    {
    }

    public SPExcelDocumentInstance(ObjectInstance prototype, SPFile file)
      : base(prototype)
    {
      var ms = new MemoryStream(file.OpenBinary());
      m_excelPackage.Load(ms);
    }

    [JSFunction(Name = "load")]
    public override void Load(params object[] args)
    {
      var firstArg = args.FirstOrDefault();

      SPFile file = null;

      if (firstArg is string)
      {
        var fileUrl = firstArg as string;
        if (SPHelper.TryGetSPFile(fileUrl, out file) == false)
          throw new JavaScriptException(this.Engine, "Error", "A file with the specified url does not exist.");
      }
      else if (firstArg is SPFileInstance)
      {
        file = (firstArg as SPFileInstance).File;
      }

      if (file != null)
      {
        var fileStream = new MemoryStream(file.OpenBinary());
        m_excelPackage.Load(fileStream);

        return;
      }

      base.Load(args);
    }
  }
}
