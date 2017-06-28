using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoCoderKata
{
    public class Coordinate
    {
        public double latitude;
        public double longitude;

        public Coordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }


        public Boolean equals(Object o)
        {
            if (this == o) return true;
            if (o == null) return false;
            if (o is Coordinate)
            {
                Coordinate that = (Coordinate)o;

                return latitude == that.latitude && longitude == that.longitude;
            }
            return false;
        }
    }

    public class GeoDistanceEngine
    {
        private static double EARTH_RADIUS_IN_METERS = 6378137;
        private static double PI = 3.14;
        private static double DEGREE_TO_RADIAN = PI / 180;

        public double evaluate(GeoCodingResult c1, GeoCodingResult c2)
        {
            double dlong = (c2.lng - c1.lng) * DEGREE_TO_RADIAN;
            double dlat = (c2.lat - c1.lat) * DEGREE_TO_RADIAN;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) +
                    Math.Cos(c1.lat * DEGREE_TO_RADIAN) * Math.Cos(c2.lat * DEGREE_TO_RADIAN) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));

            return EARTH_RADIUS_IN_METERS * c;
        }
    }

    public class GeoCodingResult
    {
        public double lat;
        public double lng;

        public GeoCodingResult(double lat, double lng)
        {
            this.lat = lat;
            this.lng = lng;
        }


        public Boolean equals(GeoCodingResult that)
        {
            if (that == null) return false;
            if (this == that) return true;

            return lat == that.lat && lng == that.lng;
        }
    }

    public class GoogleGeocoding
    {
        public GeoCodingResult geocode(String address)
        {
            if (address == "TEST_1")
            {
                return new GeoCodingResult(1, 1);
            }
            else
            {
                return new GeoCodingResult(0, 0);
            }
        }

    }

    public class OpenStreetMapGeocoding
    {

        public OpenStreetMapGeocoding()
        {

        }

        public Coordinate search(String address)
        {
            if (address == "TEST_1")
            {
                return new Coordinate(1, 1);
            }
            else
            {
                return new Coordinate(0, 0);
            }
        }
    }

    public class OpenStreetMapResult
    {
        public double lat;
        public double lon;

        public OpenStreetMapResult(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public Coordinate toCoordinate()
        {
            return new Coordinate(lat, lon);
        }
    }

    class Program
    {
        private GoogleGeocoding googleGeocoding;
        private GeoDistanceEngine distanceEngine;

        public Program()
        {
            this.googleGeocoding = new GoogleGeocoding();
            this.distanceEngine = new GeoDistanceEngine();
        }

        public Double getDistance(String firstAddress, String secondAddress)
        {
            GeoCodingResult firstCoordinate = googleGeocoding.geocode(firstAddress);
            GeoCodingResult secondCoordinate = googleGeocoding.geocode(secondAddress);
            return distanceEngine.evaluate(firstCoordinate, secondCoordinate);
        }

        public static void Main(String[] args)
        {
            Double distance = new Program().getDistance(args[0], args[1]);
            Console.WriteLine(distance);
        }
    }
}
