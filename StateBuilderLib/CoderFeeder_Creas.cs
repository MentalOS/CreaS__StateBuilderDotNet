using System;
using System .CodeDom;
using System .Collections .Generic;
using System .Linq;
using System .Text;

namespace StateForge
{
   partial class CoderFeeder
   {
      /// <summary>
      /// store  event that are feeded.
      /// EventID, FeededClassName
      /// </summary>
      /// 
      public static Dictionary<string, string> FeederEvents { get; private set; }


      partial void OnCreated ( StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace )
      {

         coderEventArgs = new CoderEventsArgs ( model, options, codeNamespace );

      }

      private CoderEventsArgs coderEventArgs;

      partial void OnWriteCodeStarting ( )
      {
         if (FeederEvents == null) FeederEvents = new Dictionary<string, string> ( );

      }

      partial void OnWriteFeederStarting ( string feederName, List<EventType> events, CodeTypeDeclaration feederCode )
      {

         writeInitializeContextEventSubscription ( feederCode, events );

         //navigation line
         feederCode .Comments .Add ( new CodeCommentStatement (
             "**************************************************************************************................ F E E D E R <<<<<<<<<<<<<>>>>>>" + feederName
             ) );

         if (events .Count > 0)
            foreach (var item in events)
            {
               string eventId = item .id;
               string fKey = eventId;

               //add the event that are feeded
               FeederEvents .Add ( fKey, feederName );

            }
      }


      partial void OnWriteFeederCompleted ( string feeder, List<EventType> events, CodeTypeDeclaration feederCode )
      {

         //todo Add IBusPublisher within the Feeder
         /*
          * 
      public IBusPublisher BusPublisher
      {
         get
         {
            if (busPublisher != null)
               return busPublisher;
            return null;
         }
         internal set
         {
            busPublisher = value;
         }
      }
      protected IBusPublisher busPublisher;
      *=
      * */
         string busSnipetCode = @"
///<summary>Bus publishing interfare</summary>
      public virtual IBusPublisher BusPublisher
      {
         get
         {
            //if (busPublisher != null) //todo logics if no bus... will be in context of the event that if interface is null, no publishing...
//Might be interesting to think about using the default App bus like: 
         //DTSApp.PublishBusMessage(message); so we would be setting DTSApp as the default bus if none choosen
               return busPublisher;
            //return DTSApp.BusApp;
         }
         internal set
         {
            this.busPublisher = value;
         }
      }
      protected virtual IBusPublisher busPublisher; //todo set bus publisher object within the feeder
";
         
         feederCode .Members .Add (
            new CodeSnippetTypeMember ( busSnipetCode ) );

      }
      partial void OnWriteEventStarting ( CodeTypeDeclaration feederCode, EventType evt )
      {

         WriteCodeTypeDelegate ( feederCode, evt );

         //formats param of events that have the save name as the event ID 
         //(fasten design and to generate code: EventIDEventArgs automatically)
         formatParametersShortcuts ( evt );








         var _eventFeederMethod = new CodeMemberMethod ( );



         writeRegionStart ( evt, _eventFeederMethod );



         #region Event Caller
         string __methodEventName = evt .id + "Event";

         _eventFeederMethod .Attributes = MemberAttributes .Final;
         _eventFeederMethod .Name = evt .id;
         _eventFeederMethod .Attributes = _eventFeederMethod .Attributes | MemberAttributes .Public;


         feederCode .Members .Add ( _eventFeederMethod );
         #endregion


         //---- PARTIAL----------CodeMemberMethod----------------------


         #region Partials after/before

         // Create partial statement
         string partialPostInvokerName = StateStore .onPostEventPrefix + evt .id;
         string partialPreInvokerName = StateStore .onPreEventPrefix + evt .id;
         var _partialOnPost_declaration = new CodeMemberMethod ( );

         feederCode .Members .Add ( _partialOnPost_declaration );
         var _partialOnPre_declaration = new CodeMemberMethod ( );
         feederCode .Members .Add ( _partialOnPre_declaration );


         #region region end

         var regionpartiolInvokerEND = new CodeRegionDirective ( )
         {
            RegionMode = CodeRegionMode .End,
            RegionText = evt .id
         };
         _partialOnPre_declaration
             .EndDirectives .Add ( regionpartiolInvokerEND );

         #endregion

         _partialOnPost_declaration .Name = partialPostInvokerName;
         _partialOnPost_declaration .Attributes = MemberAttributes .Public;

         _partialOnPost_declaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPost_declaration .ReturnType = new CodeTypeReference ( "partial void" );
         _partialOnPost_declaration .Comments .Add ( new CodeCommentStatement ( "Extension method for " + evt .id ) );

         _partialOnPre_declaration .Name = partialPreInvokerName;
         _partialOnPre_declaration .Attributes = MemberAttributes .Public;

         _partialOnPre_declaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPre_declaration .ReturnType = new CodeTypeReference ( "partial void" );

         #endregion



         #region Comments lines

         _eventFeederMethod .Comments .Add (
           new CodeCommentStatement ( $"-------------{_eventFeederMethod .Name}------------" ) );

         _eventFeederMethod .Comments .Add (
           new CodeCommentStatement ( $"--------------------------------" ) );

         #endregion


         var context_eventInvoker
           = new CodeMethodInvokeExpression (
                         new CodeVariableReferenceExpression (
                           Model .settings .context .instance ),
                                           evt .id );


         CodeDelegateInvokeExpression _eventDelegateInvoker = null;

         string _invokerParam = "";


         //Declare the Event delegate 
         //example: public delegate void HookRetiredEventHandler(object sender, EventArgs e);
         CodeTypeDelegate cd
           = new CodeTypeDelegate ( evt .name + "EventHandler" );


         #region Partial Method Invoker Declaration

         var _partialOnPost_MethodInvoker
           = new CodeMethodInvokeExpression (
                         new CodeThisReferenceExpression ( ),
                                       partialPostInvokerName );

         var _partialOnPre_MethodInvoker
           = new CodeMethodInvokeExpression (
                         new CodeThisReferenceExpression ( ),
                                       partialPreInvokerName );

         #endregion


         if (evt .parameter != null)
         {
            foreach (ParameterType param in evt .parameter)
            {
               #region Add Events Params to Declaration and invoker

               _invokerParam = addEventsParamsTo ( evt, _eventFeederMethod, _partialOnPost_declaration, _partialOnPre_declaration, context_eventInvoker, _partialOnPost_MethodInvoker, _partialOnPre_MethodInvoker, param );

               #endregion



            }
         }



         if (evt .parameter != null)
            _eventDelegateInvoker = createEventInvoker ( evt .id, true, _invokerParam );
         else
            _eventDelegateInvoker = createEventInvoker ( evt .id, false );




         //Extension OnPre
         _eventFeederMethod .Statements .Add ( _partialOnPre_MethodInvoker );


         // Add StateCurrent.EvOpen(this);
         _eventFeederMethod .Statements .Add ( context_eventInvoker );




         #region if delegate not null call event

         /*I want: 
          if (this.EventDELEGATE != null) this.EventDELEGATE(this,e);
          */

         conditionallyInvokeOfEventDelegate ( _eventFeederMethod, __methodEventName, _eventDelegateInvoker );


         #endregion



         //invoke the onPostPartial
         _eventFeederMethod .Statements .Add ( _partialOnPost_MethodInvoker );

         SetMethodVisibility ( _eventFeederMethod, evt );


      }

      internal static void conditionallyInvokeOfEventDelegate ( CodeMemberMethod eventMethod, string __methodEventName, CodeDelegateInvokeExpression _eventDelegateInvoker )
      {
         //CodeSnippetExpression cex = new CodeSnippetExpression(__methodEventName + "!=null");

         //CodeExpression condition2
         //      = new CodeBinaryOperatorExpression();

         //CodeStatement[] trueStatements2 = { new CodeCommentStatement("Call subscriber if any") };


         //CodeConditionStatement ifStatement2 = new CodeConditionStatement(cex, trueStatements2);
         //ifStatement2.TrueStatements.Add(_eventDelegateInvoker);

         //eventMethod.Statements.Add(ifStatement2);
      }

      private void setParamName ( EventType evt, ParameterType param )
      {
         if (param .type == evt .id)
         {
            param .type = evt .id + Options.EventMessageSuffix;
//+ "EventArgs";

         }
      }

      private void addParamToDeclaration ( EventType evt, ParameterType param, CodeParameterDeclarationExpressionCollection targetCollection )
      {
         //todo add param to the collection
         setParamName ( evt, param );

         CodeParameterDeclarationExpression _type = new CodeParameterDeclarationExpression ( GetTypeReference ( param .type ), param .name );
         targetCollection .Add ( _type );
      }

      private void addParamToInvoker ( EventType evt, ParameterType param, CodeExpressionCollection targetCollection )
      {
         //todo add param to the collection         
         setParamName ( evt, param );

         CodeVariableReferenceExpression _name = new CodeVariableReferenceExpression ( param .name );
         targetCollection .Add ( _name );
      }
      private string addEventsParamsTo ( EventType evt, CodeMemberMethod _eventFeederMethod, CodeMemberMethod _partialOnPost_MethodDeclaration, CodeMemberMethod _partialOnPre_MethodDeclaration, CodeMethodInvokeExpression _contextMethodInvoker, CodeMethodInvokeExpression _partialOnPost_MethodInvoker, CodeMethodInvokeExpression _partialOnPre_MethodInvoker, ParameterType param )
      {
         string _invokerParam;
         //Deals with the shortcut of using the same event id as args

         addParamToDeclaration ( evt, param, _eventFeederMethod .Parameters );

         addParamToInvoker ( evt, param, _contextMethodInvoker .Parameters );

         addParamToDeclaration ( evt, param, _partialOnPost_MethodDeclaration .Parameters );

         addParamToInvoker ( evt, param, _partialOnPost_MethodInvoker .Parameters );

         addParamToDeclaration ( evt, param, _partialOnPre_MethodDeclaration .Parameters );

         addParamToInvoker ( evt, param, _partialOnPre_MethodInvoker .Parameters );

         //get the parameter name out of here
         _invokerParam = param .name;
         return _invokerParam;
      }

      private static void writeRegionStart ( EventType evt, CodeMemberMethod _eventFeederMethod )
      {
         var regionpartialInvokerStart = new CodeRegionDirective ( )
         {
            RegionMode = CodeRegionMode .Start,
            RegionText = evt .id
         };

         _eventFeederMethod .StartDirectives .Add ( regionpartialInvokerStart );
      }

      string __methodEventName; string partialPostInvokerName; string partialPreInvokerName;



      void OnWriteEventStartedOLD ( CodeTypeDeclaration feederCode, EventType evt, CodeMemberMethod _eventFeederMethod )
      {
      //   throw new NotSupportedException ( );

         #region region start

         var regionpartialInvokerStart = new CodeRegionDirective ( )
         {
            RegionMode = CodeRegionMode .Start,
            RegionText = evt .id
         };


         #endregion
         __methodEventName = evt .id + "Event";

         _eventFeederMethod .StartDirectives .Add ( regionpartialInvokerStart );






         #region 2



         //---- PARTIAL----------CodeMemberMethod----------------------



         // Create partial statement
         partialPostInvokerName = StateStore .onPostEventPrefix + evt .id;
         partialPreInvokerName = StateStore .onPreEventPrefix + evt .id;
         var _partialOnPost_MethodDeclaration = new CodeMemberMethod ( );
         //_partialOnPost_MethodDeclaration.StartDirectives.Add(regionpartialInvokerStart);

         feederCode .Members .Add ( _partialOnPost_MethodDeclaration );
         var _partialOnPre_MethodDeclaration = new CodeMemberMethod ( );
         feederCode .Members .Add ( _partialOnPre_MethodDeclaration );

         #region region end

         var regionpartiolInvokerEND = new CodeRegionDirective ( )
         {
            RegionMode = CodeRegionMode .End,
            RegionText = evt .id
         };
         _partialOnPre_MethodDeclaration
             .EndDirectives .Add ( regionpartiolInvokerEND );

         #endregion
         _partialOnPost_MethodDeclaration .Name = partialPostInvokerName;
         _partialOnPost_MethodDeclaration .Attributes = MemberAttributes .Public;

         _partialOnPost_MethodDeclaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPost_MethodDeclaration .ReturnType = new CodeTypeReference ( "partial void" );
         _partialOnPost_MethodDeclaration .Comments .Add ( new CodeCommentStatement ( "Extension method for " + evt .id ) );

         _partialOnPre_MethodDeclaration .Name = partialPreInvokerName;
         _partialOnPre_MethodDeclaration .Attributes = MemberAttributes .Public;

         _partialOnPre_MethodDeclaration .Attributes = MemberAttributes .ScopeMask;
         _partialOnPre_MethodDeclaration .ReturnType = new CodeTypeReference ( "partial void" );



         _eventFeederMethod .Comments .Add (
           new CodeCommentStatement ( $"-------------{_eventFeederMethod .Name}------------" ) );

         _eventFeederMethod .Comments .Add (
           new CodeCommentStatement ( $"--------------------------------" ) );



         CodeMethodInvokeExpression _contextMethodInvoker
           = new CodeMethodInvokeExpression (
                         new CodeVariableReferenceExpression (
                           Model .settings .context .instance ),
                                           evt .id );

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



      }

      //goal : Add what goes AFTER the invoke
      partial void OnWriteEventOnPostContextMethodInvokeAdded ( CodeTypeDeclaration feederCode, EventType evt, CodeMethodInvokeExpression methodInvoke )
      {





      }
      //goal : Add what goes before the invoke
      partial void OnWriteEventOnPreContextMethodInvokeAdding ( CodeTypeDeclaration feederCode, EventType evt, CodeMethodInvokeExpression methodInvoke )
      {


      }

      private void formatParametersShortcuts ( EventType evt )
      {
         if (evt .parameter != null)
            foreach (var param in evt .parameter)
            { 

               if (param .type .Contains ( "EventArgs" ))
                  coderEventArgs .WriteEventArgsCode ( evt, CodeNamespace .Name );
           
            }
      }




      private void writeInitializeContextEventSubscription ( CodeTypeDeclaration feederCode, List<EventType> events )
      {
         //add Context prop and field context...
         _member__addContextPropertyAndField ( feederCode );



         //todo passes thru all Events and add Subscription and then handlers



         CodeMemberMethod initMethod = new CodeMemberMethod ( );
         initMethod .Name = "InitializeContext";


         #region I want to set the class visibility in internal, private depending on settings

         //initMethod .StartDirectives.Add(
         //    new CodeRegionDirective(CodeRegionMode.Start, "\ninternal"));

         //initMethod .EndDirectives.Add(
         //    new CodeRegionDirective(CodeRegionMode.End, String.Empty));

         #endregion


         initMethod .Comments .Add ( new CodeCommentStatement ( "Initialize SUBSCRIPTIAON of CONTEXT events" ) );

         SetMethodVisibility ( initMethod, MethodVisibility .@internal );
         //-------------------------------------------------------

         //todo Add partial OnInitializeContextStarting
         string OnInitializeContextStartingName = "OnInitializeContextStarting";

         initMethod .Statements
            .Add (
            new CodeSnippetStatement (
             OnInitializeContextStartingName + "();"
               ) );

         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "partial void " + OnInitializeContextStartingName + "();" ) );
         // CoderBase.Add_Partial__OnCreated__Declaration_Member(feederCode);

         //CodeConstructor ctor = new CodeConstructor();
         //ctor.Attributes = MemberAttributes.Public;

         //CoderBase.Add_Partial__OnCreated__Statement(ctor);
         //feederCode.Members.Add(ctor);

         CodeMethodInvokeExpression iInit = new CodeMethodInvokeExpression ( new CodeMethodReferenceExpression ( ) { MethodName = initMethod .Name } );
         //invoke Initializer in constructor
         //   ctor.Statements.Add(iInit);


         //-------------------------------------------------------


         //todo Add 
         // context = new PhoneContext(this);  
         string contextInitSnippet = Model .settings .context .instance + " = new " + Model .settings .context .@class + "(";

         //todo build the params and set to "this" if the current object is the feeded one
         //ex. new context(bdbo,this,abc)
         List<string> targetParams = new List<string> ( );
         foreach (var o in Model .settings .@object)
         {
            if (o .@class == feederCode .Name)
               targetParams .Add ( "this" );
            else targetParams .Add ( o .instance );
         }
         //todo  Handle if more entities described in the object of the State model.
         int c1 = 0;
         int max = Model .settings .@object .Length;
         //initMethod.Comments.Add(new CodeCommentStatement("Model.settings.@object , feeder: " + feederCode.Name));
         foreach (var o in targetParams)
         {

            contextInitSnippet += o;
            if (c1 < max - 1)
               contextInitSnippet += ",";

            c1++;
         }
         contextInitSnippet += ");";
         initMethod .Statements .Add ( new CodeSnippetStatement ( contextInitSnippet ) );

         string observerEnableValue = "false";
         if (Model .settings .observerconsole)
            observerEnableValue = "true";
         //-----------------------------------------------------
         string observerEnabledFieldName = "EnableObserver";
         feederCode .Members .Add ( new CodeSnippetTypeMember (
            "\t\t\t\t\tpublic static bool " + observerEnabledFieldName + " => " + " DTSApp.EnableObserver" + "; " ) );

         initMethod .Statements .Add ( new CodeSnippetStatement ( "\t\t\t\t\t"
            + "if (" + observerEnabledFieldName + ")"
             + Model .settings .context .instance
             + ".Observer = " + Model .settings .observertarget
             + ";" ) );

         //-----------------------------------------------------

         string evtName = ""; string evtHandler = "";
         foreach (var evt in events)
         {
            evtName = evt .name + "Event";
            evtHandler = evt .name + "ContextHandler";
            string eventArgsInstance = "";
            addEventToContextInitMethodAndAddHandler ( feederCode, initMethod, evtName, evtHandler, evt );

            ////todo Add Params to handler
            //handler.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
            ////todo Add Relevant event args
            //handler.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));

            ////todo snippet to invoke if not null (forward event)
            ////example : this.MyEventDelegate?.Invoke(s,e);
            //string i = $"this.{evtName}?.Invoke(";
            //CodeSnippetStatement invokeFeederEvent = new CodeSnippetStatement();

         } //end adding event
           //todo add EndHandler
           // Context.EndHandler += Context_EndHandler;

         evtName = "EndHandler";
         evtHandler = "Context_EndHandler";

         #region EndHandler forward to feeded class

         EventType evtEndHandler = new EventType ( ) { name = "EndHandler", id = "EndHandler" };

         var contextHandlerMethod =
         addEventToContextInitMethodAndAddHandler ( feederCode, initMethod, evtName, evtHandler, evtEndHandler );
         contextHandlerMethod .Statements .Add ( new CodeSnippetStatement ( "ended=true;" ) );
         contextHandlerMethod .Statements .Add ( new CodeSnippetStatement ( "OnEnding();" ) );
         feederCode .Members .Add ( new CodeSnippetTypeMember ( "partial void OnEnding();" ) );
         feederCode .Members .Add ( new CodeSnippetTypeMember ( "public event EventHandler<EventArgs> EndHandler;" ) );

         //todo wait ending
         feederCode .Members .Add ( new CodeMemberField ( typeof ( bool ), "ended" ) );
         int waitEndingInterval = 44;
         var waitEndingMethod = new CodeMemberMethod ( );
         waitEndingMethod .Name = "WaitEnding";
         waitEndingMethod .Attributes = MemberAttributes .Public;
         waitEndingMethod .Statements .Add ( new CodeSnippetStatement ( @"
int max = 400;
int _c = 0;
while (!ended && _c!=0)
         {
            Thread.Sleep(waitEndingInterval);
_c++;
         }" ) );
         feederCode .Members .Add ( waitEndingMethod );
         feederCode .Members .Add ( new CodeMemberField ( typeof ( int ), "waitEndingInterval" ) );
         initMethod .Statements .Add ( new CodeSnippetStatement ( "waitEndingInterval = " + waitEndingInterval + ";" ) );
         /*
          *
         */
         #endregion

         //todo Add partial OnInitializeContextCompleted
         string OnInitializeContextCompletedName = "OnInitializeContextCompleted";

         initMethod .Statements
            .Add (
            new CodeSnippetStatement (
           "\t\t\t\t\t" + OnInitializeContextCompletedName + "();"
               ) );

         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "\t\t\t\tpartial void " + OnInitializeContextCompletedName + "();" ) );

         feederCode .Members .Add ( initMethod );


         #region Context start


         CodeMemberMethod startMethod = new CodeMemberMethod ( );

         string startMethodName = "StartStateMachine";
         string evStartMethodCallName = Model .events[0] .@event[0] .id;

         string partialOnStartStateMachine = "On" + startMethodName + "Starting";

         string partialOnStartStateMachineCompleted = "On" + startMethodName + "Completed";


         startMethod .Statements
           .Add (
           new CodeSnippetStatement (
            partialOnStartStateMachine + "();"
              ) );






         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "partial void " + partialOnStartStateMachine + "();" ) );

         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "partial void " + partialOnStartStateMachineCompleted + "();" ) );




         startMethod .Attributes = MemberAttributes .Public;
         startMethod .Name = startMethodName;
         startMethod .Statements .Add ( new CodeMethodInvokeExpression ( new CodeMethodReferenceExpression ( new CodeThisReferenceExpression ( ), initMethod .Name ) ) );
         startMethod .Statements .Add ( new CodeSnippetStatement ( Model .settings .context .instance + $".{evStartMethodCallName}();" ) );
         feederCode .Members .Add ( startMethod );

         startMethod .Statements
           .Add (
           new CodeSnippetStatement (
            partialOnStartStateMachineCompleted + "();"
              ) );

         #endregion


         #region Context end

         string endMethodPartialName = "OnStopStateMachineStarting";

         string endMethodPartialNameCompleted = "OnStopStateMachineCompleted";

         CodeMemberMethod endMethod = new CodeMemberMethod ( );



         endMethod .Statements
            .Add (
            new CodeSnippetStatement (
             endMethodPartialName + "();"
               ) );
         int maxEvent = Model .events[0] .@event .Length - 1;

         string evEnd = Model .events[0] .@event[maxEvent] .id;

         endMethod .Attributes = MemberAttributes .Public;
         #region I want to set the class visibility in internal, private depending on settings


         //endMethod.StartDirectives.Add(
         //    new CodeRegionDirective(CodeRegionMode.Start, "\ninternal"));
         //endMethod.EndDirectives.Add(
         //    new CodeRegionDirective(CodeRegionMode.End, String.Empty));

         #endregion

         // endMethod.CustomAttributes.Add(new CodeAttributeDeclaration("JsonIgnore"));

         endMethod .Name = "StopStateMachine";
         endMethod .Statements .Add ( new CodeSnippetStatement ( Model .settings .context .instance + $".{evEnd}();" ) );
         feederCode .Members .Add ( endMethod );


         endMethod .Statements
            .Add (
            new CodeSnippetStatement (
             endMethodPartialNameCompleted + "();"
               ) );



         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "partial void " + endMethodPartialName + "();" ) );

         feederCode .Members .Add (
            new CodeSnippetTypeMember ( "partial void " + endMethodPartialNameCompleted + "();" ) );
         #endregion

         #region Extend the model to take a generic type to the context and other

         #endregion

         #region SetState by its name method

         //ONLY IF FIRST Feeder
         if (Model .settings .@object[0] .@class == feederCode .Name)
         {

            //ex: setState("ValidSignalFound");
            CodeMemberMethod setStateMethod = new CodeMemberMethod ( );
            setStateMethod .Attributes = MemberAttributes .Public;
            setStateMethod .Comments .Add ( new CodeCommentStatement ( "Set the State using String" ) );
            setStateMethod .Name = "SetState";
            //StatusEnum MyStatus = EnumUtil.ParseEnum<StatusEnum>("Active");
            setStateMethod .Parameters .Add ( new CodeParameterDeclarationExpression ( typeof ( string ), "stateString" ) );
            setStateMethod .Statements .Add ( new CodeSnippetStatement ( "this.State = StateBase.ParseEnum<" + StateStore .EnumTypeName + ">(stateString);" ) );
            feederCode .Members .Add ( setStateMethod );
         }

         #endregion
         #region bool IsState
         //goal : Simplify asking if state is
         //TODO Add a Function bool IsState(string)

         if (Model .settings .@object[0] .@class == feederCode .Name)
         {

            //ex: setState("ValidSignalFound");
            CodeMemberMethod isState = new CodeMemberMethod ( );
            isState .Attributes = MemberAttributes .Public;
            isState .ReturnType = new CodeTypeReference ( typeof ( bool ) );
            isState .Comments .Add ( new CodeCommentStatement ( "Is the State using String" ) );
            isState .Name = "IsState";

            isState .Parameters .Add ( new CodeParameterDeclarationExpression ( typeof ( string ), "stateString" ) );

            isState .Statements .Add ( new CodeSnippetStatement ( "return (this.State.ToString().ToLower() == stateString.ToLower()) ?  true : false;" ) );
            feederCode .Members .Add ( isState );
         }
         #endregion

         #region Alias Property StateString

         if (Model .settings .@object[0] .@class == feederCode .Name)
         {
            CodeSnippetTypeMember alaias = new CodeSnippetTypeMember ( "public string __s => State.ToString();" );
            alaias .Comments .Add ( new CodeCommentStatement ( "Alias to get the State as String" ) );

            feederCode .Members .Add ( alaias );
         }
         #endregion

      }

      /// <summary>
      /// addEventToContextInitMethodAndAddHandler
      /// </summary>
      /// <param name="feederCode"></param>
      /// <param name="initMethod"></param>
      /// <param name="evtName"></param>
      /// <param name="evtHandler"></param>
      /// <param name="evt"></param>
      /// <param name="addInvoker"></param>
      /// <returns>the event Handling method</returns>
      private CodeMemberMethod addEventToContextInitMethodAndAddHandler ( CodeTypeDeclaration feederCode, CodeMemberMethod initMethod, string evtName, string evtHandler, EventType evt, bool addInvoker = true )
      {
         _init_addSubscriptionToContextEvent ( initMethod, evtName, evtHandler );

         /*TODO
          I Want: 

private void MyEventDelegateHandler(object s,EventArgs e)
{		this.MyEventDelegate?.Invoke(s,e);
}
          */
         //todo Create Method member
         string _defaultSenderParam = "sender";
         string _defaultParamName = "e";
         #region Init handler

         CodeMemberMethod handler = new CodeMemberMethod ( );
         handler .Attributes = handler .Attributes | MemberAttributes .Private;
         handler .Name = evtHandler;
         feederCode .Members .Add ( handler );
         handler .Parameters .Add ( new CodeParameterDeclarationExpression ( typeof ( object ), _defaultSenderParam ) );
         #endregion
         CodeExpressionCollection paramCollection = new CodeExpressionCollection ( );


         //  invokeDelegate.Method = new CodeMethodReferenceExpression();
         string paramSnip = "\t\t\t\t\t"
             + "this." + evtName + "?.Invoke(";

         paramSnip += _defaultSenderParam + ", ";
         if (evt .parameter != null)
         {
            int last = evt .parameter .Length;
            int c = 0;
            foreach (var param in evt .parameter)
            {
               //tst
               handler .Comments .Add ( new CodeCommentStatement ( "Param: " + param .name ) );

               addParamToDeclaration ( evt, param, handler .Parameters );
               addParamToInvoker ( evt, param, paramCollection );
               //todo add param to event invoker
               paramSnip += param .name;
               if (last - 1 > c)
                  paramSnip += ",";
               c++;
            }
         }
         else
         {
            //todo Param was null, use EventArgs...
            handler .Parameters .Add ( new CodeParameterDeclarationExpression ( typeof ( EventArgs ), _defaultParamName ) );

            paramSnip += _defaultParamName;
         }

         paramSnip += ");";
         var invokeDelegateSnip = new CodeSnippetStatement ( paramSnip );

         if (addInvoker)
            handler .Statements .Add ( invokeDelegateSnip );

         return handler;
      }

      private void _member__addContextPropertyAndField ( CodeTypeDeclaration feederCode )
      {
         CodeMemberProperty contextProp = new CodeMemberProperty ( );
         contextProp .Type = new CodeTypeReference ( Model .settings .context .@class );
         contextProp .Name = "Context";
         CodeMemberField contextField = new CodeMemberField ( new CodeTypeReference ( Model .settings .context .@class ), Model .settings .context .instance );
         contextProp .Attributes = MemberAttributes .Public;

         contextProp .GetStatements .Add ( new CodeMethodReturnStatement ( new CodeSnippetExpression ( Model .settings .context .instance ) ) );
         contextProp .SetStatements .Add ( new CodeSnippetStatement ( defaultTabs + Model .settings .context .instance + "=value;" ) );

         feederCode .Members .Add ( contextField );
         feederCode .Members .Add ( contextProp );
      }

      string defaultTabs = "\t\t";

      //string defaultTabs ="" ;

      private void _init_addSubscriptionToContextEvent ( CodeMemberMethod initMethod, string evtName, string evtHandler )
      {
         initMethod .Statements .Add (
             new CodeCommentStatement (

             CoderBase .LineReplacementFlag +
            defaultTabs +
             Model .settings .context .instance
             + "."
             + evtName +
             "+= "
             + evtHandler
             + ";" ) );
      }
   }
}
