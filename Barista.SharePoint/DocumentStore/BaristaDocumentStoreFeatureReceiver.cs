namespace Barista.SharePoint.Features.BaristaDocumentStore
{
    using global::Barista.SharePoint.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;

    public partial class BaristaDocumentStoreFeatureReceiver
    {
        #region Site Column Field Defs Xml
        private const string NamespaceFieldDefXml =
@"<Field ID=""{5b8dd38e-b926-41b3-b7f1-896534b9d916}""
    Group=""Document Store Site Columns""
    Name=""Namespace""
    DisplayName=""Namespace""
    Type=""Note"">
</Field>";

        private const string DocumentEntityGuidFieldDefXml =
@"<Field ID=""{efab5a5d-493b-480f-867a-2f754a269200}""
    Group=""Document Store Site Columns""
    Name=""DocumentEntityGuid""
    DisplayName=""Document Entity Guid""
    Type=""Text""
    MaxLength=""36"">
</Field>";

        private const string DocumentEntityIndexedPropertyFieldDefXml =
@"<Field ID=""{559f471b-e1bb-49b5-b6a9-5816c63c9cf1}""
    Group=""Document Store Site Columns""
    Name=""DocumentEntityIndexedProperty""
    DisplayName=""Document Entity Indexed Property""
    Type=""Note"">
</Field>";

        private const string CommentFieldDefXml =
@"<Field ID=""{cfbc77e4-1fe5-4129-9b97-60021f9dd92f}""
    Group=""Document Store Site Columns""
    Name=""Comments""
    DisplayName=""Comments""
    Type=""Note""
    AppendOnly=""true"">
</Field>";

        private const string CategoryFieldDefXml =
@"<Field ID=""{36b8af8b-a773-478c-abf3-038d198e90b2}""
    Group=""Document Store Site Columns""
    Name=""Category""
    DisplayName=""Category""
    Type=""Text"">
</Field>";

        private const string DocumentEntityContentsHashFieldDefXml =
 @"<Field ID=""{fde9fe27-2621-4441-84dc-02f842aa042e}""
    Group=""Document Store Site Columns""
    Name=""DocumentEntityContentsHash""
    DisplayName=""Document Entity Contents Hash""
    Type=""Text""
    MaxLength=""255"">
</Field>";

        private const string DocumentEntityContentsLastModifiedFieldDefXml =
@"<Field ID=""{db7f92c5-50ff-4e28-a0b7-0cc43da4270f}""
    Group=""Document Store Site Columns""
    Name=""DocumentEntityContentsLastModified""
    DisplayName=""Document Entity Contents Last Modified""
    Type=""DateTime""
    Format=""DateTime"">
</Field>";

        private const string AttachmentPathFieldDefXml =
@"<Field ID=""{bba7d51c-914a-4b45-bc13-daf11c6f7aea}""
    Group=""Document Store Site Columns""
    Name=""AttachmentPath""
    DisplayName=""AttachmentPath""
    Type=""Text"">
</Field>";
        #endregion

        private void CreateSiteColumns(SPWeb web)
        {
            if (web.AvailableFields.Contains(Constants.NamespaceFieldId) == false)
            {
              web.Fields.AddFieldAsXml(NamespaceFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.DocumentEntityGuidFieldId) == false)
            {
                web.Fields.AddFieldAsXml(DocumentEntityGuidFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.DocumentEntityIndexedPropertyFieldId) == false)
            {
                web.Fields.AddFieldAsXml(DocumentEntityIndexedPropertyFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.CommentFieldId) == false)
            {
                web.Fields.AddFieldAsXml(CommentFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.CategoryFieldId) == false)
            {
                web.Fields.AddFieldAsXml(CategoryFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.DocumentEntityContentsHashFieldId) == false)
            {
                web.Fields.AddFieldAsXml(DocumentEntityContentsHashFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.DocumentEntityContentsContentsLastModifiedFieldId) == false)
            {
                web.Fields.AddFieldAsXml(DocumentEntityContentsLastModifiedFieldDefXml);
            }

            if (web.AvailableFields.Contains(Constants.AttachmentPathFieldId) == false)
            {
                web.Fields.AddFieldAsXml(AttachmentPathFieldDefXml);
            }
        }

        private void CreateContentTypes(SPWeb web)
        {
            //Create the EntityPart ContentType.
            var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
            var documentStoreEntityPartContentType = web.ContentTypes[documentStoreEntityPartContentTypeId];
            if (documentStoreEntityPartContentType == null)
            {
                documentStoreEntityPartContentType = new SPContentType(documentStoreEntityPartContentTypeId, web.ContentTypes, "Document Store Entity Part")
                {
                    Group = "Document Store Content Types",
                    Description = "Represents additional data that is contained within a Document Store Entity."
                };

                //Add field links.
                var categoryField = web.AvailableFields[Constants.CategoryFieldId];
                var categoryFieldLink = new SPFieldLink(categoryField)
                {
                    DisplayName = "Category",
                    ShowInDisplayForm = true,
                    ReadOnly = false
                };

                documentStoreEntityPartContentType.FieldLinks.Add(categoryFieldLink);

                var commentField = web.AvailableFields[Constants.CommentFieldId];
                var commentFieldLink = new SPFieldLink(commentField)
                {
                    DisplayName = "Comments",
                    ShowInDisplayForm = true,
                    ReadOnly = false
                };
                documentStoreEntityPartContentType.FieldLinks.Add(commentFieldLink);

                var documentEntityGuidField = web.AvailableFields[Constants.DocumentEntityGuidFieldId];
                var documentEntityGuidFieldLink = new SPFieldLink(documentEntityGuidField)
                {
                    DisplayName = "DocumentEntityGuid",
                    ShowInDisplayForm = false,
                    ReadOnly = true,
                    Required = false
                };
                documentStoreEntityPartContentType.FieldLinks.Add(documentEntityGuidFieldLink);

                web.ContentTypes.Add(documentStoreEntityPartContentType);
                documentStoreEntityPartContentType.Update(true);
            }

            //Create the Document Store Attachment ContentType.
            var documentStoreAttachmentContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityAttachmentContentTypeId);
            var documentStoreAttachmentContentType = web.ContentTypes[documentStoreAttachmentContentTypeId];
            if (documentStoreAttachmentContentType == null)
            {
                documentStoreAttachmentContentType = new SPContentType(documentStoreAttachmentContentTypeId, web.ContentTypes, "Document Store Attachment")
                {
                    Group = "Document Store Content Types",
                    Description = "Represents a document that is contained within a Document Store Entity."
                };

                //Add field links.
                var categoryField = web.AvailableFields[Constants.CategoryFieldId];
                var categoryFieldLink = new SPFieldLink(categoryField)
                {
                    DisplayName = "Category",
                    ShowInDisplayForm = true,
                    ReadOnly = false
                };
                documentStoreAttachmentContentType.FieldLinks.Add(categoryFieldLink);

                var commentField = web.AvailableFields[Constants.CommentFieldId];
                var commentFieldLink = new SPFieldLink(commentField)
                {
                    DisplayName = "Comments",
                    ShowInDisplayForm = true,
                    ReadOnly = false
                };
                documentStoreAttachmentContentType.FieldLinks.Add(commentFieldLink);

                var pathField = web.AvailableFields[Constants.AttachmentPathFieldId];
                var pathFieldLink = new SPFieldLink(pathField)
                {
                    DisplayName = "Path",
                    ShowInDisplayForm = true,
                    ReadOnly = false
                };
                documentStoreAttachmentContentType.FieldLinks.Add(pathFieldLink);

                var documentEntityGuidField = web.AvailableFields[Constants.DocumentEntityGuidFieldId];
                var documentEntityGuidFieldLink = new SPFieldLink(documentEntityGuidField)
                {
                    DisplayName = "DocumentEntityGuid",
                    ShowInDisplayForm = false,
                    ReadOnly = true,
                    Required = false
                };
                documentStoreAttachmentContentType.FieldLinks.Add(documentEntityGuidFieldLink);

                web.ContentTypes.Add(documentStoreAttachmentContentType);
                documentStoreAttachmentContentType.Update(true);
            }

            //Finally, create the DocumentStoreEntity ContentType which subclasses Document Set.
            var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
            var documentStoreEntityContentType = web.ContentTypes[documentStoreEntityContentTypeId];
            if (documentStoreEntityContentType == null)
            {
                documentStoreEntityContentType = new SPContentType(documentStoreEntityContentTypeId,
                  web.ContentTypes, "Document Store Entity")
                {
                    Group = "Document Store Content Types",
                    Description =
                        "Represents a document, attachments and associated metadata in a NoSql/Document-oriented database style model."
                };

                //Add field links.
                var namespaceField = web.AvailableFields[Constants.NamespaceFieldId];
                var namespaceFieldLink = new SPFieldLink(namespaceField);
                documentStoreEntityContentType.FieldLinks.Add(namespaceFieldLink);

                //TODO: Finish this...
                web.ContentTypes.Add(documentStoreEntityContentType);

                var documentStoreEntityTemplate = DocumentSetTemplate.GetDocumentSetTemplate(documentStoreEntityContentType);
                documentStoreEntityTemplate.AllowedContentTypes.Add(documentStoreEntityPartContentTypeId);
            }
        }
    }
}
