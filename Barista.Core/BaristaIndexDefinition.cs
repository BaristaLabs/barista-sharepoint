namespace Barista
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceNamespace)]
    [Serializable]
    public class BaristaIndexDefinition : IExtensibleDataObject, IEquatable<BaristaIndexDefinition>
    {
        public BaristaIndexDefinition()
        {
            ExtendedProperties = new Dictionary<string, string>();
        }

        [DataMember]
        public string IndexName
        {
            get;
            set;
        }

        [DataMember]
        public string Description
        {
            get;
            set;
        }

        [DataMember]
        public string TypeName
        {
            get;
            set;
        }

        [DataMember]
        public string IndexStoragePath
        {
            get;
            set;
        }

        [DataMember]
        public IDictionary<string, string> ExtendedProperties
        {
            get;
            set;
        }

        [NonSerialized]
        private ExtensionDataObject m_extensionData;

        public ExtensionDataObject ExtensionData
        {
            get { return m_extensionData; }
            set { m_extensionData = value; }
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as BaristaIndexDefinition;
            return other != null && Equals(other);
        }

        public bool Equals(BaristaIndexDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(TypeName, other.TypeName, StringComparison.OrdinalIgnoreCase) && string.Equals(IndexStoragePath, other.IndexStoragePath, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TypeName.GetHashCode() * 397) ^ IndexStoragePath.GetHashCode();
            }
        }

        public static bool operator ==(BaristaIndexDefinition left, BaristaIndexDefinition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaristaIndexDefinition left, BaristaIndexDefinition right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
