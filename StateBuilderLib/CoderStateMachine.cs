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
    
    public partial class CoderStateMachine : CoderBase
    {
        private CoderFeeder coderFeeder;
        private CoderContext coderContext;
        private CoderParallel coderParallel;
        private CoderState coderState;
        //private CoderEventsArgs coderEventsArgs;

        public CoderStateMachine(StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace)
            : base(model, options, codeNamespace)
        {
            this.coderFeeder = new CoderFeeder(model, options, codeNamespace);
         OnCoderFeederCreated ( coderFeeder, model, options, codeNamespace );

            this.coderContext = new CoderContext(model, options, codeNamespace);
         OnCoderContextCreated ( coderContext, model, options, codeNamespace );

            this.coderParallel = new CoderParallel(model, options, codeNamespace);
         OnCoderParallelCreated ( coderParallel, model, options, codeNamespace );

            this.coderState = new CoderState(model, options, codeNamespace);
         OnCoderStateCreated ( coderState, model, options, codeNamespace );

         //this.coderEventsArgs = new CoderEventsArgs(model, options, codeNamespace);

         OnCoderStateMachineCreated ( coderFeeder, coderContext, coderParallel, coderState, model, options, codeNamespace );
        }



      public override void WriteCode()
        {
         OnWriteCodeStarting ( );

            ts.TraceInformation("WriteCode");

            this.coderFeeder.WriteCode();
         OnCoderFeederWritten ( );

            this.coderContext.WriteCode();
         OnCoderContextWritten ( );

            this.coderParallel.WriteCode();
         OnCoderParallelWritten ( );

            this.coderState.WriteCode();
         OnCoderStateWritten ( );


         OnWriteCodeCompleted ( );

           // this.coderEventsArgs.WriteCode();
        }


   }
}
