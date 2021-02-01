using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Concurrent;
using System;

namespace ParagonUnitTests
{
    [TestClass]
    public class TestGulfstream
    {
        [TestMethod]
        public void TestGulfstreamMethod()
        {
            // We consider our timing done right if it fits in a 10 minute window
            double ACCEPTABLE_WINDOW = 10;
            // The easiest way for us to test our stuff is to make sure it works with regular files 
            //Location of Airports JSON
            string AirportsFilePath = @".\Resources\airports.json";

            // Location of ADS-B events
            string AdsbEventsFilePath = @".\Resources\events.txt";
            string p = Path.GetFullPath(AirportsFilePath);

            ParagonCodingExercise.FlightProcessor testProcessor = new ParagonCodingExercise.FlightProcessor(AirportsFilePath, AdsbEventsFilePath, "AA09F4");
            ParagonCodingExercise.FileReader fp = new ParagonCodingExercise.FileReader(testProcessor.getEventQueue(), testProcessor.getAdsbEventsFilePath());
            fp.ParseFile();
            testProcessor.ProcessEvents();
            testProcessor.ProcessIncompleteEvents();

            ConcurrentQueue<ParagonCodingExercise.Flight> testOutput = testProcessor.getFlightQueue();

            Assert.AreEqual(testOutput.Count, 2);
            ParagonCodingExercise.Flight[] flightArray = testOutput.ToArray();

            // first flight 
            DateTime testTimeArrive0 =new DateTime(2020, 5, 26, 16, 10, 00);
            DateTime testTimeDepart0 = new DateTime(2020, 5, 26, 15, 25, 00);
            double arriveDiff0 = System.Math.Abs((testTimeArrive0 - (DateTime)flightArray[0].ArrivalTime).TotalMinutes);
            double departDiff0 = System.Math.Abs((testTimeDepart0 - (DateTime)flightArray[0].DepartureTime).TotalMinutes);
            Assert.IsTrue(arriveDiff0 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(departDiff0 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[0].ArrivalAirport, "KLAS");
            Assert.AreEqual(flightArray[0].DepartureAirport, "KVNY");


            // second flight
            DateTime testTimeDepart1 = new DateTime(2020, 5, 26, 16, 50, 00);
            DateTime testTimeArrive1 = new DateTime(2020, 5, 26, 17, 30, 00);
            double departDiff1 = System.Math.Abs((testTimeDepart1 - (DateTime)flightArray[1].DepartureTime).TotalMinutes);
            double arriveDiff1 = System.Math.Abs((testTimeArrive1 - (DateTime)flightArray[1].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff1 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff1 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[1].ArrivalAirport, "KVNY");
            Assert.AreEqual(flightArray[1].DepartureAirport, "KLAS");

        }
    }
}
