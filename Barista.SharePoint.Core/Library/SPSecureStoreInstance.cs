namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using System;
  using System.Net;

  [Serializable]
  public class SPSecureStoreConstructor : ClrFunction
  {
    public SPSecureStoreConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPSecureStore", new SPSecureStoreInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPSecureStoreInstance Construct()
    {
      return new SPSecureStoreInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPSecureStoreInstance : ObjectInstance
  {
    public SPSecureStoreInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "callAsUser")]
    [JSDoc("Calls the specified function using the credentials stored under the specified application id.")]
    public object CallAsUser(string applicationId, FunctionInstance function, object thisObj, params object[] arguments)
    {
      if (thisObj == null || thisObj == Null.Value || thisObj == Undefined.Value)
        thisObj = this.Engine.Global;

      object result = null;
      ImpersonationHelper.InvokeAsUser(applicationId, new Action(() =>
        {
          result = function.Call(thisObj, arguments);
        }));

      return result;
    }

    [JSDoc("Gets a credential object that represents the credentials stored with the specified application id.")]
    [JSFunction(Name = "getCredential")]
    public object GetCredential(string applicationId)
    {
      var result = ImpersonationHelper.GetCredentialsFromSecureStore(applicationId);

      var networkCredential = result as NetworkCredential;
      if (networkCredential == null)
        return Null.Value;

      var cred = new NetworkCredentialInstance(this.Engine.Object.InstancePrototype, result as NetworkCredential)
      {
        MaskPassword = true
      };
      return cred;
    }

    [JSFunction(Name = "execAsUser")]
    [JSDoc("Executes the specified script using the credentials stored under the specified application id.")]
    public void ExecAsUser(string applicationId, string script)
    {
      ImpersonationHelper.InvokeAsUser(applicationId, new Action(() => this.Engine.Execute(script)));
    }

    [JSFunction(Name = "evalAsUser")]
    [JSDoc("Evaluates the specified script using the credentials stored under the specified application id.")]
    public object EvalAsUser(string applicationId, string script)
    {
      object result = null;
      ImpersonationHelper.InvokeAsUser(applicationId, new Action(() =>
        {
          result = this.Engine.Evaluate(script);
        }));
      return result;
    }
  }
}
