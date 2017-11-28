using System;
using System.Collections.Generic;
using System.Text;

namespace StateForge.StateMachine
{
    /// <summary>
    /// ENtity State
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TState"></typeparam>
    public interface IEntityState<TContext, TState>
    //public abstract class State<TContext, TState> : StateBase 

    {
        /// <summary>
        /// State Context of the entity
        /// </summary>
        TContext Context { get; set; }
    }
}
