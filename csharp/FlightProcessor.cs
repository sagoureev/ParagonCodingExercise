using System;
using System.Collections.Generic;
using System.Text;

//These are the system imports i added
using System.Collections.Concurrent;
using System.Threading;

//We will import classes from the current project here
using ParagonCodingExercise.Airports;
using ParagonCodingExercise.Events;

namespace ParagonCodingExercise
{
    /// <summary>
    /// This is the "Main" program class that runs everything. In the future I would like this to have global settings nd possibly command line processing
    /// </summary>
    public class FlightProcessor
    {
        //If this variable is used for debugging. Set it to a valid Airplane Identifier to only get output data 
        private readonly string DEBUG_FLIGHT;

        //Location of Airports JSON
        private readonly string AirportsFilePath = @".\Resources\airports.json";

        // Location of ADS-B events
        private readonly string AdsbEventsFilePath = @".\Resources\events.txt";

        // Write generated flights here
        private static string OutputFilePath = @".\Resources\flights.csv";

        //Instantiate once, pass it to all the classes that need it
        private readonly AirportCollection airports;

        //Todo: Implement something more than "file", it's file now
        private readonly string ProcessingType;

        //our trusty thread safe queues will be instantiated here
        // One for events, one for output flights
        private ConcurrentQueue<AdsbEvent> EventsQueue;
        private ConcurrentQueue<Flight> FlightsQueue;

        //The master class that holds all our individual airplane information will also be instantialized here
        private EventProcessor MainEventProcessor;

        /// <summary>
        /// Master constructor for the "top level" of our application
        /// TODO: create a factory method if initialization becomes complicated. Not needed now
        /// </summary>
        public FlightProcessor()
        {
            airports = AirportCollection.LoadFromFile(AirportsFilePath);
            ProcessingType = "file";
            EventsQueue = new ConcurrentQueue<AdsbEvent>();
            FlightsQueue = new ConcurrentQueue<Flight>();
            MainEventProcessor = new EventProcessor(FlightsQueue, airports);
        }

        /// <summary>
        /// Master constructor is used for unit tests. Clean it up later
        /// </summary>
        public FlightProcessor(string airportFile, string eventFile, string debugAirplane)
        {
            airports = AirportCollection.LoadFromFile(airportFile);
            ProcessingType = "file";
            EventsQueue = new ConcurrentQueue<AdsbEvent>();
            FlightsQueue = new ConcurrentQueue<Flight>();
            MainEventProcessor = new EventProcessor(FlightsQueue, airports);
            AdsbEventsFilePath = eventFile;
            DEBUG_FLIGHT = debugAirplane;
        }


        /// <summary>
        /// Runs the flight processor excecution. The actual runtime excecution is here.
        /// In its design its simple - first we load the events, then we process them, then we do "garbage collection"
        /// on planes still flying when we stop looking, then we output the results
        /// Todo: support more processing types
        /// </summary>
        public void Execute()
        {
            Console.WriteLine("Processing...");
            //This is the default processing type. Others are for future implemntation
            if(ProcessingType == "file")
            {
                FileReader fp = new FileReader(EventsQueue, AdsbEventsFilePath);
                fp.ParseFile();
                ProcessEvents();
                ProcessIncompleteEvents();
                ProcessResults();
            }

        }

        /// <summary>
        /// We have a collection of events, so we process them one by one
        /// This is also where we can debug with individual airplanes
        /// </summary>
        public void ProcessEvents() {
            AdsbEvent currentEvent;
            while (EventsQueue.TryDequeue(out currentEvent))
            {
                //We have an event to process. 
                // During development we used the following planes to test
                // Gulfstream Aerospace G-IV -  AA09F4 -- small private jet
                // A44D3F - Delta commercial jet
                // A30626 airplane - UPS cargo plane  pass
                if (DEBUG_FLIGHT != null && currentEvent.Identifier == DEBUG_FLIGHT) {
                    MainEventProcessor.ProcessAdsbEvent(currentEvent);
                }
                else if(DEBUG_FLIGHT == null)
                {
                    MainEventProcessor.ProcessAdsbEvent(currentEvent);
                }
                

            }
        }


        /// <summary>
        /// This is the "garbage collection" routine called to process flights that have taken off and have not landed
        /// </summary>
        public void ProcessIncompleteEvents()
        {
            MainEventProcessor.ProcessIncompleteEvents();
        }

        private void ProcessResults()
        {
            if (ProcessingType == "file")
            {
                FileOut fo = new FileOut(FlightsQueue, OutputFilePath);
                fo.WriteToFile();
            }
        }

        ///////////////////////////////// NOTE //////////////////////////////////////
        ///these are helper functions for unit tests. Getters usualy go higher, but these are "special" getters and i wanted to group them accordingly
        ///This is also why I gave them a different naming convention
        public ConcurrentQueue<AdsbEvent> getEventQueue()
        {
            return EventsQueue;
        }

        public string getAdsbEventsFilePath()
        {
            return AdsbEventsFilePath;
        }

        public ConcurrentQueue<Flight> getFlightQueue()
        {
            return FlightsQueue;
        }
    }
}
