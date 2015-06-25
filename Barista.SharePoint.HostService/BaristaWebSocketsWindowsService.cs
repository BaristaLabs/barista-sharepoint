using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Barista.SharePoint.HostService
{
  using System.ServiceModel;
  using Barista.Services;

  partial class BaristaWebSocketsWindowsService : ServiceBase
  {
    private ServiceHost m_serviceHost;

    public BaristaWebSocketsWindowsService()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      if (m_serviceHost != null)
      {
        m_serviceHost.Close();
      }

      // Create a ServiceHost for the CalculatorService type and 
      // provide the base address.
      m_serviceHost = new ServiceHost(typeof(BaristaWebSocketsService));

      // Open the ServiceHostBase to create listeners and start 
      // listening for messages.
      m_serviceHost.Open();
    }

    protected override void OnStop()
    {
      if (m_serviceHost != null)
      {
        m_serviceHost.Close();
        m_serviceHost = null;
      }
    }
  }
}
