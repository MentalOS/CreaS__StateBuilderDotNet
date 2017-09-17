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

  class CoderFeeder : CoderBase
  {
    public CoderFeeder(StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace)
        : base(model, options, codeNamespace)
    {
    }

    public override void WriteCode()
    {
      foreach (var pair in Model.FeedersMap)
      {
        WriteFeeder(pair.Key, pair.Value);
      }
    }

    //public partial class HelloWorld
    //{
    //    public void Print()
    //    {
    //        context.EvPrint();
    //    }
    //}
    private void WriteFeeder(string feeder, List<EventType> events)
    {
      CodeTypeDeclaration feederCode = new CodeTypeDeclaration(feeder);
      CodeNamespace.Types.Add(feederCode);

      var t = new CodeCommentStatement("MYMODIF_CLASS_COMMENT");

      feederCode.Comments.Add(t);
      // doc
      feederCode.Comments.Add(new CodeCommentStatement(feeder));

      // public
      feederCode.Attributes = MemberAttributes.Public;

      // partial
      feederCode.IsPartial = true;

      foreach (EventType evt in events)
      {
        CreateCodeTypeDelegate(feederCode, evt);
        WriteEvent(feederCode, evt);

      }

      //  AddProperties(feederCode);
    }

    //    public void Print()
    //    {
    //        context.EvPrint();
    //    }
    private void WriteEvent(CodeTypeDeclaration feederCode, EventType evt)
    {
      CodeMemberMethod _eventFeederMethod = new CodeMemberMethod();
      feederCode.Members.Add(_eventFeederMethod);

      _eventFeederMethod.Attributes = MemberAttributes.Final;

      _eventFeederMethod.Name = evt.id;
      _eventFeederMethod.Attributes = _eventFeederMethod.Attributes | MemberAttributes.Public;



      //---- PARTIAL----------CodeMemberMethod----------------------
      // Create partial statement
      string partialPostInvokerName = "OnPost" + evt.id;
      string partialPreInvokerName = "OnPre" + evt.id;
      CodeMemberMethod _partialOnPost_MethodDeclaration = new CodeMemberMethod();
      feederCode.Members.Add(_partialOnPost_MethodDeclaration);
      CodeMemberMethod _partialOnPre_MethodDeclaration = new CodeMemberMethod();
      feederCode.Members.Add(_partialOnPre_MethodDeclaration);

      _partialOnPost_MethodDeclaration.Name = partialPostInvokerName;
      _partialOnPost_MethodDeclaration.Attributes = MemberAttributes.Public;

      _partialOnPost_MethodDeclaration.Attributes = MemberAttributes.ScopeMask;
      _partialOnPost_MethodDeclaration.ReturnType = new CodeTypeReference("partial void");
      _partialOnPost_MethodDeclaration.Comments.Add(new CodeCommentStatement("Extension method for " + evt.id));

      _partialOnPre_MethodDeclaration.Name = partialPreInvokerName;
      _partialOnPre_MethodDeclaration.Attributes = MemberAttributes.Public;

      _partialOnPre_MethodDeclaration.Attributes = MemberAttributes.ScopeMask;
      _partialOnPre_MethodDeclaration.ReturnType = new CodeTypeReference("partial void");



      _eventFeederMethod.Comments.Add(
        new CodeCommentStatement($"-------------{_eventFeederMethod.Name}------------"));

      _eventFeederMethod.Comments.Add(
        new CodeCommentStatement($"--------------------------------"));



      CodeMethodInvokeExpression _contextMethodInvoker
        = new CodeMethodInvokeExpression(
                      new CodeVariableReferenceExpression(
                        Model.settings.context.instance),
                                        evt.id);

      CodeDelegateInvokeExpression _eventDelegateInvoker = null;

      string _invokerParam = "";


      //Declare the Event delegate 
      //example: public delegate void HookRetiredEventHandler(object sender, EventArgs e);
      CodeTypeDelegate cd
        = new CodeTypeDelegate(evt.name + "EventHandler");


      CodeMethodInvokeExpression _partialOnPost_MethodInvoker
        = new CodeMethodInvokeExpression(
                      new CodeThisReferenceExpression(),
                                    partialPostInvokerName);

      CodeMethodInvokeExpression _partialOnPre_MethodInvoker
        = new CodeMethodInvokeExpression(
                      new CodeThisReferenceExpression(),
                                    partialPreInvokerName);



      if (evt.parameter != null)
      {
        foreach (ParameterType param in evt.parameter)
        {

          //Deals with the shortcut of using the same event id as args
          if (param.type == evt.id)
          {
            param.type = evt.id + "EventArgs";
          }


          _eventFeederMethod.Parameters.Add(new CodeParameterDeclarationExpression(GetTypeReference(param.type), param.name));
          _contextMethodInvoker.Parameters.Add(new CodeVariableReferenceExpression(param.name));
          _partialOnPost_MethodDeclaration.Parameters.Add(new CodeParameterDeclarationExpression(GetTypeReference(param.type), param.name));
          _partialOnPost_MethodInvoker.Parameters.Add(new CodeVariableReferenceExpression(param.name));

          _partialOnPre_MethodDeclaration.Parameters.Add(new CodeParameterDeclarationExpression(GetTypeReference(param.type), param.name));
          _partialOnPre_MethodInvoker.Parameters.Add(new CodeVariableReferenceExpression(param.name));




          //get the parameter name out of here
          _invokerParam = param.name;
        }
      }


      if (evt.parameter != null)
        _eventDelegateInvoker = CreateEventInvoker(evt.id, true, _invokerParam);
      else
        _eventDelegateInvoker = CreateEventInvoker(evt.id, false);


      //Extension OnPre
      _eventFeederMethod.Statements.Add(_partialOnPre_MethodInvoker);


      // Add StateCurrent.EvOpen(this);
      _eventFeederMethod.Statements.Add(_contextMethodInvoker);
      _eventFeederMethod.Statements.Add(_eventDelegateInvoker);



      _eventFeederMethod.Statements.Add(_partialOnPost_MethodInvoker);




    }

    /* I want this to be generated : 
     * 
    public delegate void NumberPressedEventHandler(object sender,NumberPressedEventArgs e);
    public event NumberPressedEventHandler NumberPressedEvent;

    */

    private void CreateCodeTypeDelegate(CodeTypeDeclaration feederCode, EventType evt, bool specificArgs = true, string invokerParam = "e")
    {
      string id = evt.id;
      string eid = evt.id + "Event";
      string eidh = eid + "Handler";
      string eidArgs = eid + "Args";

      CodeTypeDelegate cd = new CodeTypeDelegate(eidh);


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
          else
          {
            if (param.type.Contains("Args"))
            {

              CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
               eidArgs, invokerParam);

              cd.Parameters.Add(ee);

            }

            //passes that add EventArgs to the type generated in the ....
            if (param.type == id)
            {
              param.type += "EventArgs";  //add event args to the type  (goal: Quickly model argument by just pasting the same id,param e, name to an event



              CodeParameterDeclarationExpression ee = new CodeParameterDeclarationExpression(
               eidArgs, invokerParam);

              cd.Parameters.Add(ee);

            }
          }

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

    /// <summary>
    /// Create Event Invoker
    /// </summary>
    /// <param name="id">id of event</param>
    /// <param name="specificArgs">if specific args</param>
    /// <param name="invokerParam">name of the invoked param</param>
    /// <returns></returns>
    private CodeDelegateInvokeExpression CreateEventInvoker(string id, bool specificArgs = true, string invokerParam = "e")
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
