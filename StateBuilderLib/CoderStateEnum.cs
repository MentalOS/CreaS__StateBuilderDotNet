#region Copyright
//------------------------------------------------------------------------------
// <copyright file="CoderState.cs" company="StateForge">
//      Copyright (c) 2010 StateForge.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
#endregion

namespace StateForge
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.IO;

   public enum MethodVisibility
   {
 @public,
@private,
@internal,
@protected,
none
   }
    public static class StateStore
    {
        public static string Namespace { get; set; }
        public static string PropertyCodeOut { get; set; }
        public static string PropertyType { get; set; }
        public static List<string> States { get; set; }
        public static List<StateType> StatesType { get; set; }

      public static string onPreEventPrefix = "OnPre";
      public static string onPostEventPrefix = "OnPost";

      public static string ModelName
        {
            get
            {
                //if (StatesType != null)
                //    return StatesType[StatesType.Count - 1].GetTopParent();
                //return "unnamed";
                return StateBuilder.ModelStatic.settings.name;
            }
        }

        public static string ParentStateName
        {
            get
            {             

                return StateBuilder.ModelStatic.state.name;
            }
        }


///<summary> the StateEnum Type name</summary>
        public static string EnumTypeName
            => ModelName + stateEnumSuffix;            


///<summary>default suffix of the StateEnum file/enum declaration</summary>
        public const string stateEnumSuffix = "StateEnum";

///<summary>Name of the State property in the feeder</summary>
        public const string stateEnumFeederPropertyName = "State";

///<summary>default state in which an object is created</summary>
        public const string defaultEntryStateEnum = "Instanciated";


		///<summary>get the State Enum Hierarchy</summary>
        public static string GetStateEnumHierarchyString(StateType state)
        {
   string stateHierarchy = state.ToParentHierarchy()
                                .Replace(
                                ParentStateName +"_","")
                                ;


            return stateHierarchy;
        }
        // public class StateRoot : State<HelloWorldContext, StateRoot>
        public static void WriteClass()
        {
            string enumName = EnumTypeName;

            PropertyType = enumName;

         string writeLineStateChangesValue = "false";
         string writelinestatechangesFieldName = "writelinestatechanges";
         if (StateStore.Model.settings.writelinestatechanges)
writeLineStateChangesValue = "true";


         PropertyCodeOut = "public  " + enumName + " " + StateStore.stateEnumFeederPropertyName + @"
{ 
get  {
return _state;  
}
set  {
if ("+writelinestatechangesFieldName+@") Console.WriteLine(" + "\"Transition from State: \"+_state + \" to \" + value);\n"
        + @"  _state = value;    
}
}";

         PropertyCodeOut += "\n" + $"protected {enumName} _state;\n";

         PropertyCodeOut += "\n" + $"protected bool {writelinestatechangesFieldName} ={writeLineStateChangesValue} ;\n";
          
         /*      // StateVarTarget
     BusServiceSyncStateEnum _state = BusServiceSyncStateEnum.Instanciated;
     bool writeLineStateChanges;
     public BusServiceSyncStateEnum State
     {
        get
        {
           return _state;
        }

        set
        {
           if (writeLineStateChanges) Console.WriteLine("State from {0} to {1} ", _state.ToString(), value.ToString());
           _state = value;

        }
     }
     */
         //writeLineStateChanges = true; //within the context and as prop in the feederCode

         string fileOut2 = enumName + ".cs";

            if (States != null)
            {
                if (StatesType != null)
            {
               string oState2 = "";
               oState2 += "//Generated at: " + DateTime .Now .ToString ( ) + "\n\n";


               oState2 += "namespace " + Namespace;

               oState2 += "{ \n";
               oState2 += "//" + ModelName;

               oState2 += "\n";
               oState2 += "public enum " + enumName + " { \n";

               int c = 1;


               int max = StatesType .Count;
               stateIds = new List<int> ( );
               StateIds = new Dictionary<string, int> ( );

               int pLevel = -1;
               int m = 0;
               int pid = 0;

               foreach (var state in StateStore .StatesType)
               {

                  //adding a default Instanciated when entering first state
                  if (state .name == ParentStateName)
                  {
                     oState2 += "\t";
                     oState2 += defaultEntryStateEnum + " = 0,\n";
                  }

                  //other state
                  if (state .name != ModelName
                      && state .name != ParentStateName)
                  {

                     oState2 += "\t";

                     //get the current State Hierarchy
                     string stateHierarchy = GetStateEnumHierarchyString ( state );

                     //string stateHierarchy = state.ToParentHierarchy()
                     //    .Replace(
                     //    ParentStateName +"_","");

                     oState2 += stateHierarchy;

                     level = GetLevel ( stateHierarchy );



                     //Setting the ID
                     /*I want to start at level 0-100 for state at base
                     //a sub state of state 1 would be
                      101 
                     a substate of 10001
                     10101
                      I am not so sure...
                      I want state to keep their id even if oother add on and be relatable to their parent state...
                     */
                     // if (level > 0 & level != pLevel)
                     //     c++;
                     //else if (level < pLevel)
                     //     m--;
                     // if (level == 0)
                     //     m = 0;

                     // int id = c;
                     // if (level >0 )
                     //     id = c * 100 + (m * level);

                     int id = c;

                     //TODO Increment levels and have an ID related to the parent

                     //if (level > pLevel)
                     //    id = pid * level * 100;


                     //while (StateIds.ContainsKey(id))
                     //{
                     //    id++;
                     //}
                     //if (state.Parent != null)
                     //   if (StateIds.ContainsKey(state.Parent.ToParentHierarchy()))
                     //    id = StateIds[state.name] * 100;
                     //if (!StateIds.ContainsKey(stateHierarchy))
                     //StateIds.Add(stateHierarchy,id);



                     oState2 += "=" + id;
                     if (state .name != "End") // add a coma
                        oState2 += ",";

                     oState2 += "//level:" + level + " id:" + id;
                     oState2 += "\n";

                     pLevel = level;
                     pid = id;
                     c++;
                  }
               }
               oState2 += "\t}\n";
               oState2 += "}";

               //ModelName = ModelName.Replace("EndState", ""); 
               oState2 += "\n";

               writeStateEnumFile ( fileOut2, oState2 );
            }
         }


        }

      private static void writeStateEnumFile ( string fileOut2, string oState2 )
      {
         File .WriteAllText ( fileOut2, oState2 );
      }

      private static int GetLevel(string l)
       => l.Split(separator).Length - 1;

        public static string GetTopParent(this StateType state)
        {
            string n = "";
            if (state.Parent != null)
            {
                n = state.Parent.ToParentHierarchy();
            }

            return n;
        }
        static List<int> stateIds;
        public static Dictionary<string, int> StateIds { get; set; }


      //the state machine model
      public static StateMachineType Model => StateBuilder.ModelStatic;

      public static char separator = '_';

///<summary>get the current State Hierarchy</summary>
        public static string ToParentHierarchy(this StateType state, int level = 0)
        {
            string o = "";
            string n = "";

            if (state.Parent != null)
                if (state.Parent.name != ModelName)
                {

                    n = state.Parent.ToParentHierarchy(++level);
                    o += n;
                    o += separator;
                }
            StateStore.level = level;
            o += state.name;

            return o 
            .Replace ( "___", "" ); //removes parent state __ used in simple state that needs common event
        }

        static int level = 0;
        public static string WithEnter(this string my)
        { return my + "\n"; }
        public static string WithUnderscore(this string my)
        { return my + "_"; }
    }

}
