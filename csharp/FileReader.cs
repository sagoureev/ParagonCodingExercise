using System;
using System.Collections.Generic;
using System.Text;


//These are the system imports i added
using System.Collections.Concurrent;
using System.Threading;
using System.IO;


//We will import classes from the current project here

using ParagonCodingExercise.Events;

namespace ParagonCodingExercise
{
    /// <summary>
    /// Helper class to read events from a file
    /// TODO: Threading
    /// </summary>
    public class FileReader
    {

        private ConcurrentQueue<AdsbEvent> EventsQueue;

        private readonly string ImportFilePath;

        public FileReader(ConcurrentQueue<AdsbEvent> currentEventsQueue, string filePath)
        {
            EventsQueue = currentEventsQueue;
            ImportFilePath = filePath;
        }


        /// <summary>
        /// Function that is called when a file is rea out
        /// TODO: Update this for threading - this is the reason why this function calls the other - that call should be threaded.
        /// </summary>
        public void ParseFile()
        {
            ReadFile();
        }


        /// <summary>
        /// The actual file reader function
        /// TODO: Threading
        /// </summary>
        private void ReadFile()
        {
            using TextReader reader = new StreamReader(ImportFilePath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                AdsbEvent currentEvent = AdsbEvent.FromJson(line);
                EventsQueue.Enqueue(currentEvent);
            }
        }
    }
}
