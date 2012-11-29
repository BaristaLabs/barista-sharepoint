namespace Barista.Stateless
{
  using System;

  public partial class StateMachine<TState, TTrigger>
  {
    internal class TransitioningTriggerBehaviour : TriggerBehaviour
    {
      private readonly TState m_destination;

      public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard)
        : base(trigger, guard)
      {
        m_destination = destination;
      }

      public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
      {
        destination = m_destination;
        return true;
      }
    }
  }
}
