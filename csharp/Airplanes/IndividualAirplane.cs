using System;
using System.Collections.Generic;
using System.Text;

namespace ParagonCodingExercise.Airplanes
{
    /// <summary>
    /// Individual airplane class
    /// </summary>
    public class IndividualAirplane
    {
        public Airplane Model { get; set; }

        public readonly string Identifier;

        public IndividualAirplane(string identifier)
        {
            Model = new Airplane();
            Identifier = identifier;
        }
    }
}

