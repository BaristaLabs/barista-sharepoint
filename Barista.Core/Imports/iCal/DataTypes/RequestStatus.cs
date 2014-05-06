namespace Barista.DDay.iCal
{
    using Barista.DDay.iCal.Serialization.iCalendar;
    using System;
    using System.IO;

    /// <summary>
    /// A class that represents the return status of an iCalendar request.
    /// </summary>
    [Serializable]
    public class RequestStatus :
        EncodableDataType,
        IRequestStatus
    {
        protected bool Equals(RequestStatus other)
        {
            return string.Equals(m_Description, other.m_Description) && string.Equals(m_ExtraData, other.m_ExtraData) && Equals(m_StatusCode, other.m_StatusCode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (m_Description != null ? m_Description.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (m_ExtraData != null ? m_ExtraData.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (m_StatusCode != null ? m_StatusCode.GetHashCode() : 0);
                return hashCode;
            }
        }

        #region Private Fields

        private string m_Description;
        private string m_ExtraData;
        private IStatusCode m_StatusCode;

        #endregion

        #region Public Properties

        virtual public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        virtual public string ExtraData
        {
            get { return m_ExtraData; }
            set { m_ExtraData = value; }
        }

        virtual public IStatusCode StatusCode
        {
            get { return m_StatusCode; }
            set { m_StatusCode = value; }
        }

        #endregion

        #region Constructors

        public RequestStatus() { }
        public RequestStatus(string value)
            : this()
        {
            RequestStatusSerializer serializer = new RequestStatusSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IRequestStatus)
            {
                IRequestStatus rs = (IRequestStatus)obj;                
                if (rs.StatusCode != null)
                    StatusCode = rs.StatusCode.Copy<IStatusCode>();
                Description = rs.Description;
                rs.ExtraData = rs.ExtraData;
            }
        }

        public override string ToString()
        {
            RequestStatusSerializer serializer = new RequestStatusSerializer();
            return serializer.SerializeToString(this);
        }

        public override bool Equals(object obj)
        {
            IRequestStatus rs = obj as IRequestStatus;
            if (rs != null)
            {
                return object.Equals(StatusCode, rs.StatusCode);
            }

            return base.Equals(obj);
        }

        #endregion
    }
}
