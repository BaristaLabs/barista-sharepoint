namespace Barista.SharePoint.Search
{
    using System;
    using Barista.Search;
    using Microsoft.SharePoint;

    public class SPBaristaSearchService : BaristaSearchService
    {
        protected override Lucene.Net.Store.Directory GetLuceneDirectoryImplementationFromType(Type directoryType, BaristaIndexDefinition indexDefinition)
        {
            if (directoryType != typeof (Barista.SharePoint.Search.SPDirectory))
                return base.GetLuceneDirectoryImplementationFromType(directoryType, indexDefinition);

            SPSite site = null;
            SPWeb web = null;

            //Test for the existance of the target index.
            try
            {
                SPFolder folder;
                if (SPHelper.TryGetSPFolder(indexDefinition.IndexStoragePath, out site, out web, out folder) == false)
                    throw new InvalidOperationException(
                        string.Format(
                            "An SharePoint index definition named {0} was located, however, the target index location {1} is not valid.",
                            indexDefinition.IndexName, indexDefinition.IndexStoragePath));
            }
            finally
            {
                if (web != null)
                    web.Dispose();

                if (site != null)
                    site.Dispose();
            }

            return new SPDirectory(indexDefinition.IndexStoragePath);
        }
    }
}
