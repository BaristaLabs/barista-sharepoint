using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Administration;

    [Serializable]
    public class SPQuotaConstructor : ClrFunction
    {
        public SPQuotaConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPQuota", new SPQuotaInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPQuotaInstance Construct()
        {
            return new SPQuotaInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPQuotaInstance : ObjectInstance
    {
        private readonly SPQuota m_quota;

        public SPQuotaInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPQuotaInstance(ObjectInstance prototype, SPQuota quota)
            : this(prototype)
        {
            if (quota == null)
                throw new ArgumentNullException("quota");

            m_quota = quota;
        }

        public SPQuota SPQuota
        {
            get
            {
                return m_quota;
            }
        }

        [JSProperty(Name = "invitedUserMaximumLevel")]
        public int InvitedUserMaximumLevel
        {
            get
            {
                return m_quota.InvitedUserMaximumLevel;
            }
            set
            {
                m_quota.InvitedUserMaximumLevel = value;
            }
        }

        [JSProperty(Name = "quotaId")]
        public int QuotaId
        {
            get
            {
                return m_quota.QuotaID;
            }
            set
            {
                m_quota.QuotaID = (ushort)value;
            }
        }

        [JSProperty(Name = "storageMaximumLevel")]
        public double StorageMaximumLevel
        {
            get
            {
                return m_quota.StorageMaximumLevel;
            }
            set
            {
                m_quota.StorageMaximumLevel = (long)value;
            }
        }

        [JSProperty(Name = "storageWarningLevel")]
        public double StorageWarningLevel
        {
            get
            {
                return m_quota.StorageWarningLevel;
            }
            set
            {
                m_quota.StorageWarningLevel = (long)value;
            }
        }

        [JSProperty(Name = "userCodeMaximumLevel")]
        public double UserCodeMaximumLevel
        {
            get
            {
                return m_quota.UserCodeMaximumLevel;
            }
            set
            {
                m_quota.UserCodeMaximumLevel = value;
            }
        }

        [JSProperty(Name = "userCodeWarningLevel")]
        public double UserCodeWarningLevel
        {
            get
            {
                return m_quota.UserCodeWarningLevel;
            }
            set
            {
                m_quota.UserCodeWarningLevel = value;
            }
        }
    }
}
