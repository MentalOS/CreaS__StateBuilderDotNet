#region Copyright
//------------------------------------------------------------------------------
// <copyright file="State.cs" company="StateForge">
//      Copyright (c) 2010-2012 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace StateForge.StateMachine
{
    using System;
    
    /// <summary>
    /// The kind of state.
    /// </summary>
    public enum StateKind
    {
        /// <summary>
        /// A leaf state, it doesn't have any child.
        /// </summary>
        Leaf=0,
        
        /// <summary>
        /// The root state, it doesn't have any parent state.
        /// </summary>
        Root=1,
        
        /// <summary>
        /// An error state.
        /// </summary>
        Error=2,
        
        /// <summary>
        /// A final state.
        /// </summary>
        Final=3,
        
        /// <summary>
        /// A composite state, it has child state.
        /// </summary>
        Composite=4,
        
        /// <summary>
        /// A parallel state, it has child state which run in parallel.
        /// </summary>
        Parallel=5,
    }

    /// <summary>
    /// The base class for representing a state.
    /// </summary>
    public abstract class StateBase 
    {
        /// <summary>
        /// Gets or sets the kind of state.
        /// </summary>
        public StateKind Kind { get; protected set; }

        /// <summary>
        /// Gets or sets the state name.
        /// </summary>
        public string Name { get; protected set; }


      // StateForge.StateMachine.StateBase.
      /// <summary>
      /// ParseEnum
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="value"></param>
      /// <returns></returns>
      public static T ParseEnum<T>(string value)
         {
            return (T)Enum .Parse ( typeof ( T ), value, true );
         }
}

    /// <summary>
    /// The base class for representing a state.
    /// </summary>
    /// <typeparam name="TContext">A Context class.</typeparam>
    /// <typeparam name="TState">A State class.</typeparam>
    public  abstract partial class State<TContext, TState> : StateBase 
    {
        /// <summary>
        ///  Initializes a new instance of the State class.
        /// </summary>
        /// <param name="name">The state name.</param>
        /// <param name="kind">The kind of state.</param>
        protected State(string name, StateKind kind)
        {
            this.Name = name;
            this.Kind = kind;
        }

        /// <summary>
        /// Initializes a new instance of the State class.
        /// </summary>
        /// <param name="name">The state name.</param>
        protected State(string name)
        {
            this.Name = name;
            this.Kind = StateKind.Leaf;
        }
        
        /// <summary>
        /// Initializes a new instance of the State class.
        /// </summary>
        protected State()
        {
        }
        


        /// <summary>
        /// Gets or sets the eventual parent state.
        /// </summary>
        public TState StateParent { get; protected set; }

        /// <summary>
        /// The method is invoked when entering this state.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void OnEntry(TContext context);

        /// <summary>
        /// The method is invoked when leaving this state.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void OnExit(TContext context);
    }
}