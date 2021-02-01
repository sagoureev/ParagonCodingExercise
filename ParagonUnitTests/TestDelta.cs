using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Concurrent;
using System;

namespace ParagonUnitTests
{
    [TestClass]
    public class TestDelta
    {
        [TestMethod]
        public void TestDeltaMethod()
        {
            // We consider our timing done right if it fits in a 10 minute window
            double ACCEPTABLE_WINDOW = 10;
            // The easiest way for us to test our stuff is to make sure it works with regular files 
            //Location of Airports JSON
            string AirportsFilePath = @".\Resources\airports.json";

            // Location of ADS-B events
            string AdsbEventsFilePath = @".\Resources\events.txt";
            string p = Path.GetFullPath(AirportsFilePath);

            ParagonCodingExercise.FlightProcessor testProcessor = new ParagonCodingExercise.FlightProcessor(AirportsFilePath, AdsbEventsFilePath, "A44D3F");
            ParagonCodingExercise.FileReader fp = new ParagonCodingExercise.FileReader(testProcessor.getEventQueue(), testProcessor.getAdsbEventsFilePath());
            fp.ParseFile();
            testProcessor.ProcessEvents();
            testProcessor.ProcessIncompleteEvents();

            ConcurrentQueue<ParagonCodingExercise.Flight> testOutput = testProcessor.getFlightQueue();

            
            Assert.AreEqual(testOutput.Count, 7);
            ParagonCodingExercise.Flight[] flightArray = testOutput.ToArray();

            // first flight 
            DateTime testTimeArrive0 = new DateTime(2020, 5, 26, 0, 30, 00);
            double arriveDiff0 = System.Math.Abs((testTimeArrive0 - (DateTime)flightArray[0].ArrivalTime).TotalMinutes);

            Assert.IsTrue(arriveDiff0 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[0].ArrivalAirport, "KLAX");
            Assert.IsNull(flightArray[0].DepartureAirport);
            Assert.IsNull(flightArray[0].DepartureTime);

            // second flight
            DateTime testTimeDepart1 = new DateTime(2020, 5, 26, 1, 35, 00);
            DateTime testTimeArrive1 = new DateTime(2020, 5, 26, 3, 0, 00);
            double departDiff1 = System.Math.Abs((testTimeDepart1 - (DateTime)flightArray[1].DepartureTime).TotalMinutes);
            double arriveDiff1 = System.Math.Abs((testTimeArrive1 - (DateTime)flightArray[1].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff1 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff1 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[1].ArrivalAirport, "KSLC");
            Assert.AreEqual(flightArray[1].DepartureAirport, "KLAX");

            // Third flight
            DateTime testTimeDepart2 = new DateTime(2020, 5, 26, 4, 00, 00);
            DateTime testTimeArrive2 = new DateTime(2020, 5, 26, 5, 05, 00);
            double departDiff2 = System.Math.Abs((testTimeDepart2 - (DateTime)flightArray[2].DepartureTime).TotalMinutes);
            double arriveDiff2 = System.Math.Abs((testTimeArrive2 - (DateTime)flightArray[2].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff2 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff2 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[2].ArrivalAirport, "KRNO");
            Assert.AreEqual(flightArray[2].DepartureAirport, "KSLC");

            // Fourth flight
            DateTime testTimeDepart3 = new DateTime(2020, 5, 26, 13, 20, 00);
            DateTime testTimeArrive3 = new DateTime(2020, 5, 26, 14, 20, 00);
            double departDiff3 = System.Math.Abs((testTimeDepart3 - (DateTime)flightArray[3].DepartureTime).TotalMinutes);
            double arriveDiff3 = System.Math.Abs((testTimeArrive3 - (DateTime)flightArray[3].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff3 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff3 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[3].ArrivalAirport, "KSLC");
            Assert.AreEqual(flightArray[3].DepartureAirport, "KRNO");

            // Fifth flight
            DateTime testTimeDepart4 = new DateTime(2020, 5, 26, 15, 45, 00);
            DateTime testTimeArrive4 = new DateTime(2020, 5, 26, 17, 15, 00);
            double departDiff4 = System.Math.Abs((testTimeDepart4 - (DateTime)flightArray[4].DepartureTime).TotalMinutes);
            double arriveDiff4 = System.Math.Abs((testTimeArrive4 - (DateTime)flightArray[4].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff4 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff4 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[4].ArrivalAirport, "KLAX");
            Assert.AreEqual(flightArray[4].DepartureAirport, "KSLC");

            // Sixth Flight
            DateTime testTimeDepart5 = new DateTime(2020, 5, 26, 18, 30, 00);
            DateTime testTimeArrive5 = new DateTime(2020, 5, 26, 20, 50, 00);
            double departDiff5 = System.Math.Abs((testTimeDepart5 - (DateTime)flightArray[5].DepartureTime).TotalMinutes);
            double arriveDiff5 = System.Math.Abs((testTimeArrive5 - (DateTime)flightArray[5].ArrivalTime).TotalMinutes);
            Assert.IsTrue(departDiff5 < ACCEPTABLE_WINDOW);
            Assert.IsTrue(arriveDiff5 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[5].ArrivalAirport, "KSEA");
            Assert.AreEqual(flightArray[5].DepartureAirport, "KLAX");

            // Seventh Flight 
            DateTime testTimeDepart6 = new DateTime(2020, 5, 26, 22, 30, 00);
            double departDiff6 = System.Math.Abs((testTimeDepart6 - (DateTime)flightArray[6].DepartureTime).TotalMinutes);

            Assert.IsTrue(departDiff6 < ACCEPTABLE_WINDOW);
            Assert.AreEqual(flightArray[6].DepartureAirport, "KSEA");
            Assert.IsNull(flightArray[6].ArrivalAirport);
            Assert.IsNull(flightArray[6].ArrivalTime);
        }
    }
}
