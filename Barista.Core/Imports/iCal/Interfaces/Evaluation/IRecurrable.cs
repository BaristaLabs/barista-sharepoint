namespace Barista.DDay.iCal
{
    using System.Collections.Generic;

    public interface IRecurrable :
        IGetOccurrences,
        IServiceProvider
    {
        /// <summary>
        /// Gets/sets the start date/time of the component.
        /// </summary>
        IDateTime Start { get; set; }

        IList<IPeriodList> ExceptionDates { get; set; }
        IList<IRecurrencePattern> ExceptionRules { get; set; }
        IList<IPeriodList> RecurrenceDates { get; set; }
        IList<IRecurrencePattern> RecurrenceRules { get; set; }
        IDateTime RecurrenceId { get; set; }        
    }
}
