using System;
using System .CodeDom;
using System .Collections .Generic;
using System .IO;
using System .Linq;
using System .Text;

namespace StateForge
{
   partial class CoderContext
   {
      partial void OnWriteClassStarting ( StateType state )
      {




      }
      partial void OnWriteClassStarted ( StateType state, CodeTypeDeclaration contextCode, string contextClassName, string contextParentClassName, string stateClassName )
      {


         // File.WriteAllText("dev-state-contextname2.txt", "context class name : " + contextClassName                + "\ncontextParentClassName: " + contextParentClassName                + "\n stateClassName: " + stateClassName                );

         //  contextCode.Comments.Add(new CodeCommentStatement("MyContextCodeComment:" + contextClassName));

         // doc
         contextCode .Comments .Add ( new CodeCommentStatement ( contextClassName ) );

         //long navigation line
         contextCode .Comments .Add ( new CodeCommentStatement (
             "***************************************************************************************************** C O N T E X T *************** " + contextClassName
             ) );
      }


      partial void OnWriteEventsIteratingEventStarting ( CodeTypeDeclaration contextCode, StateType state, EventType evt )
      {
         //todo Change event param when the same as eventID add EventArgs
         //
         string id = evt .id;
         if (evt .parameter != null)
            foreach (ParameterType param in evt .parameter)
            {
               if (param .type == id)
                  param .type += "EventArgs";  //add event args to the type  (goal: Quickly model argument by just pasting the same id,param e, name to an event
            }



         //imported
         WriteCodeTypeDelegate ( contextCode, evt );

         //imported
         //   WriteEvent(contextCode, evt);
      }




      #region ASync
      partial void OnWriteEventSyncStarting ( CodeTypeDeclaration contextCode, StateType state, EventType evt, CodeMemberMethod eventMethod, bool asynchronous, CodeMethodInvokeExpression methodInvoke )
      {
       


         //call our class that we migrate to add the logics of the event within it
         WriteEvent ( contextCode, evt, eventMethod, asynchronous, methodInvoke );

      }

      partial void OnWriteEventAsyncStarting ( CodeTypeDeclaration contextCode, StateType state, EventType evt, int evenId )
      {
         //todo code if ASync
      }
      #endregion











      #region event

      private void WriteEvent ( CodeTypeDeclaration contextCode, EventType evt, CodeMemberMethod eventMethod, bool asynchronous, CodeMethodInvokeExpression methodInvoke )
      {



         //todo cleanup old buspublish idea code...
         #region Extension using the buspublish="true" to Event
         //Suggestion : Add the BusName="MyBus" to the Xml

         //  eventMethod.Comments.Add(new CodeCommentStatement("Extending the Model with publishing Event to the BUS"));


         //I want to publish the event to the bus if the 

         //if (evt.buspublishSpecified)
         //{
         //code with the ext
         //string tstModelExtComments =
         //    "...EXTENDED Model Extension :  " + evt.buspublish;

         //eventMethod.Comments.Add(new CodeCommentStatement(tstModelExtComments));

         //string n = "";
         //if (evt.busname != null)
         //    if (evt.busname.Length > 0)
         //    {
         //        n = ", \"" + evt.busname + "\"";
         //    }
         //if (Model .settings .buspublisherenabledSpecified)
         //{

         //   string busSnip = "";
         //   //--------------------------------IBusPublisher--------------
         //   //TODO Consider 171127 Design with the IBusPublisher from the State context that the Feeded object will implement or refer to another object....

         //   busSnip = MakeBusPublishMethodCall2 ( evt );


         //   File .WriteAllText ( "bussnip.txt", busSnip );
         //   bool pubBus = false;

         //   if (evt .buspublish)
         //      pubBus = true;

         //   if (pubBus)
         //      eventMethod .Statements .Add (
         //          new CodeSnippetExpression
         //          ( busSnip ) );
         //}
         //}
         //else
         //{
         //    //code with the ext
         //    string tstModelExtComments =
         //        "Model NO Extension :  " + evt.buspublish;

         //    eventMethod.Comments.Add(new CodeCommentStatement(tstModelExtComments));
         //}
         #endregion

     



         #region region start

         var regionpartialInvokerStart = new CodeRegionDirective ( )
         {
            RegionMode = CodeRegionMode .Start,
            RegionText = evt .id
         };


         #endregion
         string __methodEventName = evt .id + "Event";

         




         //---- PARTIAL----------CodeMemberMethod----------------------



         // Create partial statement
         string partialPostInvokerName = StateStore .onPostEventPrefix + evt .id;
         string partialPreInvokerName = StateStore .onPreEventPrefix + evt .id;

         #region Add Partial declaration

         var _partialOnPost_MethodDeclaration = new CodeMemberMethod ( );
         

         contextCode .Members .Add ( _partialOnPost_MethodDeclaration );

         var _partialOnPre_MethodDeclaration = new CodeMemberMethod ( );

         contextCode .Members .Add ( _partialOnPre_MethodDeclaration );

 
         //---------------------------------- POST / PRE---------------
         //Partial method Declaration  POST / PRE

         _partialOnPost_MethodDeclaration .Name = partialPostInvokerName;
         _partialOnPost_MethodDeclaration .Attributes = MemberAttributes .Public;

         _partialOnPost_MethodDeclaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPost_MethodDeclaration .ReturnType = new CodeTypeReference ( "partial void" );
         _partialOnPost_MethodDeclaration .Comments .Add ( new CodeCommentStatement ( "Extension method for " + evt .id ) );

         _partialOnPre_MethodDeclaration .Name = partialPreInvokerName;
         _partialOnPre_MethodDeclaration .Attributes = MemberAttributes .Public;

         _partialOnPre_MethodDeclaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPre_MethodDeclaration .ReturnType = new CodeTypeReference ( "partial void" );


         #endregion


         #region Event Delegate
         CodeDelegateInvokeExpression _eventDelegateInvoker = null;

         string _invokerParam = "";


         //Declare the Event delegate 
         //example: public delegate void HookRetiredEventHandler(object sender, EventArgs e);
         CodeTypeDelegate cd
           = new CodeTypeDelegate ( evt .name + "EventHandler" );


         CodeMethodInvokeExpression _partialOnPost_MethodInvoker
           = new CodeMethodInvokeExpression (
                         new CodeThisReferenceExpression ( ),
                                       partialPostInvokerName );

         CodeMethodInvokeExpression _partialOnPre_MethodInvoker
           = new CodeMethodInvokeExpression (
                         new CodeThisReferenceExpression ( ),
                                       partialPreInvokerName );

         #endregion

         if (evt .parameter != null)
         {
            foreach (ParameterType param in evt .parameter)
            {
               //Deals with the shortcut of using the same event id as args
               //if (param .type == evt .id) MOVED IN StateMachineXmlModel when model is created           
               //{
               //   param .type = evt .id + "EventArgs";
               //}


               eventMethod .Parameters .Add ( new CodeParameterDeclarationExpression ( GetTypeReference ( param .type ), param .name ) );

               //   _contextMethodInvoker.Parameters.Add(new CodeVariableReferenceExpression(param.name));

               _partialOnPost_MethodDeclaration .Parameters .Add ( new CodeParameterDeclarationExpression ( GetTypeReference ( param .type ), param .name ) );
               _partialOnPost_MethodInvoker .Parameters .Add ( new CodeVariableReferenceExpression ( param .name ) );

               _partialOnPre_MethodDeclaration .Parameters .Add ( new CodeParameterDeclarationExpression ( GetTypeReference ( param .type ), param .name ) );
               _partialOnPre_MethodInvoker .Parameters .Add ( new CodeVariableReferenceExpression ( param .name ) );




               //get the parameter name out of here
               _invokerParam = param .name;
            }
         }


         if (evt .parameter != null)
            _eventDelegateInvoker = WriteEventInvoker ( evt .id, true, _invokerParam );
         else
            _eventDelegateInvoker = WriteEventInvoker ( evt .id, false );


         SetMethodVisibility ( eventMethod, evt );

         //Extension OnPre
         eventMethod .Statements .Add ( _partialOnPre_MethodInvoker );
         

         //extension to Insert after the Partial Pre added
         OnEventMethodPartialPreAdded ( contextCode, evt, eventMethod, asynchronous, methodInvoke );

         #region new 1711 bus code
         //  string feederPropName = GetPropertyName()
         //todo add
         // IBusPublisher __busPub= his .FeederProperty .BusPublisher;
         //...into event context method if buspublish==true

         //IBusPublisher __busPub = this .FeederProperty .BusPublisher;

         #endregion



         // Add StateCurrent.EvOpen(this);
         // _eventFeederMethod.Statements.Add(_contextMethodInvoker);


         //todo ADd if not null invoke
         //_eventFeederMethod.Comments.Add(new CodeCommentStatement("todo Add if..."));
         #region if delegate not null call event
         /*I want: 
          if (this.EventDELEGATE != null) this.EventDELEGATE(this,e);
          */

         CodeSnippetExpression cex = new CodeSnippetExpression ( __methodEventName + "!=null" );

         CodeExpression condition2
               = new CodeBinaryOperatorExpression (

         );

         CodeStatement[] trueStatements2 = { new CodeCommentStatement ( "Do this if true" ) };


         CodeConditionStatement ifStatement2 = new CodeConditionStatement ( cex, trueStatements2 );
         ifStatement2 .TrueStatements .Add ( _eventDelegateInvoker );


         // Add StateCurrent.EvOpen(this);
         eventMethod .Statements .Add ( methodInvoke );

         eventMethod .Statements .Add ( ifStatement2 );


         #endregion

         //  _eventFeederMethod.Statements.Add(_eventDelegateInvoker);


         eventMethod .Statements .Add ( _partialOnPost_MethodInvoker );



         eventMethod .Comments .Add (
           new CodeCommentStatement ( $"--------------------------------" ) );


      }

      /// <summary>
      /// extension to Insert after the Partial Pre added to an Event method
      /// </summary>
      /// <param name="contextCode"></param>
      /// <param name="evt"></param>
      /// <param name="eventMethod"></param>
      /// <param name="asynchronous"></param>
      /// <param name="methodInvoke"></param>
      partial void OnEventMethodPartialPreAdded ( CodeTypeDeclaration contextCode, EventType evt, CodeMemberMethod eventMethod, bool asynchronous, CodeMethodInvokeExpression methodInvoke );




      //todo write comments to write event
      partial void OnWriteEventSyncEntered ( CodeTypeDeclaration contextCode, StateType state, EventType evt, CodeMemberMethod eventMethod )
      {

         eventMethod .Comments .Add (
           new CodeCommentStatement ( $"--------------------------------" ) );

      }


      private string MakeBusPublishMethodCall2 ( EventType evt )
      {
         /* we will want to add to the event context this: 
          * 
          
  IBusPublisher __busPub= this .InstrumentPerspective__InstrumentPerspective .BusPublisher;
          if ( __busPub != null) //bus publish           __busPub .Publish<CreateMarketOverViewerNodeCompletedEventArgs > ( e );

         so  IBusPublisher __busPub= this .MyFeederInstance .BusPublisher;

          */

         return "";
      }
      /// <summary>
      /// This creates the call to the Bus to publish the Event
      /// </summary>
      /// <param name="evt"></param>
      /// <returns></returns>
      private string MakeBusPublishMethodCall ( EventType evt )
      {
         string busSnip = "";
         string n = "";
         if (evt .buspublish)
         {

            if (evt .busname != null)
               if (evt .busname != "")
               {
                  if (evt .busname .Length > 0)
                  {
                     n = ", \"" + evt .busname + "\"";
                  }
               }
               else
               {
                  //Evt bus name null, check if global specified
                  if (Model .settings .busglobalname != null)
                     if (Model .settings .busglobalname != "")

                     {
                        //todo set bus name to global
                        n = ", \"" + Model .settings .busglobalname + "\"";
                     }
               }
            busSnip = "//HERE LIES CALLING THE BUS\n"
                  + $"//{CoderBase .LineReplacementFlag}{Model .settings .buspublishermethod}(\"" +
                  evt .name
                  + "\"" + n
                  +
                  ");";
         }

         //todo Think of publishing its data
         return busSnip;
      }


      /// <summary>
      /// Write  Event Invoker
      /// </summary>
      /// <param name="id">id of event</param>
      /// <param name="specificArgs">if specific args</param>
      /// <param name="invokerParam">name of the invoked param</param>
      /// <returns></returns>
      private CodeDelegateInvokeExpression WriteEventInvoker ( string id, bool specificArgs = true, string invokerParam = "e" )
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
                     new CodeThisReferenceExpression ( ), eid
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
                   new CodeThisReferenceExpression ( ), eid
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

      #endregion







      private void WriteCodeTypeDelegate ( CodeTypeDeclaration contextCode, EventType evt, bool specificArgs = true, string invokerParam = "e" )
      {
         string id = evt .id;
         string eid = evt .id + "Event";
         string eidh = eid + "Handler";
         string eidArgs = eid + "Args";

         CodeTypeDelegate cd = new CodeTypeDelegate ( eidh );


         // WriteStatePropPlaceholder(cd);

         //add object sender
         CodeParameterDeclarationExpression sender = new CodeParameterDeclarationExpression ( "System.Object", "sender" );

         cd .Parameters .Add ( sender );

         cd .Comments .Add ( new CodeCommentStatement ( "DELEGATE  for :" + eid ) );
         cd .Attributes = MemberAttributes .Public;

         if (evt .parameter != null)
         {
            foreach (ParameterType param in evt .parameter)
            {
               if (!param .type .Contains ( evt .id ))
               {
                  //specific if parameters are different from the event id 

                  CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression (
                   param .type, param .name );

                  cd .Parameters .Add ( ee );


               }
               else
               {
                  if (param .type .Contains ( "Args" ))
                  {

                     CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression (
                      eidArgs, invokerParam );

                     cd .Parameters .Add ( ee );

                  }

                  //passes that add EventArgs to the type generated in the ....
                  if (param .type == id)
                  {
                     param .type += "EventArgs";  //add event args to the type  (goal: Quickly model argument by just pasting the same id,param e, name to an event



                     CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression (
                      eidArgs, invokerParam );

                     cd .Parameters .Add ( ee );

                  }
               }

            }
         }
         else
         {

            eidArgs = "Event" + "Args"; //Standard event args for no param events

            CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression (
             eidArgs, invokerParam );

            cd .Parameters .Add ( ee );
         }


         contextCode .Members .Add ( cd );

         //generate the code event
         /*
          I want : 
       public event NumberPressedEventHandler NumberPressedEvent;

          */

         CodeMemberEvent cme = new CodeMemberEvent ( );
         cme .Type = new CodeTypeReference ( eidh );
         cme .Name = eid;
         cme .Comments .Add ( new CodeCommentStatement ( "use to register event " + eid ) );
         cme .Attributes = MemberAttributes .Public;
         contextCode .Members .Add ( cme );
      }

   }
}
