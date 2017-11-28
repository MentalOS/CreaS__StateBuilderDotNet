//Feels now a better Idea to do it from the feeder...


using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StateForge
{
    /// <summary>
    /// Generate Event Args class once when event handler/delegate are created for feeder
    /// 
    /// I want : 
    ///     Each EVENTEventArgs class to be created within a subdirectory  ./EventArguments
    ///     If it already exist it does not recreate it
    ///     it might derived from an EventARgsBaseClass that is distributed into StateMachine.dll
    ///     
    /// OR
    ///     1 file with All EventsArgs and you just rename it...
    /// </summary>
    public class CoderEventsArgs : CoderBase
    {
        public CoderEventsArgs(StateMachineType model, StateBuilderOptions options, CodeNamespace codeNamespace)
        : base(model, options, codeNamespace)
        {
            _model = model;
        }




        private CodeDomProvider codeDomProvider;
        //public StateBuilderOptions Options { get; set; }

        //private StateMachineXmlModel xmlModel;
        private StateMachineType _model;
        //private CoderStateMachine coder;
        // private CodeNamespace codeNamespace;



        #region Copied code not used yet
        public void Build()
        {

          

        }


#endregion


        /// <summary>
        /// The Idea of this class is to write once files arguments of the events
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="nameSpace"></param>
        internal void WriteEventArgsCode(EventType evt, string nameSpace)
        {

            string id = evt.id;
            string eid = evt.id + "Event";
            string eidh = eid + "Handler";
            string eidArgs = eid + "Args";



            //would write the header of the event file
            WriteHeader(CodeNamespace);


            //would write the event class file code
            WriteEventCode(CodeNamespace, eidArgs);


            string OutputFileName = eidArgs;
            Console.Write("Writting EventARgs: " + evt.id);
            //if (!File.Exists(OutputFileName))
            //{
            Console.WriteLine("....");

            //StreamWriter streamWriter = new StreamWriter(OutputFileName);

            ////  AddProperties(feederCode);

            //codeDomProvider.GenerateCodeFromNamespace(CodeNamespace, streamWriter, null);

            //streamWriter.Close();

            //}
            //else Console.WriteLine("EventARgs file for : " + eidArgs + " EXIST Already");
        }

        private void WriteEventCode(CodeNamespace codeNamespace, string eidArgs)
        {
            CodeTypeDeclaration eventARgsCode = new CodeTypeDeclaration(eidArgs);
            codeNamespace.Types.Add(eventARgsCode);

            var t = new CodeCommentStatement("EVent ARgument for event " + eidArgs);

            eventARgsCode.Comments.Add(t);
            // doc
            eventARgsCode.Comments.Add(new CodeCommentStatement(eidArgs));

            // public
            eventARgsCode.Attributes = MemberAttributes.Public;

            // partial
            eventARgsCode.IsPartial = true;


            const string eventTypeBase = "SMEventArgsBase";
            eventARgsCode.BaseTypes.Add(new CodeTypeReference(typeof(EventArgs)));


        }

        private void WriteHeader(CodeNamespace codeNamespace)
        {
            //WritePrependFile(codeNamespace);
            //WriteStateBuilderInfo(codeNamespace);
            //WriteImport(codeNamespace);
        }

        /// <summary>
        /// Create the namespace given in settings/namespace
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private CodeNamespace CreateNameSpace(CodeCompileUnit code, string nameSpaceName = "")
        {
            CodeNamespace codeNamespace = null;

            if (nameSpaceName == "")
                codeNamespace = new CodeNamespace(_model.settings.@namespace);
            else codeNamespace = new CodeNamespace(nameSpaceName);

            code.Namespaces.Add(codeNamespace);
            return codeNamespace;
        }

        public override void WriteCode()
        {
            throw new NotImplementedException();
        }
    }
}
