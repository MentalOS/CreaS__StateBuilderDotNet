using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS.Common.SCDE.CodeGeneratorLib
{
    public static class CGExt
    {

        public static CodeSnippetExpression newSnippet(string code)
        => new CodeSnippetExpression(code);

    }

    public static class CGExtMethod
    {


    }

    /// <summary>
    /// Create my Own code extention to the CodeMemberProperty 
    /// it Enable to Add the Getter and Setter to Parse an Enum from that  
    /// nt property and set the value of another one that is into the model.
    /// </summary>
    public static class CGExtProperties
    {




        /// <summary>
        /// Add a Return for the Property and sets his type of.
        /// </summary>
        /// <param name="codeProperty"></param>
        /// <param name="type"></param>
        /// <param name="typeVariableSource"></param>
        /// <param name="goalComments"></param>
        /// <param name="targetClass"></param>
        public static void AddReturn
                   (this CodeMemberProperty codeProperty,
            Type type,
            string typeVariableSource,
            string goalComments = "",
            CodeTypeDeclaration targetClass = null)
        {
            codeProperty.HasGet = true;

            var refField = fCreateFieldRef(typeVariableSource);

            var _propReturnStatement
                = mCreateReturnStatement(refField);

            //add comment if some specified
            if (goalComments != "")
                codeProperty.GetStatements.Add(new CodeCommentStatement(goalComments));

            codeProperty.GetStatements.Add(_propReturnStatement);
            codeProperty.Type = new CodeTypeReference(type);




            if (targetClass != null) targetClass.Members.Add(codeProperty);
        }

        /// <summary>
        ///  Add a ParseEnum Setter Value From Int
        /// </summary>
        /// <param name="codeProperty"></param>
        /// <param name="instanceTarget"></param>
        /// <param name="targetEnumType"></param>
        /// <param name="sourceInt2Cast"></param>
        /// <param name="propName"></param>
        /// <param name="goalComments"></param>
        /// <param name="targetClass"></param>
        public static void SetAddParseEnumSetterValueFromInt
                   (this CodeMemberProperty codeProperty,
                   string instanceTarget,
                   string targetEnumType,
                   string sourceInt2Cast, string propName = "", string goalComments = "", CodeTypeDeclaration targetClass = null)
        {
            if (propName != "")
                codeProperty.Name = propName;
            codeProperty.Attributes = MemberAttributes.Public;


            var x
                = newEnumParseSnippet(instanceTarget,
                     targetEnumType,
                     sourceInt2Cast);


            //add comment if some specified
            if (goalComments != "") codeProperty.SetStatements.Add(new CodeCommentStatement(goalComments));


            var setSet = newSnippet($"{sourceInt2Cast} = value");


            codeProperty.HasSet = true;
            codeProperty.SetStatements.Add(setSet);
            codeProperty.SetStatements.Add(x);


            if (targetClass != null) targetClass.Members.Add(codeProperty);
        }

        public static CodeSnippetExpression newEnumParseSnippet(string instanceTarget,
                   string targetEnumType,
                   string sourceInt2Cast)
       =>
            newSnippet($"{ instanceTarget}=({targetEnumType})Enum.Parse(typeof({ targetEnumType}),{sourceInt2Cast})");


        ///// <summary>
        ///// Used to add to a setter an enum parsed from an Int.
        ///// </summary>
        ///// <param name="codeProperty"></param>
        ///// <param name="instanceTarget"></param>
        ///// <param name="targetEnumType"></param>
        ///// <param name="sourceInt2Cast"></param>
        //public static void SetAddParseEnumSetterValueFromInt
        //    (this CodeMemberProperty codeProperty,
        //    string instanceTarget,
        //    string targetEnumType,
        //    string sourceInt2Cast)
        //{


        //}





        public static CodeSnippetExpression newSnippet(string code)
        => new CodeSnippetExpression(code);


        public static CodeMethodReturnStatement mCreateReturnStatement(CodeExpression express)
                   => new CodeMethodReturnStatement(express);

        public static CodeFieldReferenceExpression fCreateFieldRef(string name)
        {
            var _this = new CodeThisReferenceExpression();
            var _refStateProperty
                = new CodeFieldReferenceExpression(_this, name);
            return _refStateProperty;
        }
    }

}
