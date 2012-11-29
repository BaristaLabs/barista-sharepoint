namespace Barista.Stateless
{
  public partial class StateMachine<TState, TTrigger>
  {
    /// <summary>
    /// Describes a state transition.
    /// </summary>
    public class Transition
    {
      private readonly TState m_source;
      private readonly TState m_destination;
      private readonly TTrigger m_trigger;

      /// <summary>
      /// Construct a transition.
      /// </summary>
      /// <param name="source">The state transitioned from.</param>
      /// <param name="destination">The state transitioned to.</param>
      /// <param name="trigger">The trigger that caused the transition.</param>
      public Transition(TState source, TState destination, TTrigger trigger)
      {
        m_source = source;
        m_destination = destination;
        m_trigger = trigger;
      }

      /// <summary>
      /// The state transitioned from.
      /// </summary>
      public TState Source { get { return m_source; } }

      /// <summary>
      /// The state transitioned to.
      /// </summary>
      public TState Destination { get { return m_destination; } }

      /// <summary>
      /// The trigger that caused the transition.
      /// </summary>
      public TTrigger Trigger { get { return m_trigger; } }

      /// <summary>
      /// True if the transition is a re-entry, i.e. the identity transition.
      /// </summary>
      public bool IsReentry { get { return Source.Equals(Destination); } }
    }
  }
}
