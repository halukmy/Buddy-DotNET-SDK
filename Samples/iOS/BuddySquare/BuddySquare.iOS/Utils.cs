using System;

namespace BuddySquare.iOS
{
    public class Utils
    {
        /// <summary>Converts miles to latitude degrees</summary>
        public static double MilesToLatitudeDegrees(double miles)
        {
            double earthRadius = 3960.0; // in miles
            double radiansToDegrees = 180.0/Math.PI;
            return (miles/earthRadius) * radiansToDegrees;
        }

        /// <summary>Converts miles to longitudinal degrees at a specified latitude</summary>
        public static double MilesToLongitudeDegrees(double miles, double atLatitude)
        {
            double earthRadius = 3960.0; // in miles
            double degreesToRadians = Math.PI/180.0;
            double radiansToDegrees = 180.0/Math.PI;
            // derive the earth's radius at that point in latitude
            double radiusAtLatitude = earthRadius * Math.Cos(atLatitude * degreesToRadians);
            return (miles / radiusAtLatitude) * radiansToDegrees;
        }
    }
}

