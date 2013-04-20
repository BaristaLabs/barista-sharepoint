namespace Barista.SharePoint.Features.BaristaUnitTests
{
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Runtime.InteropServices;
  using System.Xml;

  /// <summary>
  /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
  /// </summary>
  /// <remarks>
  /// The GUID attached to this class may be used during packaging and should not be modified.
  /// </remarks>

  [Guid("03bbb69f-c338-4af9-9df2-5012bf868a79")]
  public class BaristaUnitTestsEventReceiver : SPFeatureReceiver
  {
    public override void FeatureActivated(SPFeatureReceiverProperties properties)
    {
      //Ensure that the Barista farm feature has been activated.
      var isBaristaServiceFeatureActivated =
        SPWebService.ContentService.Features[new Guid("90fb8db4-2b5f-4de7-882b-6faba092942c")] != null;

      if (isBaristaServiceFeatureActivated == false)
        throw new SPException(
          "The Farm-Level Barista Service Feature (90fb8db4-2b5f-4de7-882b-6faba092942c) must first be activated.");

      base.FeatureActivated(properties);
    }
    public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
    {
      SPWeb web = properties.Feature.Parent as SPWeb;

      List<String> lModuleFiles = new List<String>();
      List<String> lListInstances = new List<String>();
      SPElementDefinitionCollection spFeatureElements = properties.Definition.GetElementDefinitions(CultureInfo.CurrentCulture);

      foreach (SPElementDefinition spElementDefinition in spFeatureElements)
      {
        if (spElementDefinition.ElementType == "Module")
        {
          XmlElement xmlElementNode = (XmlElement)spElementDefinition.XmlDefinition;
          String sModName = xmlElementNode.GetAttribute("Name");
          String sModUrl = SPUtility.ConcatUrls(web.Site.Url, xmlElementNode.GetAttribute("Url"));

          foreach (XmlElement xmlChildElementNode in xmlElementNode.ChildNodes)
          {
            if (xmlChildElementNode.Name == "File")
            {
              String sFile = SPUtility.ConcatUrls(sModUrl, xmlChildElementNode.GetAttribute("Url"));
              lModuleFiles.Add(sFile);
            }
          }
        }
        else if (spElementDefinition.ElementType == "ListInstance")
        {
          XmlElement xmlElementNode = (XmlElement)spElementDefinition.XmlDefinition;
          String sModTitle = xmlElementNode.GetAttribute("Title");
          String sModUrl = xmlElementNode.GetAttribute("Url");
          lListInstances.Add(sModTitle);
        }
      }
      #region Delete list instances added by this feature
      try
      {
        if (lListInstances.Count > 0)
        {
          foreach (String listName in lListInstances)
          {
            try
            {
              SPList workList = web.Lists.TryGetList(listName);
              if (workList != null)
              {
                Guid gd = workList.ID;
                web.Lists.Delete(gd);
              }
            }
            catch { }
          }
        }
      }
      catch { }
      try
      {
        if (lModuleFiles.Count > 0)
        {
          foreach (String fileUrl in lModuleFiles)
          {
            try
            {
              SPFile file = web.GetFile(fileUrl);
              if (file != null)
              {
                SPFolder folder = file.ParentFolder;
                file.Delete();
                web.Update();

                //attempt to delete the folder if it is now empty
                if (folder.Files.Count < 1)
                  folder.Delete();
              }
            }
            catch { }
          }
        }
      }
      catch { }
      #endregion
    }
  }
}
