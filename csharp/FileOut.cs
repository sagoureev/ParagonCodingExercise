using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Concurrent;


using System.IO;



namespace ParagonCodingExercise
{
    /// <summary>
    /// This class processes file outputs
    /// TODO: Threading
    /// </summary>
    public class FileOut
    {

        private ConcurrentQueue<Flight> FlightsQueue;
        private readonly string ExportFilePath;

        public FileOut(ConcurrentQueue<Flight> outputFlights, string filePath)
        {
            FlightsQueue = outputFlights;
            ExportFilePath = filePath;
        }


        /// <summary>
        /// Function that is called when it is time to write data to file
        /// TODO: Update this for threading - this is the reason why this function calls the other - that call should be threaded.
        /// </summary>
        public void WriteToFile()
        {
            WriteToFileExcecution();
        }

        /// <summary>
        /// The actual function that writes to file
        /// TODO: Threading
        /// </summary>
        private void WriteToFileExcecution()
        {
            TextWriter writer = new StreamWriter(ExportFilePath);
            Flight currentFlight;
            writer.WriteLine("Identifier,DepartureTime,DepartureAirport,ArrivalTime,ArrivalAirport");
            while (FlightsQueue.TryDequeue(out currentFlight))
            {
                //We have a flight
                //Console.WriteLine("Flight {0} Departure airpot {1} Departure Time {2} Arrival Airport {3} Arrival Time {4}", currentFlight.AircraftIdentifier, currentFlight.DepartureAirport, currentFlight.DepartureTime, currentFlight.ArrivalAirport, currentFlight.ArrivalTime);
                writer.WriteLine(currentFlight.ToCSV());

            }
            Console.WriteLine("Output written to {0} - have a look", Path.GetFullPath(ExportFilePath));
            writer.Flush();
            writer.Close();
            writer = null;
        }
    }
}
