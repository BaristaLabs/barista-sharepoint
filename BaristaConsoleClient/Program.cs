namespace BaristaConsoleClient
{
  using System.IO;
  using Barista;
  using Barista.Bundles;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Barista.Raven.Bundles;
  using BaristaConsoleClient.Bundles;
  using System.Linq;
  using System;

  class Program
  {
    static void Main(string[] args)
    {
      //Validate parameters.
      if (args.Length != 1)
      {
        PrintParameters();
        return;
      }

      var scriptPath = args.ElementAtOrDefault(0);

      if (String.IsNullOrWhiteSpace(scriptPath))
      {
        PrintParameters();
        return;
      }

      //Get the script engine
      var engine = GetScriptEngine();

      string source;
      if (TryGetScriptSource(scriptPath, out source) == false)
      {
        Console.WriteLine(@"Could not obtain the source for the specified script.");
        Console.WriteLine();
        PrintParameters();
        return;
      }

      //Execute the script -- catch any exception in order to pretty print the exception.
      try
      {
        var result = engine.Evaluate(source);

        //TODO: Make this better, e.g. pretty print the result.
        Console.WriteLine(result.ToString());
      }
      catch (JavaScriptException ex)
      {
        //TODO: Log the exception.
        PrettyPrintJavaScriptException(ex);
      }
      catch (Exception ex)
      {
        //TODO: Log the exception.
        PrettyPrintException(ex);
      }
      finally
      {
        //Cleanup
        // ReSharper disable RedundantAssignment
        engine = null;
        // ReSharper restore RedundantAssignment
      }
    }

    public static void PrintParameters()
    {
      Console.WriteLine(@"Usage:");
      Console.WriteLine(@"BaristaConsoleClient.exe <script>");
    }

    public static void PrettyPrintJavaScriptException(JavaScriptException ex)
    {
    }

    public static void PrettyPrintException(Exception ex)
    {
    }

    /// <summary>
    /// Obtains the script source -- either locally or from the script repro.
    /// </summary>
    /// <param name="scriptPath"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static bool TryGetScriptSource(string scriptPath, out string source)
    {
      //Attempt to get the script source from a local file.
      try
      {
        var path = Path.GetFullPath(scriptPath);
        if (File.Exists(path))
        {
          source = File.ReadAllText(scriptPath);
          return true;
        }
      }
      catch (ArgumentException)
      {
        /* Do Nothing */
      }


      //TODO: Obtain the script by name from the script repro.
      

      source = null;
      return false;
    }

    public static ScriptEngine GetScriptEngine()
    {
      //Initialize the script engine.
      var engine = new ScriptEngine();

      //Register Bundles.
      var instance = new BaristaGlobal(engine);

      instance.Common.RegisterBundle(new StringBundle());
      instance.Common.RegisterBundle(new SugarBundle());
      instance.Common.RegisterBundle(new SucraloseBundle());
      instance.Common.RegisterBundle(new MomentBundle());
      instance.Common.RegisterBundle(new MustacheBundle());
      instance.Common.RegisterBundle(new LinqBundle());
      instance.Common.RegisterBundle(new JsonDataBundle());
      instance.Common.RegisterBundle(new ActiveDirectoryBundle());
      instance.Common.RegisterBundle(new DocumentBundle());
      instance.Common.RegisterBundle(new UtilityBundle());
      instance.Common.RegisterBundle(new DocumentStoreBundle());
      instance.Common.RegisterBundle(new RavenClientBundle());
      instance.Common.RegisterBundle(new SimpleInheritanceBundle());
      instance.Common.RegisterBundle(new SqlDataBundle());
      instance.Common.RegisterBundle(new StateMachineBundle());
      instance.Common.RegisterBundle(new DeferredBundle());
      instance.Common.RegisterBundle(new TfsBundle());
      instance.Common.RegisterBundle(new BaristaSearchIndexBundle());
      instance.Common.RegisterBundle(new WebAdministrationBundle());
      instance.Common.RegisterBundle(new UnitTestingBundle());

      //Global Types
      engine.SetGlobalValue("Barista", instance);

      engine.SetGlobalValue("fs", new FileSystemInstance(engine));

      engine.SetGlobalValue("Guid", new GuidConstructor(engine));
      engine.SetGlobalValue("Uri", new UriConstructor(engine));
      engine.SetGlobalValue("Point", new PointConstructor(engine));
      engine.SetGlobalValue("Size", new SizeConstructor(engine));
      engine.SetGlobalValue("NetworkCredential", new NetworkCredentialConstructor(engine));
      engine.SetGlobalValue("Base64EncodedByteArray", new Base64EncodedByteArrayConstructor(engine));

      var console = new FirebugConsole(engine)
      {
        Output = new BaristaConsoleOutput(engine)
      };

      engine.SetGlobalValue("console", console);
      return engine;
    }
  }
}
