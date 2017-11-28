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
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.IO;

    public partial class StateBuilder
    {
        protected static TraceSource ts = new TraceSource("StateBuilder");
        public string InputFileName { get; set; }
        public string OutputDirectory { get; set; }
        private string outputFileName;
        public string OutputFileName
        {
            get
            {
                if (String.IsNullOrEmpty(this.outputFileName) == true)
                {
                    this.outputFileName =
                        OutputDirectory +
                        Path.DirectorySeparatorChar +
                        Path.GetFileNameWithoutExtension(InputFileName) + "Fsm." +
                        this.codeDomProvider.FileExtension;

                    //ext
                    OnOutputFileNameInitialized(outputFileName, OutputDirectory);
                }
                return this.outputFileName;
            }
            set
            {
                this.outputFileName = value;
            }
        }
        partial void OnOutputFileNameInitialized(string outputFileName, string outputDirectory);

        private CodeDomProvider codeDomProvider;
        public StateBuilderOptions Options { get; set; }

        private StateMachineXmlModel xmlModel;
        private StateMachineType model;
        private CoderStateMachine coder;

        public StateBuilder(string inputFilename)
        {
            InputFileName = inputFilename;
            OutputDirectory = Path.GetDirectoryName(inputFilename);
            Options = new StateBuilderOptions();
            this.codeDomProvider = CreateDomProvider();

            //ext
            OnCreated(inputFilename);
        }
        partial void OnCreated(string inputFilename);

        partial void OnModelRead(StateMachineType model);
        public void Build()
        {
            //ext
            OnBuildStarting();

            xmlModel = new StateMachineXmlModel(InputFileName);
            model = xmlModel.Build();

            //ext
            OnModelRead(model);

            StreamWriter streamWriter = new StreamWriter(OutputFileName);

            CodeCompileUnit code = new CodeCompileUnit();
            CodeNamespace codeNamespace = CreateNameSpace(code);

            WriteHeader(codeNamespace);

            coder = new CoderStateMachine(model, Options, codeNamespace);
            coder.WriteCode();



            OnBuildWriteCompleted(coder, codeNamespace, code, model);

            codeDomProvider.GenerateCodeFromNamespace(codeNamespace, streamWriter, null);

            streamWriter.Close();

            OnBuildCompleted(coder, codeNamespace, code, model,OutputFileName,OutputDirectory);


        }

        partial void OnBuildStarting();
        partial void OnBuildCompleted(CoderStateMachine coder, CodeNamespace codeNamespace, CodeCompileUnit code, StateMachineType model,string outputFileName,string outputDirectory);

        partial void OnBuildWriteCompleted(CoderStateMachine coder, CodeNamespace codeNamespace, CodeCompileUnit code, StateMachineType model);



        /// <summary>
        /// Create the CSharp or VisualBasic CodeDomProvider
        /// </summary>
        /// <returns></returns>
        private CodeDomProvider CreateDomProvider()
        {
            string language = "CSharp";
            if (InputFileName.Contains(".fsmvb"))
            {
                Options.TargetLanguage = StateBuilderOptions.Language.VB;
                language = "VisualBasic";
            }
            return CodeDomProvider.CreateProvider(language);
        }

        /// <summary>
        /// Create the namespace given in settings/namespace
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private CodeNamespace CreateNameSpace(CodeCompileUnit code)
        {
            CodeNamespace codeNamespace = new CodeNamespace(model.settings.@namespace);
            code.Namespaces.Add(codeNamespace);
            return codeNamespace;
        }

        /// <summary>
        /// Write the header composed of the prepend file, the product info and the imports.
        /// </summary>
        /// <param name="codeNamespace"></param>
        private void WriteHeader(CodeNamespace codeNamespace)
        {
            //ext
            OnWriteHeaderStarted(codeNamespace);

            WritePrependFile(codeNamespace);
            WriteStateBuilderInfo(codeNamespace);
            WriteImport(codeNamespace);

            //ext
            OnWriteHeaderCompleted(codeNamespace);
        }
        partial void OnWriteHeaderStarted(CodeNamespace codeNamespace);
        partial void OnWriteHeaderCompleted(CodeNamespace codeNamespace);

        private void WriteFooter()
        {
        }

        /// <summary>
        /// Write the import:
        /// using System;
        /// using System.Threading;
        /// using StateForge.StateMachine
        /// </summary>
        /// <param name="codeNamespace"></param>
        private void WriteImport(CodeNamespace codeNamespace)
        {
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            if (this.model.settings.asynchronous == true)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport("System.Threading"));
            }

            codeNamespace.Imports.Add(new CodeNamespaceImport("StateForge.StateMachine"));
            if (this.model.settings.@using != null)
            {
                foreach (string assembly in this.model.settings.@using)
                {
                    codeNamespace.Imports.Add(new CodeNamespaceImport(assembly));
                }
                    codeNamespace.Imports.Add(new CodeNamespaceImport("PS"));
            }

            foreach (ObjectType objectType in this.model.settings.@object)
            {
                if (objectType.@namespace != null)
                {
                    codeNamespace.Imports.Add(new CodeNamespaceImport(objectType.@namespace));
                }
            }

            //ext
            OnWriteImportCompleted(codeNamespace.Imports);
        }

        partial void OnWriteImportCompleted(CodeNamespaceImportCollection _importsCollection);

        /// <summary>
        /// Prepend a file to the generated code.
        /// </summary>
        /// <param name="codeNamespace"></param>
        private void WritePrependFile(CodeNamespace codeNamespace)
        {
            string prependFileName = Options.PrependFile;
            if (prependFileName == null)
            {
                return;
            }

            string prependFileContent = File.ReadAllText(prependFileName);
            if (string.IsNullOrEmpty(prependFileContent) == false)
            {
                codeNamespace.Comments.Add(new CodeCommentStatement(prependFileContent));
                codeNamespace.Comments.Add(new CodeCommentStatement(""));
            }
        }

        /// <summary>
        /// Write some info such as the product name, company, date, version and input file name.
        /// </summary>
        /// <param name="codeNamespace"></param>
        private void WriteStateBuilderInfo(CodeNamespace codeNamespace)
        {
            var assemblyInfo = new AssemblyInfo<StateBuilder>();
            codeNamespace.Comments.Add(new CodeCommentStatement("This file has generated automatically by " + assemblyInfo.Product + " version " + assemblyInfo.Version));
            codeNamespace.Comments.Add(new CodeCommentStatement(assemblyInfo.Product + " has been brought to you by " + assemblyInfo.Company));
            codeNamespace.Comments.Add(new CodeCommentStatement("Date: " + Convert.ToString(System.DateTime.Now)));
            codeNamespace.Comments.Add(new CodeCommentStatement("Input: " + InputFileName));
            codeNamespace.Comments.Add(new CodeCommentStatement("Command line: " + Environment.CommandLine));
        }
    }
}
