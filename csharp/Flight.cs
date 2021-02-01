using System;

namespace ParagonCodingExercise
{
    public class Flight
    {
        public string AircraftIdentifier { get; set; }

        public DateTime? DepartureTime { get; set; }

        public string DepartureAirport { get; set; }

        public DateTime? ArrivalTime { get; set; }

        public string ArrivalAirport { get; set; }

        /// <summary>
        /// Flight constructor. This accepts nullable data. We make use of this as well as Object passing by reference for flights and queues
        /// </summary>
        public Flight(string identifier, DateTime? departureTime, string departingAirport, DateTime? arrivalTime, string arrivingAirport)
        {
            AircraftIdentifier = identifier;
            DepartureTime = departureTime;
            DepartureAirport = departingAirport;
            ArrivalTime = arrivalTime;
            ArrivalAirport = arrivingAirport;
        }

        public string ToCSV()
        {
            string toReturn = $"{AircraftIdentifier}, {DepartureTime}, {DepartureAirport}, {ArrivalTime}, {ArrivalAirport}";
            return toReturn;
        }
    }
}
