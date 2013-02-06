﻿namespace Barista.SharePoint.HostService
{
  using System.Collections.Generic;
  using System.ServiceModel;
  using Barista.SharePoint.Search;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaSearch
  {
    [OperationContract]
    void DeleteAll(IndexDefinition definition);

    [OperationContract]
    void Commit(IndexDefinition definition);

    [OperationContract]
    void AddDocumentToIndex(IndexDefinition definition, Document document);

    [OperationContract]
    void UpdateDocumentInIndex(IndexDefinition definition, Term term, Document document);

    [OperationContract]
    void DeleteDocuments(IndexDefinition definition, Term term);

    [OperationContract]
    IList<Hit> Search(IndexDefinition definition, string defaultField, string query, int maxResults);
  }
}