using System;
using System.Collections.Generic;
using System.Text;


using System.Collections.Concurrent;

using ParagonCodingExercise.Airplanes;
using ParagonCodingExercise.Events;
using ParagonCodingExercise.Airports;

namespace ParagonCodingExercise
{
    /// <summary>
    /// This is the real meat of the solution.
    /// In this class we process ADS-B events for an individual flight
    /// The logic behind this file is more thoroughly described in the README
    /// Since we are dealing with real life data, we can make several assumptions on airplane behavior and act accordingly
    /// TODO: Way more test cases and unit tests are neede to truly call this complete
    /// </summary>
    public class AirplaneEventProcessor
    {
        /// <summary>
        /// This enum is used to signify the current status of an airplane.
        /// UNKNOWN - this is when we first get a readout of an airplanes data
        /// FLYING - its currently flying
        /// GROUNDED - its currently grounded
        /// </summary>
        enum AirplaneStatus : int
        {
            UNKNOWN = 0,
            FLYING = 1,
            GROUNDED = 2,
        }

        /* 
         * The following constants are used to smooth out edge cases and process events
         * They are in seconds and MPH, where applicable
         */

        // When has reached landing speed, we can usually ignore it for some time  
        private const double REASONABLE_GROUNDING_DELAY = 900;

        // When we take off, we can usually ignore takeoff for some time
        private const double REASONABLE_TAKEOFF_DELAY = 300;

        //This is to account for the average time it takes to land a plane once we dip below landing speed
        // Used to get better time readings and account for the A30626 UPS Kona flight
        private const double REASONABLE_LANDING_TIME = 300;

        // if we are below this speed we are sure to be taxiing. We will get the most accurate airport lookup here
        private const double REASONABLE_TAXI_SPEED = 20;


        /// <summary>
        /// This is the Indvidual airplane we get our information from. 
        /// </summary>
        public readonly IndividualAirplane AirplaneProcessed;

        /// <summary>
        /// WE make sure to record the last event and compare to the current one on several ocasions
        /// </summary>
        private AdsbEvent LastEvent { get; set; }

        /// <summary>
        /// Current aircraft status. Numerator AirplaneStatus is explained above
        /// </summary>
        private AirplaneStatus CurrentStatus { get; set; }

        /// <summary>
        /// The last airport the plane was grounded in. Used mostly for takeoff information
        /// </summary>
        private Airport LastAirport { get; set; }

        /// <summary>
        /// Last known geolocation 
        /// </summary>
        private GeoCoordinate LastKnownPosition { get; set; }

        /// <summary>
        /// Last time the flight was grounded. 
        /// </summary>
        private DateTime? LastGrounded { get; set; }

        /// <summary>
        /// Last takeoff time. 
        /// </summary>
        private DateTime? LastTakeoff { get; set; }


        // =The following two properties are no longer used, but theycould be useful in the future
        private double? LastDeviceSpeed { get; set; }
        private DateTime? LastDeviceSpeedTime { get; set; }


        /// <summary>
        /// Last flight that was grounded. This object referece is also in the flights queue
        /// This is used to adjust the landing coordinates for a more accurate landing area once the flight has landed
        /// </summary>
        private Flight LastGroundedFlight { get; set; }

        // Our output queue
        private ConcurrentQueue<Flight> FlightsQueue;

        //Our airports
        private readonly AirportCollection Airports;

        /// <summary>
        /// Cntsructor. sets all the appropriate variables
        /// </summary>
        public AirplaneEventProcessor(IndividualAirplane individualAirplane, ConcurrentQueue<Flight> outputQueue, AirportCollection airportDB)
        {
            AirplaneProcessed = individualAirplane;
            FlightsQueue = outputQueue;
            Airports = airportDB;
            LastEvent = null;
            CurrentStatus = AirplaneStatus.UNKNOWN;
            LastKnownPosition = null;
            LastGrounded = null;
            LastTakeoff = null;
            LastDeviceSpeed = null;
            LastDeviceSpeedTime = null;
        }


        /// <summary>
        /// Gets an individual event and delegates it based on the current plane staus
        /// </summary>
        public void ProcessEventForThisPlane(AdsbEvent currentEvent)
        {
            if (currentEvent.Latitude != null)
            {
                LastKnownPosition = new GeoCoordinate((double)currentEvent.Latitude, (double)currentEvent.Longitude);
            }
            switch (CurrentStatus)
            {
                case AirplaneStatus.UNKNOWN:
                    ProcessUnknown(currentEvent);
                    break;
                case AirplaneStatus.FLYING:
                    ProcessFlying(currentEvent);
                    break;
                case AirplaneStatus.GROUNDED:
                    ProcessGrounded(currentEvent);
                    break;
            }

            LastEvent = currentEvent;
            

        }


        /// <summary>
        /// Function called when we first get the plane info, our goal here is to get it flying or grounded asap
        /// </summary>
        private void ProcessUnknown(AdsbEvent currentEvent)
        {
            // Thi
            GeoCoordinate currentPosition = null;
            if (currentEvent.Latitude != null)
            {
                currentPosition = new GeoCoordinate((double)currentEvent.Latitude, (double)currentEvent.Longitude);
                LastKnownPosition = currentPosition;
            }

            if (LastEvent != null)
            {
                //This happens if we did not get a status reading on the last event
                // We want to set this value as fast as possible so it is a reasonable place to use distance calculation
                // set speed 
                if ((currentEvent.Speed == null) && (currentPosition != null)) {
                    if (LastEvent.Latitude != null) {
                        GeoCoordinate lastPosition = new GeoCoordinate((double)LastEvent.Latitude, (double)LastEvent.Longitude);
                        double distance = currentPosition.GetDistanceTo(lastPosition);
                        TimeSpan span = currentEvent.Timestamp.Subtract(LastEvent.Timestamp);
                        double seconds = span.TotalSeconds;
                        currentEvent.Speed = distance / seconds * 3600;
                    }
                }
            }
            // We should be grounded in mid air or again unknown
            if (currentEvent.Speed != null)
            {
                if (currentEvent.Speed > AirplaneProcessed.Model.TurbulencePenetrationSpeed)
                {
                    // Means we are flying 
                    CurrentStatus = AirplaneStatus.FLYING;
                    //Console.WriteLine("We are FLYING");
                } else if (currentEvent.Speed < AirplaneProcessed.Model.TakeoffSpeed)
                {
                    CurrentStatus = AirplaneStatus.GROUNDED;
                    //Console.WriteLine("We are GROUNDED");
                }
            }
        }


        /// <summary>
        /// Processing a grounded flight
        /// We can adjust its position and look for takeoffs
        /// </summary>
        private void ProcessGrounded(AdsbEvent currentEvent)
        {
            //Set the last auport first if it hasn been set yet
            // This happens if we enter GROUNDED from Unknown
            if (LastAirport == null && LastKnownPosition != null)
            {
                LastAirport = Airports.GetClosestAirport(LastKnownPosition);
            }

            // This is where we can process flights as they slow down. We can assume a reasonable distance here
            // The goal is to get coser and closer with the airport until we hit taxi speed
            // Inspired by A44D3F Delta - and the multitude of CLos Angeles area municipal airports
            if (LastGroundedFlight != null)
            {
                if (currentEvent.Speed != null && currentEvent.Speed > REASONABLE_TAXI_SPEED)
                {
                    Airport tempAirport = Airports.GetClosestAirport(LastKnownPosition);
                    if(tempAirport.Identifier != LastAirport.Identifier)
                    {
                        UpdateLastGroundedFlight(tempAirport);
                    }
                }
            }

            //The goal here is to look for takeoffs
            // This means we have a Reasonable grounding delay of 15 minutes before searching for takeoff speed
            bool takeoffEligible = true;
            if (LastGrounded != null)
            {
                // we will ignore events to let the plane land 
                TimeSpan landSpan = currentEvent.Timestamp.Subtract((DateTime)LastGrounded);
                double landSeconds = landSpan.TotalSeconds;
                if (landSeconds < REASONABLE_GROUNDING_DELAY)
                {

                    takeoffEligible = false;

                }
            }
            if ((currentEvent.Speed) != null && takeoffEligible )
            {
                LastDeviceSpeed = currentEvent.Speed;
                LastDeviceSpeedTime = currentEvent.Timestamp;

                // We have hit takeoff speed, change status
                if (currentEvent.Speed > AirplaneProcessed.Model.TakeoffSpeed)
                {
                    // We are taking off 
                    //Console.WriteLine("Taking off at " + currentEvent.Timestamp);
                    CurrentStatus = AirplaneStatus.FLYING;
                    LastTakeoff = currentEvent.Timestamp;
                }
            }
        }

        /// <summary>
        /// Processing a flight mid air
        /// We look for a dip into landing speed after a rasonable amunt of time has been sent in the air
        /// </summary>
        private void ProcessFlying(AdsbEvent currentEvent)
        {

            if (currentEvent.Speed != null)
            {
                LastDeviceSpeed = currentEvent.Speed;
                LastDeviceSpeedTime = currentEvent.Timestamp;
            }

            /// if we hit a speed below the landing speed  we  can be sure we are landing 
            if (currentEvent.Speed != null) {
                bool eligibleToLand = true;

                if (LastTakeoff != null)
                {
                    TimeSpan span = currentEvent.Timestamp.Subtract((DateTime)LastTakeoff);
                    double seconds = span.TotalSeconds;
                    // Give the plane some time to take off
                    if (seconds < REASONABLE_TAKEOFF_DELAY)
                    {
                        eligibleToLand = false;
                    }
                }

                // Land the plane
                if ((currentEvent.Speed < AirplaneProcessed.Model.LandingSpeed) && eligibleToLand)
                {
                    //Console.WriteLine("grounded at" + currentEvent.Timestamp);
                    CurrentStatus = AirplaneStatus.GROUNDED;
                    LastGrounded = currentEvent.Timestamp;
                    ExportFlight(currentEvent);
                }
            }
        }


        /// <summary>
        /// Add a completed flight to the flight queue
        /// A completed flight is one that went from Flying to Grounded
        /// </summary>
        private void ExportFlight(AdsbEvent currentEvent)
        {
            // initialize variables
            DateTime? departureTime = null;
            DateTime? arrivalime = null;
            string departureAirport = null;
            string arrivalAirport = null;

            //set variables

            //records of this flight started Grounded
            if(LastAirport != null)
            {
                departureTime = LastTakeoff;
                departureAirport = LastAirport.Identifier;
            }

            //records of flighs that end grounded
            if(CurrentStatus == AirplaneStatus.GROUNDED)
            {
                arrivalime = currentEvent.Timestamp.AddSeconds(REASONABLE_LANDING_TIME);
                Airport airportObject = Airports.GetClosestAirport(LastKnownPosition);
                if (airportObject != null)
                {
                    arrivalAirport = airportObject.Identifier;
                    LastAirport = airportObject;
                }
                

            }

            Flight toAdd = new Flight(AirplaneProcessed.Identifier, departureTime, departureAirport, arrivalime, arrivalAirport);
            LastGroundedFlight = toAdd;
            FlightsQueue.Enqueue(toAdd);
        }


        /// <summary>
        /// Update the airport value - this is called from a grounded flight that just got a better airport reading
        /// </summary>
        private void UpdateLastGroundedFlight(Airport airport)
        {
            LastAirport = airport;
            LastGroundedFlight.ArrivalAirport = airport.Identifier;
        }

        /// <summary>
        /// Garbage collector for the indvidual Airplane Event Processor
        /// </summary>
        public void AddUnknownFlight()
        {
            if(CurrentStatus == AirplaneStatus.FLYING)
            {
                ExportFlight(null);
            }
        }


    }
}
