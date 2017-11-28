#region Copyright
//------------------------------------------------------------------------------
// <copyright file="StateBuilderOptions.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

namespace StateForge
{
   using System;

   public partial class StateBuilderOptions //accessible thru CoderBase
   {

      /// <summary>
      /// default suffix to add for Event Message  (default is : "EventArgs"
      /// </summary>
      public string EventMessageSuffix
      {
         get
         { return eventMessageSuffixDefault; }
         set
         { eventMessageSuffixDefault = value; }
      }

      private string eventMessageSuffixDefault = "EventArgs";
   }
}
