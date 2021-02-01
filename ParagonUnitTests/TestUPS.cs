using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Concurrent;
using System;

namespace ParagonUnitTests
{
    [TestClass]
    public class TestUPS
    {
        [TestMethod]
        public void TestUPSMethod()
        {
            // We consider our timing done right if it fits in a 10 minute window
            double ACCEPTABLE_WINDOW = 10;
            // The easiest way for us to test our stuff is to make sure it works with regular files 
            //Location of Airports JSON
            string AirportsFilePath = @".\Resources\airports.json";

            // Location of ADS-B events
            string AdsbEventsFilePath = @".\Resources\events.txt";
            string p = Path.GetFullPath(AirportsFilePath);

            ParagonCodingExercise.FlightProcessor testProcessor = new ParagonCodingExercise.FlightProcessor(AirportsFilePath, AdsbEventsFilePath, "A30626");
            ParagonCodingExercise.FileReader fp = new ParagonCodingExercise.FileReader(testProcessor.getEventQueue(), testProcessor.getAdsbEventsFilePath());
            fp.ParseFile();
            testProcessor.ProcessEvents();
            testProcessor.ProcessIncompleteEvents();

            ConcurrentQueue<ParagonCodingExercise.Flight> testOutput = testProcessor.getFlightQueue();

            Assert.AreEqual(testOutput.Count, 3);
            ParagonCodingExercise.Flight[] flightArray = testOutput.ToArray();

            // first flight 
            DateTime testTimeArrive0 = new DateTime(2020, 5, 26, 0, 30, 00);
            double arriveDiff0 = System.Math.Abs((testTimeArrive0 - (DateTime)flightArray[0].ArrivalTime).TotalMinutes);

            Assert.IsTrue(arriveDiff0 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[0].ArrivalAirport, "KONT");
            Assert.IsNull(flightArray[0].DepartureAirport);
            Assert.IsNull(flightArray[0].DepartureTime);

            // second flight
            DateTime testTimeDepart1 = new DateTime(2020, 5, 26, 4, 00, 00);
            DateTime testTimeArrive1 = new DateTime(2020, 5, 26, 13, 05, 00);
            double departDiff1 = System.Math.Abs((testTimeDepart1 - (DateTime)flightArray[1].DepartureTime).TotalMinutes);
            double arriveDiff1 = System.Math.Abs((testTimeArrive1 - (DateTime)flightArray[1].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff1 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff1 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[1].ArrivalAirport, "PHKO");
            Assert.AreEqual(flightArray[1].DepartureAirport, "KONT");

            // Third flight
            DateTime testTimeDepart2 = new DateTime(2020, 5, 26, 15, 00, 00);
            DateTime testTimeArrive2 = new DateTime(2020, 5, 26, 20, 05, 00);
            double departDiff2 = System.Math.Abs((testTimeDepart2 - (DateTime)flightArray[2].DepartureTime).TotalMinutes);
            double arriveDiff2 = System.Math.Abs((testTimeArrive2 - (DateTime)flightArray[2].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff2 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff2 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[2].ArrivalAirport, "KONT");
            Assert.AreEqual(flightArray[2].DepartureAirport, "PHKO");
        }
    }
}
