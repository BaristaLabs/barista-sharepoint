﻿namespace Barista.Stateless
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Models behaviour as transitions between a finite set of states.
  /// </summary>
  /// <typeparam name="TState">The type used to represent the states.</typeparam>
  /// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
  public partial class StateMachine<TState, TTrigger>
  {
    private readonly IDictionary<TState, StateRepresentation> m_stateConfiguration = new Dictionary<TState, StateRepresentation>();
    private readonly IDictionary<TTrigger, TriggerWithParameters> m_triggerConfiguration = new Dictionary<TTrigger, TriggerWithParameters>();
    private readonly Func<TState> m_stateAccessor;
    private readonly Action<TState> m_stateMutator;
    private Action<TState, TTrigger> m_unhandledTriggerAction = DefaultUnhandledTriggerAction;

    /// <summary>
    /// Construct a state machine with external state storage.
    /// </summary>
    /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
    /// <param name="stateMutator">An action that will be called to write new state values.</param>
    public StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator)
    {
      m_stateAccessor = Enforce.ArgumentNotNull(stateAccessor, "stateAccessor");
      m_stateMutator = Enforce.ArgumentNotNull(stateMutator, "stateMutator");
    }

    /// <summary>
    /// Construct a state machine.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    public StateMachine(TState initialState)
    {
      var reference = new StateReference { State = initialState };
      m_stateAccessor = () => reference.State;
      m_stateMutator = s => reference.State = s;
    }

    /// <summary>
    /// The current state.
    /// </summary>
    public TState State
    {
      get
      {
        return m_stateAccessor();
      }
      private set
      {
        m_stateMutator(value);
      }
    }

    /// <summary>
    /// The currently-permissible trigger values.
    /// </summary>
    public IEnumerable<TTrigger> PermittedTriggers
    {
      get
      {
        return CurrentRepresentation.PermittedTriggers;
      }
    }

    private StateRepresentation CurrentRepresentation
    {
      get
      {
        return GetRepresentation(State);
      }
    }

    private StateRepresentation GetRepresentation(TState state)
    {
      StateRepresentation result;

      if (!m_stateConfiguration.TryGetValue(state, out result))
      {
        result = new StateRepresentation(state);
        m_stateConfiguration.Add(state, result);
      }

      return result;
    }

    /// <summary>
    /// Begin configuration of the entry/exit actions and allowed transitions
    /// when the state machine is in a particular state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <returns>A configuration object through which the state can be configured.</returns>
    public StateConfiguration Configure(TState state)
    {
      return new StateConfiguration(GetRepresentation(state), GetRepresentation);
    }

    /// <summary>
    /// Transition from the current state via the specified trigger.
    /// The target state is determined by the configuration of the current state.
    /// Actions associated with leaving the current state and entering the new one
    /// will be invoked.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">The current state does
    /// not allow the trigger to be fired.</exception>
    public void Fire(TTrigger trigger)
    {
      InternalFire(trigger, new object[0]);
    }

    /// <summary>
    /// Transition from the current state via the specified trigger.
    /// The target state is determined by the configuration of the current state.
    /// Actions associated with leaving the current state and entering the new one
    /// will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="arg0">The first argument.</param>
    /// <exception cref="System.InvalidOperationException">The current state does
    /// not allow the trigger to be fired.</exception>
    public void Fire<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
    {
      Enforce.ArgumentNotNull(trigger, "trigger");
      InternalFire(trigger.Trigger, arg0);
    }

    /// <summary>
    /// Transition from the current state via the specified trigger.
    /// The target state is determined by the configuration of the current state.
    /// Actions associated with leaving the current state and entering the new one
    /// will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">The current state does
    /// not allow the trigger to be fired.</exception>
    public void Fire<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
    {
      Enforce.ArgumentNotNull(trigger, "trigger");
      InternalFire(trigger.Trigger, arg0, arg1);
    }

    /// <summary>
    /// Transition from the current state via the specified trigger.
    /// The target state is determined by the configuration of the current state.
    /// Actions associated with leaving the current state and entering the new one
    /// will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="arg2">The third argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">The current state does
    /// not allow the trigger to be fired.</exception>
    public void Fire<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
    {
      Enforce.ArgumentNotNull(trigger, "trigger");
      InternalFire(trigger.Trigger, arg0, arg1, arg2);
    }

    private void InternalFire(TTrigger trigger, params object[] args)
    {
      TriggerWithParameters configuration;
      if (m_triggerConfiguration.TryGetValue(trigger, out configuration))
        configuration.ValidateParameters(args);

      TriggerBehaviour triggerBehaviour;
      if (!CurrentRepresentation.TryFindHandler(trigger, out triggerBehaviour))
      {
        m_unhandledTriggerAction(CurrentRepresentation.UnderlyingState, trigger);
        return;
      }

      var source = State;
      TState destination;
      if (triggerBehaviour.ResultsInTransitionFrom(source, args, out destination))
      {
        var transition = new Transition(source, destination, trigger);

        CurrentRepresentation.Exit(transition);
        State = transition.Destination;
        CurrentRepresentation.Enter(transition, args);
      }
    }

    /// <summary>
    /// Override the default behaviour of throwing an exception when an unhandled trigger
    /// is fired.
    /// </summary>
    /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
    public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
    {
      if (unhandledTriggerAction == null) throw new ArgumentNullException("unhandledTriggerAction");
      m_unhandledTriggerAction = unhandledTriggerAction;
    }

    /// <summary>
    /// Determine if the state machine is in the supplied state.
    /// </summary>
    /// <param name="state">The state to test for.</param>
    /// <returns>True if the current state is equal to, or a substate of,
    /// the supplied state.</returns>
    public bool IsInState(TState state)
    {
      return CurrentRepresentation.IsIncludedIn(state);
    }

    /// <summary>
    /// Returns true if <paramref name="trigger"/> can be fired
    /// in the current state.
    /// </summary>
    /// <param name="trigger">Trigger to test.</param>
    /// <returns>True if the trigger can be fired, false otherwise.</returns>
    public bool CanFire(TTrigger trigger)
    {
      return CurrentRepresentation.CanHandle(trigger);
    }

    /// <summary>
    /// A human-readable representation of the state machine.
    /// </summary>
    /// <returns>A description of the current state and permitted triggers.</returns>
    public override string ToString()
    {
      return string.Format(
          "StateMachine {{ State = {0}, PermittedTriggers = {{ {1} }}}}",
          State,
          string.Join(", ", PermittedTriggers.Select(t => t.ToString()).ToArray()));
    }

    /// <summary>
    /// Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>An object that can be passed to the Fire() method in order to 
    /// fire the parameterised trigger.</returns>
    public TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger)
    {
      var configuration = new TriggerWithParameters<TArg0>(trigger);
      SaveTriggerConfiguration(configuration);
      return configuration;
    }

    /// <summary>
    /// Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>An object that can be passed to the Fire() method in order to 
    /// fire the parameterised trigger.</returns>
    public TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(TTrigger trigger)
    {
      var configuration = new TriggerWithParameters<TArg0, TArg1>(trigger);
      SaveTriggerConfiguration(configuration);
      return configuration;
    }

    /// <summary>
    /// Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>An object that can be passed to the Fire() method in order to 
    /// fire the parameterised trigger.</returns>
    public TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger)
    {
      var configuration = new TriggerWithParameters<TArg0, TArg1, TArg2>(trigger);
      SaveTriggerConfiguration(configuration);
      return configuration;
    }

    private void SaveTriggerConfiguration(TriggerWithParameters trigger)
    {
      if (m_triggerConfiguration.ContainsKey(trigger.Trigger))
        throw new InvalidOperationException(
            string.Format("Parameters for the trigger '{0}' have already been configured.", trigger));

      m_triggerConfiguration.Add(trigger.Trigger, trigger);
    }

    private static void DefaultUnhandledTriggerAction(TState state, TTrigger trigger)
    {
      throw new InvalidOperationException(
          string.Format(
              "No valid leaving transitions are permitted from state '{1}' for trigger '{0}'. Consider ignoring the trigger.",
              trigger, state));
    }
  }
}
