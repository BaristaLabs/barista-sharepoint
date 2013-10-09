namespace Barista.Library
{
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class EnvironmentInstance : ObjectInstance
  {
    public EnvironmentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "commandLine")]
    public string CommandLine
    {
      get
      {
        return Environment.CommandLine;
      }
    }

    [JSProperty(Name = "currentDirectory")]
    public string CurrentDirectory
    {
      get
      {
        return Environment.CurrentDirectory;
      }
    }

    [JSProperty(Name = "exitCode")]
    public int ExitCode
    {
      get
      {
        return Environment.ExitCode;
      }
    }

    [JSProperty(Name = "hasShutdownStarted")]
    public bool HasShutdownStarted
    {
      get
      {
        return Environment.HasShutdownStarted;
      }
    }

    [JSProperty(Name = "machineName")]
    public string MachineName
    {
      get
      {
        return Environment.MachineName;
      }
    }

    [JSProperty(Name = "newLine")]
    public string NewLine
    {
      get
      {
        return Environment.NewLine;
      }
    }

    [JSProperty(Name = "osVersion")]
    public OperatingSystemInstance OSVersion
    {
      get
      {
        return new OperatingSystemInstance(this.Engine.Object.InstancePrototype, Environment.OSVersion);
      }
    }

    [JSProperty(Name = "processorCount")]
    public int ProcessorCount
    {
      get
      {
        return Environment.ProcessorCount;
      }
    }

    [JSProperty(Name = "stackTrace")]
    public string StackTrace
    {
      get
      {
        return Environment.StackTrace;
      }
    }

    [JSProperty(Name = "systemDirectory")]
    public string SystemDirectory
    {
      get
      {
        return Environment.SystemDirectory;
      }
    }

    [JSProperty(Name = "tickCount")]
    public int TickCount
    {
      get
      {
        return Environment.TickCount;
      }
    }

    [JSProperty(Name = "userDomainName")]
    public string UserDomainName
    {
      get
      {
        return Environment.UserDomainName;
      }
    }

    [JSProperty(Name = "userInteractive")]
    public bool UserInteractive
    {
      get
      {
        return Environment.UserInteractive;
      }
    }

    [JSProperty(Name = "userName")]
    public string UserName
    {
      get
      {
        return Environment.UserName;
      }
    }

    [JSProperty(Name = "version")]
    public string Version
    {
      get
      {
        return Environment.Version.ToString();
      }
    }

    [JSProperty(Name = "workingSet")]
    public double WorkingSet
    {
      get
      {
        return Environment.WorkingSet;
      }
    }

    [JSFunction(Name = "expandEvnironmentVariables")]
    public string ExpandEnvironmentVariables(string variable)
    {
      return Environment.ExpandEnvironmentVariables(variable);
    }

    [JSFunction(Name = "getEnvironmentVariable")]
    public string GetEnvironmentVariable(string variable)
    {
      return Environment.GetEnvironmentVariable(variable);
    }
  }
}
