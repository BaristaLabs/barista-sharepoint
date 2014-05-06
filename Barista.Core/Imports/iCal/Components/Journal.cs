using System;
using System.Runtime.Serialization;

namespace Barista.DDay.iCal
{
    /// <summary>
    /// A class that represents an RFC 5545 VJOURNAL component.
    /// </summary>
    [Serializable]
    public class Journal : 
        RecurringComponent,
        IJournal
    {
        #region IJournal Members
        
        public JournalStatus Status
        {
            get { return Properties.Get<JournalStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        } 

        #endregion

        #region Constructors

        public Journal()
        {            
        }

        void Initialize()
        {
            this.Name = Components.JOURNAL;
        }

        #endregion

        #region Overrides

        protected override bool EvaluationIncludesReferenceDate
        {
            get
            {
                return true;
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion
    }
}
