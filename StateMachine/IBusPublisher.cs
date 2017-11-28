using System;
using System .Collections .Generic;

using System .Text;

namespace StateForge .StateMachine
{
   /// <summary>
   /// Interface to the bus publisher
   /// </summary>
  public interface IBusPublisher
   {
      /// <summary>
      /// Interface to bus publisher
      /// </summary>
      /// <typeparam name="TMessage"></typeparam>
      /// <param name="t"></param>
    void PublishBusMessage<TMessage>(TMessage t);
   }
}

//-------------------------------------------
/*   SKETCHES for the BUS Publisher
 //IN the Context 

  IBusPublisher __busPub= this .InstrumentPerspective__InstrumentPerspective .BusPublisher;
          if ( __busPub != null) //bus publish
           __busPub .Publish<CreateMarketOverViewerNodeCompletedEventArgs > ( e );



 //IN THE FEEDER
      public IBusPublisher BusPublisher
      {
         get
         {
            if (busPublisher != null)
               return busPublisher;
            return null;
         }
         internal set
         {
            busPublisher = value;
         }
      }
      protected IBusPublisher busPublisher;


   //todo In Init of implemented manually class
   ctor ()
   {
   busPublisher = this; //or any other object
   }
    */
