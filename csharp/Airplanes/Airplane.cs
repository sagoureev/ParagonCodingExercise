using System;
using System.Collections.Generic;
using System.Text;


namespace ParagonCodingExercise.Airplanes
{
    /// <summary>
    /// Represents an airplane model
    /// This will be use to set certain data about a particular airplane's physical and tehnical information
    /// </summary>
    public class Airplane
    {
        public static readonly double DEFAULT_TURBULENCE_PENETRATION_SPEED = 300;
        public static readonly double DEFAULT_TAKEOFF_SPEED = 50;

        // This variable was hosen because of flight A30626 going to Kona - that broadcast this signal before going dark for a takeoff
        public static readonly double DEFAULT_LANDING_SPEED = 170;

        public readonly string Name;
        /// <summary>
        /// Turbulence penetration speed is the speed a plane slows down to during severe turbulence
        /// We will use this to potentially see if a plane is landing 
        /// </summary>
        public readonly double TurbulencePenetrationSpeed;

        /// <summary>
        /// Average speed of an aircraft 
        /// </summary>
        public readonly double TakeoffSpeed;

        /// <summary>
        /// Average Landing Speed of an aircraft 
        /// </summary>
        public readonly double LandingSpeed;

        /// <summary>
        /// Default Constructor for planes. Given the data in the assignment we will use this
        /// </summary>
        public Airplane(): this("Unknown", DEFAULT_TURBULENCE_PENETRATION_SPEED, DEFAULT_TAKEOFF_SPEED, DEFAULT_LANDING_SPEED)
        {
        }

        /// <summary>
        /// Constructor when all currently available data is known
        /// </summary>
        public Airplane(string name, double turbulencePenetrationSpeed, double takeoffSpeed, double landingSpeed)
        {
            Name = name;
            TurbulencePenetrationSpeed = turbulencePenetrationSpeed;
            TakeoffSpeed = takeoffSpeed;
            LandingSpeed = landingSpeed;
        }


    }
}
