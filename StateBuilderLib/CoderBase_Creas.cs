#region Copyright
//------------------------------------------------------------------------------
// <copyright file="CoderBase.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

namespace StateForge
{
   using System;
   using System .CodeDom;
   using System .Diagnostics;

   public abstract partial class CoderBase
   {

      public static StateBuilderOptions OptionsStatic { get; set; }

      static CoderBase()
      {
         if (OptionsStatic == null)
            OptionsStatic = new StateBuilderOptions ( );

      //   OptionsStatic .EventMessageSuffix = "EventArgs";
      }

      public static string LineReplacementFlag => "@@";
      public static string MethodVisibilityHackReplacementFlag => "@HACK";

      partial void OnCoderBaseCreated ( StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace )
      {
         OptionsStatic = Options; //access option statically
      }

      /// <summary>
      /// hack to set visibility of events - must be last added as it is a comments that hacks it
      /// </summary>
      /// <param name="targetMethod"></param>
      /// <param name="evt"></param>
      public static void SetMethodVisibility ( CodeMemberMethod targetMethod, EventType evt )
      {
         targetMethod .Attributes = MemberAttributes .Overloaded;
         MethodVisibility visibility = MethodVisibility .none;

         string vTag = visibility .ToString ( );
         switch (evt .visibility)
         {
            case "public":
               visibility = MethodVisibility .@public;
               targetMethod .Attributes = MemberAttributes .Public;
               break;
            case "protected":
               visibility = MethodVisibility .@protected;
               break;
            case "internal":
               visibility = MethodVisibility .@internal;
               break;
            case "none":
               visibility = MethodVisibility .none;
               break;
            case "private":
               visibility = MethodVisibility .@private;
               targetMethod .Attributes = MemberAttributes .Private;
               break;
            default:
               visibility = MethodVisibility .@public;
               break;
         }

         //targetMethod.Comments.Add(new CodeCommentStatement(CoderBase.LineReplacementFlag + visibility ));
         if (visibility != MethodVisibility .@public && visibility != MethodVisibility .@private && visibility != MethodVisibility .none)
            targetMethod .CustomAttributes .Add (
               new CodeAttributeDeclaration ( CoderBase .MethodVisibilityHackReplacementFlag + visibility ) );

      }

      public static void SetMethodVisibility ( CodeMemberMethod targetMethod, MethodVisibility visibility )
      {
         targetMethod .Attributes = MemberAttributes .Overloaded;

         targetMethod .Comments .Add ( new CodeCommentStatement ( CoderBase .LineReplacementFlag + visibility ) );

      }




      #region JGWill Extension 
      protected void AddComments ( CodeTypeMember code, string key, string value )
        => AddComments ( code,
                value, "KeyValueTest::" + key );

      protected void AddComments ( CodeTypeMember code, string textComment )
     => code
              .Comments
              .Add ( new CodeCommentStatement ( textComment ) );

      // /// <summary>
      // /// Add comments to test KV Pairs
      // /// </summary>
      // /// <param name="code"></param>
      // /// <param name="key"></param>
      // /// <param name="value"></param>
      // protected void AddComments(CodeTypeDeclaration code, string key, string value)
      //=>    AddComments(code,
      //         value, "KeyValueTest::" + key);


      // /// <summary>
      // /// add comments
      // /// </summary>
      // /// <param name="code"></param>
      // /// <param name="textComment"></param>
      // protected void AddComments(CodeTypeDeclaration code, string textComment)
      // => code
      //     .Comments
      //     .Add( new CodeCommentStatement(textComment));

      #endregion


   }
}
