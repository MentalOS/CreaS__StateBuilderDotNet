#region Copyright
//------------------------------------------------------------------------------
// <copyright file="StateMachineXmlModel.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
//--------------
//-- CreAs Extension
//---------------------------------
#endregion

namespace StateForge
{
   using System;
   using System .Diagnostics;
   using System .IO;
   using System .Reflection;
   using System .Xml;
   using System .Xml .Schema;
   using System .Xml .Serialization;
   using System .Collections .Generic;
   using System .CodeDom .Compiler;
   using ICSharpCode .NRefactory;
   using ICSharpCode .NRefactory .Ast;
   using ICSharpCode .NRefactory .PrettyPrinter;

   partial class StateMachineXmlModel
   {
      partial void OnFilledDefaultSettings ( StateMachineType model, string inputFileName )
      {
         //todo parse such thing like event param shortcut so we dont have to do it at many places
         foreach (var eventSource in model .events)
         {
            foreach (var evt in eventSource .@event)
            {
               if (evt .parameter != null)
               {
                  foreach (ParameterType param in evt .parameter)
                  { 
                     #region param shortcut

               /*@param shortcut : 
                *          If you named the event args
                *          with the same name as the event
                    *          example:  
                    *          eventID: PhoneAnswered
                    *          event params: PhoneAnswered e
                    *      you will get "EventArgs" added in the code like
                    *      PhoneAnswered(object sender, PhoneAnsweredEventArgs e)
                */
                     //Deals with the shortcut of using the same event id as args
                     if (param .type == evt .id)
                     {
                        param .type = evt .id + CoderBase.OptionsStatic.EventMessageSuffix;
                     }


                     #endregion
                  }
               }

            }
         }
      }
   }
}
