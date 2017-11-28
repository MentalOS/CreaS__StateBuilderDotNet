#region Copyright
//------------------------------------------------------------------------------
// <copyright file="StateBuilder.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

namespace StateForge
{
   using System;
   using System .CodeDom;
   using System .CodeDom .Compiler;
   using System .Diagnostics;
   using System .IO;

   public static class SBExt
   {
      public static string ToSaveName ( this string s )
      => s .Replace ( "<", "" ) .Replace ( ">", "" );

   }
   public partial class StateBuilder
   {

      public static StateMachineType ModelStatic { get; internal set; }
      public StateMachineType Model => model;

      partial void OnModelRead ( StateMachineType model )
      {
         ModelStatic = model;
      }
      partial void OnCreated ( string inputFilename )
      {


      }
      partial void OnBuildCompleted ( CoderStateMachine coder, CodeNamespace codeNamespace, CodeCompileUnit code, StateMachineType model, string outputFileName, string outputDirectory )
      {

         //Hack to make Partial method
         postPatchPartialDeclarationOnFileOut ( );

         CodeChanger cc = new CodeChanger ( );
         cc .ChangeCode ( outputFileName, outputFileName .Replace ( ".cs", "_.cs" ) );
      }
      /*CodeChanger cc = new CodeChanger();
cc.ChangeCode(@"D:\1.cs", @"D:\3.cs"); */

      partial void OnWriteHeaderCompleted ( CodeNamespace codeNamespace )
      {
         codeNamespace .Comments .Add ( new CodeCommentStatement ( "-----------------------" ) );
         codeNamespace .Comments .Add ( new CodeCommentStatement ( "CreAs Extension" ) );
         codeNamespace .Comments .Add ( new CodeCommentStatement ( "By J.Guillaume D-Isabelle" ) );
         codeNamespace .Comments .Add ( new CodeCommentStatement ( "------2017-09-------" ) );
         codeNamespace .Comments .Add ( new CodeCommentStatement ( "-----------------------" ) );
      }

      #region JGWIll extension
      private void postPatchPartialDeclarationOnFileOut ( )
      {
         string[] fileContent = File .ReadAllLines ( OutputFileName );
         string fixedContent = "//Post Fixed content\n\n";

         //used to know if last line was a partial so we wipe the }
         bool hadPartial = false;
         foreach (var l in fileContent)
         {
            bool processed = false;

            #region Patch feeder code
            string n = l;
            string c = CoderBase .LineReplacementFlag;
            #region Prep State Extention partial
            //todo remove // and @ from the lines
            if (l .Contains ( c ))
            {
               n = l .Replace ( c, "" ) .Replace ( "//", " " );
               fixedContent += n + "\n";
               processed = true;
            }
            #endregion
            else
            {

               if (hadPartial)
               {
                  n = l .Replace ( '}', ' ' ) .Replace ( '{', ';' );
                  hadPartial = false;
               }
               else
               //todo find better way to express that
               if ((l .Contains ( "partial void" ) || hadPartial))
               {
                  n = l .Replace ( '}', ' ' ) .Replace ( '{', ';' );
                  // if (l.Length < 3)
                  hadPartial = true;
               }
               fixedContent += n + "\n";

            }
            // if (l.Replace("\t","").Replace(" ","").Length > 2) hadPartial = false;

            //dont know why my replacement dont work...this fix it
            if (l .Contains ( "// partial void" ) && !processed)
            {
               n = l .Replace ( "// ", "" ) .Replace ( "//", "" );


               fixedContent += n + "\n";
               processed = true;
            }

            if (l .Contains ( "//partial void" ) && !processed)
            {
               n = l .Replace ( "//", "" ) .Replace ( "// ", "" );


               string endLine = "\n";


               fixedContent += n + endLine;


               processed = true;
            }


            //STATE Variable
            if (l .Contains ( CoderFeeder .StateVarTarget ))
            {
               if (StateStore .PropertyCodeOut != null)
                  fixedContent += "\n\t" + StateStore .PropertyCodeOut + "\n";
            }
            #endregion



         }

         string[] linesFixTab = fixedContent .Split ( '\n' );
         fixedContent = "//SECOND PASS FIX\n\n";
         int level = 0;
         bool wasHacked = false;
         foreach (string l in linesFixTab)
         {
            //todo create level and add tabs to format the doc
            string n = l .TrimStart ( ' ' ) .TrimStart ( '\t' );
            string p = "";

            for (int i = 0 ; i < level + 1 ; i++)
            {

               p += "\t";
            }

            if (wasHacked) p = " ";

            if (!n .Contains ( "}" )) p += "\t"; // one more indend if not the end or starts


            // if ((n.Contains("public") || n.Contains("private") || n.Contains("protected")) && 
            if (n .Contains ( "{" ))//level up
               level++;
            if (n .Contains ( "}" )) //level up
               level--;

            wasHacked = false;

            if (!n .Contains ( "//partial void" )) //ignore duplicate lines I could not get rid of before

            {
               if (n .Contains ( CoderBase .MethodVisibilityHackReplacementFlag ))
               {
                  //todo 
                  //clean: [HACK__public()]
                  n = n .Replace ( "[", "" ) .Replace ( "]", "" ) .Replace ( "(", "" ) .Replace ( ")", "" )
                     .Replace ( CoderBase .MethodVisibilityHackReplacementFlag, "" );
               }


               string endLine = "\n";

               #region fixes hack for method visibility

               int nl = n .Trim ( ) .Length;
               string e = "";

               e = "internal";
               int el = e .Length;
               if (n .Contains ( e )
                  && nl == el)
               {
                  endLine = "";
                  wasHacked = true;
               }

               e = "protected";
               el = e .Length;
               if (n .Contains ( e )
                  && nl == el)
               {
                  endLine = "";
                  wasHacked = true;

               }
               #endregion


               #region Remove typeof used in event that are thought as being specific such as string are replaced by @string
               string __repl = "string";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

               __repl = "int";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

  __repl = "double";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

  __repl = "float";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

  __repl = "bool";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

  __repl = "Guid";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

  __repl = "List";
               if (n .Contains ( "@" + __repl ))
                  n = n .Replace ( "@" + __repl, __repl );

               #endregion






               if (wasHacked) n = n .Trim ( );
               fixedContent += p + n + endLine;
            }
         }

         File .WriteAllText ( OutputFileName, fixedContent );
      }

      #endregion


   }
}
