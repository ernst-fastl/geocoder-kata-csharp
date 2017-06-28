using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeocoderKataImplementation
{
    class Coordinate
    {
        public double latitude;
        public double longitude;

        public Coordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }


        public String toString()
        {
            return "Coordinate{" +
                    "latitude=" + latitude +
                    ", longitude=" + longitude +
                    '}';
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
    class GeoDistanceEngine
    {
        private static double EARTH_RADIUS_IN_METERS = 6378137;
        private static double PI = 3.14;
        private static double DEGREE_TO_RADIAN = PI / 180;

        public double evaluate(Coordinate c1, Coordinate c2)
        {
            double dlong = (c2.longitude - c1.longitude) * DEGREE_TO_RADIAN;
            double dlat = (c2.latitude - c1.latitude) * DEGREE_TO_RADIAN;
            double a = Math.Pow(Math.Sin(dlat / 2D), 2D) +
                    Math.Cos(c1.latitude * DEGREE_TO_RADIAN) * Math.Cos(c2.latitude * DEGREE_TO_RADIAN) * Math.Pow(Math.Sin(dlong / 2D), 2D);
            double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));

            return EARTH_RADIUS_IN_METERS * c;
        }
    }
    class GoogleGeocoding
    {
        private GeoApiContext context;

        public GoogleGeocoding(String apiKey)
        {
            context = new GeoApiContext().setApiKey(apiKey);
        }

        public Optional<Coordinate> geocode(String address)
        {
            return getGeocodingResults(address)
                    .filter(results->results.length > 0)
                    .map(results->results[0])
                    .map(result-> new Coordinate(result.geometry.location.lat, result.geometry.location.lng));
        }

        private Optional<GeocodingResult[]> getGeocodingResults(String address)
        {
            try
            {
                return Optional.ofNullable(GeocodingApi.geocode(context, address).await());
            }
            catch (Exception e)
            {
                return empty();
            }
        }
    }

    class OpenStreetMapGeocoding
    {
        private static String OPEN_STREET_MAP_URL = "http://nominatim.openstreetmap.org/search?format=json&limit=1&q=";

        private OkHttpClient httpClient;
        private Gson gson;

        public OpenStreetMapGeocoding()
        {
            httpClient = new OkHttpClient();
            gson = new GsonBuilder().create();
        }

        public Coordinate search(String address)
        {
            String encodedAddress;
            encodedAddress = URLEncoder.encode(address, "UTF-8");

            Request request = new Request.Builder().url(OPEN_STREET_MAP_URL + encodedAddress).build();

            Response response = httpClient.newCall(request).execute();

            return gson.fromJson(response.body());
            //TODO
        }
    }

    class OpenStreetMapResult
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
    class App
    {
        private GoogleGeocoding googleGeocoding;
        private GeoDistanceEngine distanceEngine;

        public App()
        {
            String apiKey = Optional.ofNullable(System.getProperty("google.apiKey"))
                    .orElseThrow(()-> new RuntimeException("please provide system property google.apiKey"));

            this.googleGeocoding = new GoogleGeocoding(apiKey);
            this.distanceEngine = new GeoDistanceEngine();
        }

        public Optional<Double> getDistance(String firstAddress, String secondAddress)
        {
            Optional<Coordinate> firstCoordinate = googleGeocoding.geocode(firstAddress);
            if (!firstCoordinate.isPresent())
            {
                return empty();
            }

            Optional<Coordinate> secondCoordinate = googleGeocoding.geocode(secondAddress);
            if (!secondCoordinate.isPresent())
            {
                return empty();
            }

            return Optional.of(distanceEngine.evaluate(
                    firstCoordinate.get(),
                    secondCoordinate.get()));
        }

        public static void main(String[] args)
        {
            if (args.length != 2)
            {
                System.err.println("Please provide first and second address. Example: command <first> <second>");
                System.exit(1);
            }
            Optional<Double> distance = new App().getDistance(args[0], args[1]);
            System.out.println(distance);
        }
    }
