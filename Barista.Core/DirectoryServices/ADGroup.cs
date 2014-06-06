namespace Barista.DirectoryServices
{
    using System;
    using System.Security.Principal;
    using Interop.ActiveDs;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceNamespace)]
    [DirectorySchema("group", typeof(IADsGroup))]
    public class ADGroup : DirectoryEntity
    {
        [DataMember]
        [DirectoryAttribute("objectSid")]
        public object RawsID
        {
            get;
            set;
        }

        [DataMember]
        [DirectoryAttribute("objectSid")]
        public object sID
        {
            get
            {
                SecurityIdentifier s = null;

                try
                {
                    if (RawsID != null)
                        s = new SecurityIdentifier((Byte[])RawsID, 0);
                }
                catch { }

                if (s == null)
                    return String.Empty;

                return s.ToString();
            }
            set { }
        }

        [DataMember]
        public string Name
        {
            get;
            set;
        }

        [DataMember]
        [DirectoryAttribute("displayName")]
        public string DisplayName
        {
            get;
            set;
        }

        [DataMember]
        [DirectoryAttribute("distinguishedName")]
        public string DistinguishedName
        {
            get;
            set;
        }

        [DataMember]
        [DirectoryAttribute("member")]
        public string[] Members
        {
            get;
            set;
        }
    }
}
