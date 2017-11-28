using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Xml.Serialization;
namespace StateForge
{

   //added constructor to the generated Schema to preset value to false

   public partial class SettingsType
   {
      public SettingsType()
      {
         buspublisherenabled = false;
         buspublisherenabledField = false;

         //default observed by console
         observerconsole = true;

         //default observer
         observertarget = "ObserverConsole.Instance";
         writelinestatechangesSpecified = false;
         writelinestatechanges = true;
      }
   }


   //----------------------
   public partial class EventType
   {
      public EventType()
      {
         buspublish = false;
         buspublishField = false;

         visibility = "public";
      }

   }

   public partial class EventSourceType
   {
      public EventSourceType()
      {
         buspublishFieldSpecified = false;
         buspublishField = false;
         buspublish = false;

         //feed the current event source using its name if : 
         //if (feededSpecified)
            if (feeded)
               feeder = this .name;
         
      }
   }
}