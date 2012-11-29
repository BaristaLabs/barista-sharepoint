namespace Barista.Stateless
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public partial class StateMachine<TState, TTrigger>
  {
    internal class StateRepresentation
    {
      private readonly TState m_state;

      private readonly IDictionary<TTrigger, ICollection<TriggerBehaviour>> m_triggerBehaviours =
          new Dictionary<TTrigger, ICollection<TriggerBehaviour>>();

      private readonly ICollection<Action<Transition, object[]>> m_entryActions = new List<Action<Transition, object[]>>();
      private readonly ICollection<Action<Transition>> m_exitActions = new List<Action<Transition>>();

      private StateRepresentation m_superstate; // null

      private readonly ICollection<StateRepresentation> m_substates = new List<StateRepresentation>();

      public StateRepresentation(TState state)
      {
        m_state = state;
      }

      public bool CanHandle(TTrigger trigger)
      {
        TriggerBehaviour unused;
        return TryFindHandler(trigger, out unused);
      }

      public bool TryFindHandler(TTrigger trigger, out TriggerBehaviour handler)
      {
        return (TryFindLocalHandler(trigger, out handler) ||
            (Superstate != null && Superstate.TryFindHandler(trigger, out handler)));
      }

      private bool TryFindLocalHandler(TTrigger trigger, out TriggerBehaviour handler)
      {
        ICollection<TriggerBehaviour> possible;
        if (!m_triggerBehaviours.TryGetValue(trigger, out possible))
        {
          handler = null;
          return false;
        }

        var actual = possible.Where(at => at.IsGuardConditionMet).ToArray();

        if (actual.Count() > 1)
          throw new InvalidOperationException(
              string.Format("Multiple permitted exit transitions are configured from state '{1}' for trigger '{0}'. Guard clauses must be mutually exclusive.",
              trigger, m_state));

        handler = actual.FirstOrDefault();
        return handler != null;
      }

      public void AddEntryAction(TTrigger trigger, Action<Transition, object[]> action)
      {
        Enforce.ArgumentNotNull(action, "action");
        m_entryActions.Add((t, args) =>
        {
          if (t.Trigger.Equals(trigger))
            action(t, args);
        });
      }

      public void AddEntryAction(Action<Transition, object[]> action)
      {
        m_entryActions.Add(Enforce.ArgumentNotNull(action, "action"));
      }

      public void AddExitAction(Action<Transition> action)
      {
        m_exitActions.Add(Enforce.ArgumentNotNull(action, "action"));
      }

      public void Enter(Transition transition, params object[] entryArgs)
      {
        Enforce.ArgumentNotNull(transition, "transtion");

        if (transition.IsReentry)
        {
          ExecuteEntryActions(transition, entryArgs);
        }
        else if (!Includes(transition.Source))
        {
          if (m_superstate != null)
            m_superstate.Enter(transition, entryArgs);

          ExecuteEntryActions(transition, entryArgs);
        }
      }

      public void Exit(Transition transition)
      {
        Enforce.ArgumentNotNull(transition, "transtion");

        if (transition.IsReentry)
        {
          ExecuteExitActions(transition);
        }
        else if (!Includes(transition.Destination))
        {
          ExecuteExitActions(transition);
          if (m_superstate != null)
            m_superstate.Exit(transition);
        }
      }

      private void ExecuteEntryActions(Transition transition, object[] entryArgs)
      {
        Enforce.ArgumentNotNull(transition, "transtion");
        Enforce.ArgumentNotNull(entryArgs, "entryArgs");
        foreach (var action in m_entryActions)
          action(transition, entryArgs);
      }

      private void ExecuteExitActions(Transition transition)
      {
        Enforce.ArgumentNotNull(transition, "transtion");
        foreach (var action in m_exitActions)
          action(transition);
      }

      public void AddTriggerBehaviour(TriggerBehaviour triggerBehaviour)
      {
        ICollection<TriggerBehaviour> allowed;
        if (!m_triggerBehaviours.TryGetValue(triggerBehaviour.Trigger, out allowed))
        {
          allowed = new List<TriggerBehaviour>();
          m_triggerBehaviours.Add(triggerBehaviour.Trigger, allowed);
        }
        allowed.Add(triggerBehaviour);
      }

      public StateRepresentation Superstate
      {
        get
        {
          return m_superstate;
        }
        set
        {
          m_superstate = value;
        }
      }

      public TState UnderlyingState
      {
        get
        {
          return m_state;
        }
      }

      public void AddSubstate(StateRepresentation substate)
      {
        Enforce.ArgumentNotNull(substate, "substate");
        m_substates.Add(substate);
      }

      public bool Includes(TState state)
      {
        return m_state.Equals(state) || m_substates.Any(s => s.Includes(state));
      }

      public bool IsIncludedIn(TState state)
      {
        return
            m_state.Equals(state) ||
            (m_superstate != null && m_superstate.IsIncludedIn(state));
      }

      public IEnumerable<TTrigger> PermittedTriggers
      {
        get
        {
          var result = m_triggerBehaviours
              .Where(t => t.Value.Any(a => a.IsGuardConditionMet))
              .Select(t => t.Key);

          if (Superstate != null)
            result = result.Union(Superstate.PermittedTriggers);

          return result.ToArray();
        }
      }
    }
  }
}
