namespace Barista.Stateless
{
  using System;

  public partial class StateMachine<TState, TTrigger>
  {
    internal class DynamicTriggerBehaviour : TriggerBehaviour
    {
      private readonly Func<object[], TState> m_destination;

      public DynamicTriggerBehaviour(TTrigger trigger, Func<object[], TState> destination, Func<bool> guard)
        : base(trigger, guard)
      {
        m_destination = Enforce.ArgumentNotNull(destination, "destination");
      }

      public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
      {
        destination = m_destination(args);
        return true;
      }
    }
  }
}
