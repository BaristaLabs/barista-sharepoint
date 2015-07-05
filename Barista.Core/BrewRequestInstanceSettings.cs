namespace Barista
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = Constants.ServiceNamespace)]
    [Serializable]
    public class BrewRequestInstanceSettings
    {
        public BrewRequestInstanceSettings()
        {
            this.InstanceMode = BaristaInstanceMode.PerCall;
            this.InstanceName = null;

            this.InstanceAbsoluteExpiration = null;
            this.InstanceSlidingExpiration = null;
        }

        [DataMember]
        public BaristaInstanceMode InstanceMode
        {
            get;
            set;
        }

        [DataMember]
        public string InstanceInitializationCode
        {
            get;
            set;
        }

        [DataMember]
        public string InstanceInitializationCodePath
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? InstanceAbsoluteExpiration
        {
            get;
            set;
        }

        [DataMember]
        public TimeSpan? InstanceSlidingExpiration
        {
            get;
            set;
        }

        [DataMember]
        public string InstanceName
        {
            get;
            set;
        }
    }
}
