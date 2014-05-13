namespace Barista.SharePoint.DocumentStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Barista.DocumentStore;
    using Microsoft.SharePoint;

    public partial class SPDocumentStore
    {
        #region Containers

        /// <summary>
        /// Creates a container in the document store with the specified title and description.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public virtual Container CreateContainer(string containerTitle, string description)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            web.AllowUnsafeUpdates = true;
            try
            {
                var listId = web.Lists.Add(containerTitle,                         //List Title
                                            description,                            //List Description
                                            "Lists/" + containerTitle,              //List Url
                                            "1e084611-a8c5-449c-a1f0-841a56ee2712", //Feature Id of List definition Provisioning Feature – CustomList Feature Id
                                            10001,                                  //List Template Type
                                            "121");                                 //Document Template Type .. 101 is for None
                return SPDocumentStoreHelper.MapContainerFromSPList(web.Lists[listId]);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Gets the container with the specified title from the document store.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual Container GetContainer(string containerTitle)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            return SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false
                ? null
                : SPDocumentStoreHelper.MapContainerFromSPList(list);
        }

        /// <summary>
        /// Updates the container in the document store with new values.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        public virtual bool UpdateContainer(Container container)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, container.Title, out list, out folder, String.Empty) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                list.Title = container.Title;
                list.Description = container.Description;
                list.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
            return true;
        }

        /// <summary>
        /// Deletes the container with the specified title from the document store.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        public virtual void DeleteContainer(string containerTitle)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return;

            web.AllowUnsafeUpdates = true;
            try
            {
                list.Recycle();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        /// <summary>
        /// Lists all containers contained in the document store.
        /// </summary>
        /// <returns></returns>
        public virtual IList<Container> ListContainers()
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            return SPDocumentStoreHelper.GetContainers(web)
                                        .Select(w => SPDocumentStoreHelper.MapContainerFromSPList(w))
                                        .ToList();
        }
        #endregion
    }
}
