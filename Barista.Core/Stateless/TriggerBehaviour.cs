namespace Barista.Stateless
{
  using System;

  public partial class StateMachine<TState, TTrigger>
  {
    internal abstract class TriggerBehaviour
    {
      private readonly TTrigger m_trigger;
      private readonly Func<bool> m_guard;

      protected TriggerBehaviour(TTrigger trigger, Func<bool> guard)
      {
        m_trigger = trigger;
        m_guard = guard;
      }

      public TTrigger Trigger { get { return m_trigger; } }

      public bool IsGuardConditionMet
      {
        get
        {
          return m_guard();
        }
      }

      public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
    }
  }
}
