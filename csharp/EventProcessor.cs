using System;
using System.Collections.Generic;
using System.Text;


using System.Collections.Concurrent;

//We will import classes from the current project here
using ParagonCodingExercise.Events;
using ParagonCodingExercise.Airports;

namespace ParagonCodingExercise
{

    /// <summary>
    /// This class is responsible for processing an event from the input and delegating it to individual per airplane processes
    /// </summary>
    public class EventProcessor
    {
        /// <summary>
        /// Data structure for storing individual airplane processes
        /// TODO: use thread safe dictionary instead. This deserves its own collection class
        /// </summary>
        private Dictionary<string, AirplaneEventProcessor> IndividualPlaneProcessor;

        private ConcurrentQueue<Flight> FlightsQueue;

        private readonly AirportCollection Airports;

        public EventProcessor(ConcurrentQueue<Flight> outputQueue, AirportCollection airportDB)
        {
            IndividualPlaneProcessor = new Dictionary<string, AirplaneEventProcessor>();
            FlightsQueue = outputQueue;
            Airports = airportDB;
        }

        /// <summary>
        /// This function takes an event and assigns it to the correct indivudual airplane module for processing.
        /// It also creates a new instance of an individual airplane event processor if needed
        /// </summary>
        public void ProcessAdsbEvent(AdsbEvent currentEvent)
        {
            AirplaneEventProcessor currentProcessor = null;
            if (IndividualPlaneProcessor.ContainsKey(currentEvent.Identifier))
            {
                currentProcessor = IndividualPlaneProcessor[currentEvent.Identifier];
            } else
            {
                currentProcessor = new AirplaneEventProcessor(new Airplanes.IndividualAirplane(currentEvent.Identifier), FlightsQueue, Airports);
                IndividualPlaneProcessor[currentEvent.Identifier] = currentProcessor;
            }
            currentProcessor.ProcessEventForThisPlane(currentEvent);

        }

        /// <summary>
        /// Cleanup function that pulls all data from airplanes that are currently flying and adds them to the flight queue with an Unknown destination
        /// TODO: This might have to be rewritten if other input/output methods are used
        /// </summary>
        public void ProcessIncompleteEvents()
        {
            foreach (KeyValuePair<string, AirplaneEventProcessor> processor in IndividualPlaneProcessor)
            {
                processor.Value.AddUnknownFlight();
            }
        }
    }


    
}
