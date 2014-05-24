namespace Barista.DocumentStore
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceV1Namespace)]
    public class Comment : DSObject
    {
        [DataMember]
        public int Id
        {
            get;
            set;
        }

        [DataMember]
        public string CommentText
        {
            get;
            set;
        }

        [DataMember]
        public string Category
        {
            get;
            set;
        }

        [DataMember]
        public string Source
        {
            get;
            set;
        }

        [DataMember]
        public ICollection<string> Tags
        {
            get;
            set;
        }
    }
}
