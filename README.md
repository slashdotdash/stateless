# [DEPRECATED] Stateless .NET State Machine Framework

Go to the official [Stateless](https://github.com/dotnet-state-machine/stateless) repo for updates.

Stateless is a _hierarchical state machine_ framework based on [Simple State Machine](http://codeplex.com/simplestatemachine) for Boo, but configured using C# 3.0.

Authored by [Nicholas Blumhardt](http://nblumhardt.com/).

* http://code.google.com/p/stateless/
* [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0)

Create state machines and lightweight state machine-based workflows directly in .NET code:

	var phoneCall = new StateMachine<State, Trigger>(State.OffHook);

	phoneCall.Configure(State.OffHook)
	    .Permit(Trigger.CallDialed, State.Ringing);
        
	phoneCall.Configure(State.Ringing)
	    .Permit(Trigger.HungUp, State.OffHook)
	    .Permit(Trigger.CallConnected, State.Connected);
 
	phoneCall.Configure(State.Connected)
	    .OnEntry(() => StartCallTimer())
	    .OnExit(() => StopCallTimer())
	    .Permit(Trigger.LeftMessage, State.OffHook)
	    .Permit(Trigger.HungUp, State.OffHook)
	    .Permit(Trigger.PlacedOnHold, State.OnHold);

	// ...

	phoneCall.Fire(Trigger.CallDialled);
	Assert.AreEqual(State.Ringing, phoneCall.State);

This project, as well as the example above, was inspired by Simple State Machine.

##Features

Most standard state machine constructs are supported:

* Generic support for states and triggers of any .NET type (numbers, strings, enums, etc.)
* Hierarchical states
* Entry/exit events for states
* Guard clauses to support conditional transitions
* Introspection

Some useful extensions are also provided:

* Ability to store state externally (for example, in a property tracked by Linq to SQL)
* Parameterised triggers
* Reentrant states

##Hierarchical States

In the example below, the OnHold state is a substate of the Connected state. This means that an OnHold call is still connected.

	phoneCall.Configure(State.OnHold)
	    .SubstateOf(State.Connected)
	    .Permit(Trigger.TakenOffHold, State.Connected)
	    .Permit(Trigger.HungUp, State.OffHook)
	    .Permit(Trigger.PhoneHurledAgainstWall, State.PhoneDestroyed);

In addition to the StateMachine.State property, which will report the precise current state, an `IsInState(State)` method is provided. `IsInState(State)` will take substates into account, so that if the example above was in the OnHold state, `IsInState(State.Connected)` would also evaluate to `true`.

##Entry/Exit Events

In the example, the `StartCallTimer()` method will be executed when a call is connected. The `StopCallTimer()` will be executed when call completes (by either hanging up or hurling the phone against the wall.)

The call can move between the Connected and OnHold states without the `StartCallTimer()` and `StopCallTimer()` methods being called repeatedly because the `OnHold` state is a substate of the Connected state.

Entry/Exit event handlers can be supplied with a parameter of type `Transition` that describes the trigger, source and destination states.

##External State Storage

Stateless has been designed with encapsulation within an ORM-ed domain model in mind. Some ORMs place requirements upon where mapped data may be stored. To this end, the StateMachine constructor can accept function arguments that will be used to read and write the state values:

	var stateMachine = new StateMachine<State, Trigger>(
	    () => myState.Value,
	    s => myState.Value = s);

In this example the state machine will use the myState object for state storage.

##Introspection

The state machine can provide a list of the triggers than can be successfully fired within the current state via the `StateMachine.PermittedTriggers` property.

##Guard Clauses

The state machine will choose between multiple transitions based on guard clauses, e.g.:

	phoneCall.Configure(State.OffHook)
	    .PermitIf(Trigger.CallDialled, State.Ringing, () => IsValidNumber)
	    .PermitIf(Trigger.CallDialled, State.Beeping, () => !IsValidNumber);

Guard clauses within a state must be mutually exclusive (multiple guard clauses cannot be valid at the same time.) Substates can override transitions by respecifying them, however substates cannot disallow transitions that are allowed by the superstate.

##Parameterised Triggers

Strongly-typed parameters can be assigned to triggers:

	var assignTrigger = stateMachine.SetTriggerParameters<string>(Trigger.Assign);

	stateMachine.Configure(State.Assigned)
	    .OnEntryFrom(assignTrigger, email => OnAssigned(email));

	stateMachine.Fire(assignTrigger, "joe@example.com");

Trigger parameters can be used to dynamically select the destination state using the `PermitDynamic()` configuration method.

##Ignored Transitions and Reentrant States

Firing a trigger that does not have an allowed transition associated with it will cause an exception to be thrown.

To ignore triggers within certain states, use the `Ignore(TTrigger)` directive:

	phoneCall.Configure(State.Connected)
	    .Ignore(Trigger.CallDialled);

Alternatively, a state can be marked reentrant so its entry and exit events will fire even when transitioning from/to itself:

	stateMachine.Configure(State.Assigned)
	    .PermitReentry(Trigger.Assigned)
	    .OnEntry(() => SendEmailToAssignee());

By default, triggers must be ignored explicitly. To override Stateless's default behaviour of throwing an exception when an unhandled trigger is fired, configure the state machine using the `OnUnhandledTrigger` method:

	stateMachine.OnUnhandledTrigger((state, trigger) => { });

##Project Goals

Stateless is a base for exploration of generic and functional programming to drive workflow in .NET.

This page is an almost-complete description of Stateless, and its explicit aim is to remain minimal.
