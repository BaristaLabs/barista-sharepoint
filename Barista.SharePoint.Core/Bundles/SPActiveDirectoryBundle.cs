﻿namespace Barista.SharePoint.Bundles
{
  using Barista.Bundles;
  using Barista.Library;
  using System;

  /// <summary>
  /// Installs the SharePoint-specific implementation of the Active Directory bundle.
  /// </summary>
  [Serializable]
  public class SPActiveDirectoryBundle : ActiveDirectoryBundle
  {
    public override object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var adInstance = (ActiveDirectoryInstance)base.InstallBundle(engine);

      //Set the current login name based on the current user associated with the context.
      adInstance.CurrentUserLoginNameFactory = () => SPBaristaContext.Current.Web.CurrentUser == null
          ? String.Empty
          : SPBaristaContext.Current.Web.CurrentUser.LoginName;

      return adInstance;
    }
  }
}
