namespace Barista.SharePoint.DocumentStore
{
    using System;

    internal static class Constants
    {
        public const string MetadataPrefix = "ods_";
        public const string DocumentSetEntityPartExtension = ".dsep";

        public static Guid DocumentContainerFeatureId = new Guid("1e084611-a8c5-449c-a1f0-841a56ee2712");

        #region Fields
        public static Guid NamespaceFieldId = new Guid("5b8dd38e-b926-41b3-b7f1-896534b9d916");
        public static Guid DocumentEntityGuidFieldId = new Guid("efab5a5d-493b-480f-867a-2f754a269200");
        public static Guid DocumentEntityIndexedPropertyFieldId = new Guid("559f471b-e1bb-49b5-b6a9-5816c63c9cf1");
        public static Guid CategoryFieldId = new Guid("36b8af8b-a773-478c-abf3-038d198e90b2");
        public static Guid CommentFieldId = new Guid("cfbc77e4-1fe5-4129-9b97-60021f9dd92f");
        public static Guid DocumentEntityContentsHashFieldId = new Guid("fde9fe27-2621-4441-84dc-02f842aa042e");
        public static Guid DocumentEntityContentsContentsLastModifiedFieldId = new Guid("db7f92c5-50ff-4e28-a0b7-0cc43da4270f");

        public static Guid AttachmentPathFieldId = new Guid("bba7d51c-914a-4b45-bc13-daf11c6f7aea");
        #endregion
        public static string DocumentStoreEntityContentTypeId = "0x0120D520009156b84f06ea4f2f96e34101e1e1b1a8";
        public static string DocumentStoreEntityPartContentTypeId = "0x010100c85948cf1fa64eed9d29eaa2a1078fd6";
        public static string DocumentStoreEntityAttachmentContentTypeId = "0x010100d1f6bb0d78094ba3acabd9118f81b0c5";

        public static string DocumentStoreDefaultEntityPartFileName = "default.dsep";
        public static string DocumentStoreEntityContentsPartFileName = "__contents.dsep";

        public const string DSServiceV1Namespace = "http://barista/sharepoint/documentstore/v1/";
        public const string ServiceNamespace = "http://barista/sharepoint/documentstore/2012/02/";
    }
}
