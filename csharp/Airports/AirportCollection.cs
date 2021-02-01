using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

//For our safe dictionary
using System.Collections.Concurrent;

namespace ParagonCodingExercise.Airports
{
    public class AirportCollection
    {
        //1 mile should be a reasonable distance - this should be experimented with
        private const double REASONABLE_AIRPORT_DISANCE = 1;

        // We will use an array to represent objects
        // iterating through an array is faster than a list and the immutable property will make it thread safe
        private Airport[] AirportArray;

        /// <summary>
        /// AirportCollection constructor
        /// We fully instantiate the array and add to it for best speed in this case
        /// </summary>
        public AirportCollection(List<Airport> airports)
        {
            AirportArray = new Airport[airports.Count];
            int loopIndex = 0;
            foreach (var airport in airports)
            {
                AirportArray[loopIndex] = airport;
                loopIndex++;
            }
        }

        public static AirportCollection LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            using TextReader reader = new StreamReader(filePath);
            var json = reader.ReadToEnd();

            var airports = JsonSerializer.Deserialize<List<Airport>>(json);
            return new AirportCollection(airports);
        }

        /// <summary>
        /// Get the closest airport to a geolocation
        /// We do this through an array loop. Use a pure for loop for optimization
        /// TODO: Implement Geographic indexing
        /// </summary>
        public Airport GetClosestAirport(GeoCoordinate coordinate)
        {
            Airport toReturn = null;
            double airportDistance = double.MaxValue;
            for (int i = 0; i < AirportArray.Length; i++)
            {
                Airport tempAirport = AirportArray[i];
                GeoCoordinate airportCoordinate = new GeoCoordinate(tempAirport.Latitude, tempAirport.Longitude);
                double distance = coordinate.GetDistanceTo(airportCoordinate);
                if(distance < airportDistance)
                {
                    airportDistance = distance;
                    toReturn = tempAirport;
                    if (distance < REASONABLE_AIRPORT_DISANCE)
                    {
                        break;
                    }
                }
            }

            return toReturn;
        }
    }
}
