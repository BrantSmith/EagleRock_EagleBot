﻿namespace Transmax_EagleRock_EagleBot.Models
{
    public struct Coordinate
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}