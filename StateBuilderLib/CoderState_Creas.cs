using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PS.Common.SCDE.CodeGeneratorLib;

namespace StateForge
{
    partial class CoderState
    {
        partial void OnWriteCodeCompleted(StateType state,StateMachineType model)
        {
            //writes the enum State store
            StateStore.WriteClass();
             
        }

        Dictionary<string, string> partialInvokers;

        partial void OnWriteOnEntryExitStarting(CodeTypeDeclaration stateCode, StateType state, ActionsType action, string onEntryExit, CodeMemberMethod onMethod)
        {
            if (partialInvokers == null) partialInvokers = new Dictionary<string, string>();



            //explore what data we can get from the base class
            string baseContext = GetContextBaseClassName(state);

            string contextClassName = GetContextClassName(state);



            #region Partial OnEntryExit Extention

            writePartialDeclaration(onEntryExit, onMethod, contextClassName, "Starting");

            writePartialDeclaration(onEntryExit, onMethod, contextClassName, "Ending");

            #endregion


            #region Feeder Object state set
            //I want 
            //  context._instance.State = CurrentState;

            //todo Put all that into a Method....

            writeStateEntryStateSetter(state, onMethod,onEntryExit);



            #endregion


        }

        /// <summary>
        /// Write the State enum to the current state when OnEntry is called by the context.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="onMethod"></param>
        /// <param name="onEntryExit"></param>
        private void writeStateEntryStateSetter(StateType state, CodeMemberMethod onMethod, string onEntryExit)
        {

            string o = GetStateHierarchicalName(state);


            string dType = Model.settings.@object[0]
                            .@class;
            string dTypeInstance = Model.settings.@object[0]
                            .instance;

            string enumTypeName = StateStore.EnumTypeName;


            string currentStateEnumPart
                = StateStore.GetStateEnumHierarchyString(state);

            string statePropSetterTest = "";

            if (state.name != Model.state.name) // state not root state            
                statePropSetterTest =makeStatePropertySetterStatement(dType, dTypeInstance, enumTypeName, currentStateEnumPart);
            
            
            //default if state is root
            if (state.name == StateStore.ParentStateName)            
                statePropSetterTest =makeStatePropertySetterStatement(dType, dTypeInstance, enumTypeName,  StateStore.defaultEntryStateEnum );
            

       

            if (onEntryExit == "OnEntry"
               // && state.name != Model.state.name
               )
            {
                CodeSnippetStatement enumSetterState
                    = new CodeSnippetStatement(statePropSetterTest);
                 


                //todo add the snippet to the method
                onMethod.Statements.Add(enumSetterState);
            }

        }

        /// <summary>
        /// creates the setter string statement that goes within an OnEntry method
        /// </summary>
        /// <example>
        /// context.Phone_p.State = PhoneStateEnum.Active_DialToning;
        /// </example>
        /// <param name="feederType"></param>
        /// <param name="feederTypeInstance"></param>
        /// <param name="enumTypeName"></param>
        /// <param name="currentStateEnumPart"></param>
        /// <returns></returns>
        private string makeStatePropertySetterStatement(string feederType, string feederTypeInstance, string enumTypeName, string currentStateEnumPart)
        =>
                                 "\t\t\t\t"
                                +   Model.settings.context.instance
                                + "." 
                                + feederType + "_"
                                + feederTypeInstance
                                + "." 
                                + StateStore.stateEnumFeederPropertyName+" = "
                                + enumTypeName + "." 
                                + currentStateEnumPart
                                + ";"
                                ;

        

        partial void OnWriteStateStarted(StateType state, string stateClassName, string contextClassName, CodeTypeDeclaration stateCode)
        {


           stateCode .Comments.Add(new CodeCommentStatement(                "**************************************************************************************///////////////.... S T A T E .................." + stateClassName
                ));

            // Add state Type in doc
            stateCode.Comments.Add(new CodeCommentStatement("State:: " + state.name));


            stateCode.Comments.Add(new CodeCommentStatement("-------------------------------------------------"));

            //------------------------------------
            #region That is where I could grab on the states to extend the feeder with an enum state



            //todo test writting State list to file
            if (StateStore.States == null) StateStore.States = new List<string>();
            if (StateStore.StatesType == null) StateStore.StatesType = new List<StateType>();
            StateStore.States.Add(state.name);
            if (state != null)
                StateStore.StatesType.Add(state);
            StateStore.Namespace = CodeNamespace.Name;

            #endregion

        }



       





        partial void OnWriteOnEntryExitEnding(CodeTypeDeclaration stateCode, StateType state, ActionsType action, string onEntryExit, CodeMemberMethod onMethod, string x)
        {
            writePartialInvoker(onEntryExit, onMethod, x);          

        }



        private void writePartialDeclaration(string onEntryExit, CodeMemberMethod onMethod, string contextClassName, string x)
        {
            string partialMethodName = onEntryExit + x + "Extension";
            string partialSignature = "(" + contextClassName + " context)";
            string txtFlagReplacer = CoderBase.LineReplacementFlag;
            string partialInvoker = txtFlagReplacer + partialMethodName + partialSignature + ";";

            if (!partialInvokers.ContainsKey(onEntryExit))
                partialInvokers.Add(onEntryExit, partialMethodName);
            //partialInvokers.Add(onEntryExit, partialInvoker);

            string partialCode =

                "partial void "
                + partialMethodName
                + partialSignature
                + ";";

            onMethod
              .Comments
              .Add(new CodeCommentStatement(partialCode));
        }

        private void writePartialInvoker(string onEntryExit, CodeMemberMethod onMethod, string x)
        {
            string mname = onEntryExit + x + "Extension";

            //if (partialInvokers != null)
            //    foreach (var invokerPartial in partialInvokers)
            //    {
            //        if (invokerPartial.Key == onEntryExit) //pick the right one
            //            mname = partialInvokers[onEntryExit];
            //    }



            CodeMethodInvokeExpression _partial_XTension_Invoker
        = new CodeMethodInvokeExpression(
                      new CodeThisReferenceExpression(),
                                    mname);

            _partial_XTension_Invoker.Parameters.Add(new CodeVariableReferenceExpression(Model.settings.context.instance));

            onMethod.Statements.Add(_partial_XTension_Invoker);
        }




        partial void OnWriteConstructorCompleted(CodeTypeDeclaration stateCode, StateType state, CodeConstructor ctor)
        {
            //todo Write Partial OnCreated() stuff
            CoderBase.Add_Partial__OnCreated__Declaration_Member(stateCode);
            //  stateCode.Members.Add(CoderBase.  Create_Partial__OnCreated__Declaration_Member());
            CoderBase.Add_Partial__OnCreated__Statement(ctor);
       //     ctor.Statements.Add(CoderBase. Create_Partial__OnCreated__Statement() );

        }

    }
}
