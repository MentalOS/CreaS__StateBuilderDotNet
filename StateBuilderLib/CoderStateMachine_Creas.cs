#region Copyright
//------------------------------------------------------------------------------
// <copyright file="CoderStateMachine.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

namespace StateForge
{
    using System;
    using System.CodeDom;
    
    public partial class CoderStateMachine  
    {
      


      #region OnCoderState Created Extensions

      partial void OnCoderStateMachineCreated ( CoderFeeder coderFeeder, CoderContext coderContext, CoderParallel coderParallel, CoderState coderState, StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace );

      partial void OnCoderStateCreated ( CoderState coderState, StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace );

      partial void OnCoderParallelCreated ( CoderParallel coderParallel, StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace );

      partial void OnCoderContextCreated ( CoderContext coderContext, StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace );

      partial void OnCoderFeederCreated ( CoderFeeder coderFeeder, StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace );

      #endregion

   


      #region OnWriteCode Extensions

      partial void OnWriteCodeCompleted ( );

      partial void OnCoderStateWritten ( );

      partial void OnCoderParallelWritten ( );

      partial void OnCoderContextWritten ( );

      partial void OnCoderFeederWritten ( );

      partial void OnWriteCodeStarting ( );
      #endregion
   }
}
