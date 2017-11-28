#region Copyright
//------------------------------------------------------------------------------
// <copyright file="CoderFeeder.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

#region JG Extension
// Added Feature Event Handler and partial method extension Pre and Post event.
// Definition : The Event feature add Event Handler +/ delegate to the generated code when feeder is used. It also add partial method that are called within the feeder. these method body can be declared when wanting to extend.
// Modified by: J.Guillaume D.-Isabelle, 2017
#endregion

namespace StateForge
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.IO;

    partial class CoderFeeder : CoderBase
    {
        public CoderFeeder(StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace)
            : base(model, options, codeNamespace)
        {

            //ext
            OnCreated(model, options, codeNamespace);
        }

        partial void OnCreated(StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace);


        partial void OnWriteCodeStarting();
        public override void WriteCode()
        {
            //ext
            OnWriteCodeStarting();


            foreach (var pair in Model.FeedersMap)
            {
                WriteFeeder(pair.Key, pair.Value);

                #region Test to see what is in the tst feeder...
                bool tst = false;
                if (tst)
                {

                    string V = "tst-feeders-" + pair.Key + ".txt";

                    File.WriteAllText(V, "TEST Feeders\n\n");

                    File.AppendAllText(V, "Key: " + pair.Key + "::\n");
                    foreach (var item in pair.Value)
                    {
                        File.AppendAllText(V, pair.Key + "::" + item.id + "\n");

                    }
                    File.AppendAllText(V, "\n");
                }
                #endregion
            }
        }



        //public partial class HelloWorld
        //{
        //    public void Print()
        //    {
        //        context.EvPrint();
        //    }
        //}

        partial void OnWriteFeederStarting(string feeder, List<EventType> events, CodeTypeDeclaration feederCode);
        partial void OnWriteFeederCompleted(string feeder, List<EventType> events, CodeTypeDeclaration feederCode);



        private void WriteFeeder(string feeder, List<EventType> events)
        {


            CodeTypeDeclaration feederCode = new CodeTypeDeclaration(feeder);
            CodeNamespace.Types.Add(feederCode);

            //ext
            OnWriteFeederStarting(feeder, events, feederCode);

            var enumStateTst = new CodeCommentStatement(StateStore.PropertyCodeOut);

            feederCode.Comments.Add(enumStateTst);



            feederCode.Comments.Add(new CodeCommentStatement("Feeded class entity"));


            // doc
            feederCode.Comments.Add(new CodeCommentStatement(feeder));

            // public
            feederCode.Attributes = MemberAttributes.Public;

            // partial
            feederCode.IsPartial = true;

            foreach (EventType evt in events)
            {

                WriteEvent(feederCode, evt);

            }
            

         OnWriteFeederCompleted (feeder, events,feederCode );
        }

        //    public void Print()
        //    {
        //        context.EvPrint();
        //    }

        partial void OnWriteEventStarting(CodeTypeDeclaration feederCode, EventType evt);
        partial void OnWriteEventStarted(CodeTypeDeclaration feederCode, EventType evt, CodeMemberMethod _eventFeederMethod);

        /// <summary>
        /// is called after the feeder invoked the context code
        /// </summary>
        /// <param name="feederCode"></param>
        /// <param name="evt"></param>
        /// <param name="_eventFeederMethod"></param>
        /// <param name="_contextMethodInvoker"></param>
        /// <param name="partialPostInvokerName"></param>
        /// <param name="partialPreInvokerName"></param>
        partial void OnWriteEventCodeMethodInvokeExpressionWritten(CodeTypeDeclaration feederCode, EventType evt, CodeMemberMethod _eventFeederMethod, CodeMethodInvokeExpression _contextMethodInvoker);

        bool once = false;
        /// <summary>
        /// Executes when into the loop parsing Event parameter
        /// </summary>
        /// <param name="feederCode"></param>
        /// <param name="evt"></param>
        /// <param name="param"></param>
        partial void OnWriteEventIteratingEventParameter(CodeTypeDeclaration feederCode, EventType evt, ParameterType param, CodeParameterDeclarationExpression _paramDecl, CodeVariableReferenceExpression _paramRef);

        private void WriteEventOri_migrating(CodeTypeDeclaration feederCode, EventType evt)
        {
            //ext
            OnWriteEventStarting(feederCode, evt);

            CodeMemberMethod eventMethod = new CodeMemberMethod();
            feederCode.Members.Add(eventMethod);


            eventMethod.Attributes = MemberAttributes.Final;

            eventMethod.Name = evt.id;
            eventMethod.Attributes = eventMethod.Attributes | MemberAttributes.Public;

            eventMethod.Comments.Add(new CodeCommentStatement(eventMethod.Name));

            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(
                                              new CodeVariableReferenceExpression(Model.settings.context.instance),
                                              evt.id);

            //ext
            OnWriteEventStarted(feederCode, evt, eventMethod);

            if (evt.parameter != null)
            {
                foreach (ParameterType param in evt.parameter)
                {
                    var _paramDecl =
new CodeParameterDeclarationExpression(GetTypeReference(param.type), param.name);
                    var _paramRef = new CodeVariableReferenceExpression(param.name);

                    OnWriteEventIteratingEventParameter(feederCode, evt, param, _paramDecl, _paramRef);

                    eventMethod.Parameters.Add(_paramDecl);
                    methodInvoke.Parameters.Add(_paramRef);
                }

            }

            //ext
            OnWriteEventOnPreContextMethodInvokeAdding(feederCode, evt, methodInvoke);

            // Add StateCurrent.EvOpen(this);
            eventMethod.Statements.Add(methodInvoke);

            OnWriteEventOnPostContextMethodInvokeAdded(feederCode, evt, methodInvoke);

            //ext
            OnWriteEventCompleted(feederCode, evt, methodInvoke);

        }

        partial void OnWriteEventOnPostContextMethodInvokeAdded(CodeTypeDeclaration feederCode, EventType evt, CodeMethodInvokeExpression methodInvoke);

        partial void OnWriteEventCompleted(CodeTypeDeclaration feederCode, EventType evt, CodeMethodInvokeExpression methodInvoke);

        partial void OnWriteEventOnPreContextMethodInvokeAdding(CodeTypeDeclaration feederCode, EventType evt, CodeMethodInvokeExpression methodInvoke);

        private void WriteEvent(CodeTypeDeclaration feederCode, EventType evt)
        {

            OnWriteEventStarting(feederCode, evt);


        }

  

        private void WriteCodeTypeDelegate(CodeTypeDeclaration feederCode, EventType evt, bool specificArgs = true, string invokerParam = "e")
        {
            string id = evt.id;
            string eid = evt.id + "Event";
            string eidh = eid + "Handler";
            string eidArgs = eid + "Args";

            CodeTypeDelegate cd = new CodeTypeDelegate(eidh);


            WriteStatePropPlaceholder(cd);

            //add object sender
            CodeParameterDeclarationExpression sender = new CodeParameterDeclarationExpression("System.Object", "sender");

            cd.Parameters.Add(sender);

            cd.Comments.Add(new CodeCommentStatement("DELEGATE  for :" + eid));
            cd.Attributes = MemberAttributes.Public;


            if (evt.parameter != null)
            {
                foreach (ParameterType param in evt.parameter)
                {
                    if (!param.type.Contains(evt.id))
                    {
                        //specific if parameters are different from the event id 

                        CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
                         param.type, param.name);

                        cd.Parameters.Add(ee);


                    }
                    else //the Param type name has the Event Id in it, we assume we want to do some more when that happens...
                    {
                        if (param.type.Contains("Args"))
                        {

                            CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
                             eidArgs, invokerParam);

                            cd.Parameters.Add(ee);

                        }


                  //--------------------------------------------
                  //TODO See if this section is still relevant and explain what it does more clearly.
                  #region passes to clarify about param type and Event Id.

                  //passes that add EventArgs to the type generated in the ....
                  if (param.type == id)
                        {
                     param .type += OptionsStatic .EventMessageSuffix;
                        //"EventArgs";  //add event args to the type  (goal: Quickly model argument by just pasting the same id,param e, name to an event


                     //What does that do??

                            CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
                             eidArgs, invokerParam);

                            cd.Parameters.Add(ee);

                        }
                    }
                  #endregion

                }
            }
            else
            {

                eidArgs = "Event" + "Args"; //Standard event args for no param events

                CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
                 eidArgs, invokerParam);

                cd.Parameters.Add(ee);
            }


            feederCode.Members.Add(cd);

            //generate the code event
            /*
             I want : 
          public event NumberPressedEventHandler NumberPressedEvent;

             */

            CodeMemberEvent cme = new CodeMemberEvent();
            cme.Type = new CodeTypeReference(eidh);
            cme.Name = eid;
            cme.Comments.Add(new CodeCommentStatement("use to register event " + eid));
            cme.Attributes = MemberAttributes.Public;
            feederCode.Members.Add(cme);
        }

        private void WriteStatePropPlaceholder(CodeTypeDeclaration tCode)
        {
            if (!stateVarPlaceHolderWritten)
            {
                tCode.Comments.Add(new CodeCommentStatement(StateVarTarget));
                stateVarPlaceHolderWritten = true;
            }
        }

        bool stateVarPlaceHolderWritten = false;
        public static string StateVarTarget = "StateVarTarget";

        /// <summary>
        /// Write  Event Invoker
        /// </summary>
        /// <param name="id">id of event</param>
        /// <param name="specificArgs">if specific args</param>
        /// <param name="invokerParam">name of the invoked param</param>
        /// <returns></returns>
        private CodeDelegateInvokeExpression createEventInvoker(string id, bool specificArgs = true, string invokerParam = "e")
        {

            string eid = id;
            string eidArgs = eid;

            if (specificArgs)
            {

                eid = id + "Event";
                eidArgs = eid + "Args"; //Standard event args for no param events
                CodeDelegateInvokeExpression invoke1 = new CodeDelegateInvokeExpression
                  (
                    new CodeEventReferenceExpression
                      (
                         new CodeThisReferenceExpression(), eid
                      ),
                        new CodeExpression[]
                        {
                  new CodeThisReferenceExpression(),
                  new CodeVariableReferenceExpression(invokerParam)
                        }
                    );
                return invoke1;
            }
            else
            {
                eid = id + "Event";
                eidArgs = "Event" + "Args"; //Standard event args for no param events
                CodeDelegateInvokeExpression invoke1 = new CodeDelegateInvokeExpression
                (
                  new CodeEventReferenceExpression
                    (
                       new CodeThisReferenceExpression(), eid
                    ),
                      new CodeExpression[]
                      {
                  new CodeThisReferenceExpression(),
                  new CodeObjectCreateExpression(eidArgs)
                      }
                  );
                return invoke1;
            }

        }

    }
}
