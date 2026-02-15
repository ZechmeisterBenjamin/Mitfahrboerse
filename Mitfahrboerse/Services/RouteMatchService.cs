using Mitfahrboerse.Models;

namespace Mitfahrboerse.Services
{
    public class RouteMatchService : IRouteMatchService
    {
        private const double EarthRadiusKm = 6371.0;
        private const double MaxDetourToleranceKm = 30;
        public double CalculateDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var dLat = DegreesToRadians((double)(lat2 - lat1));
            var dLon = DegreesToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians((double)lat1)) * Math.Cos(DegreesToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

        public List<RideWithDetourInfo> FindMatchingRides(
            List<t_Ride> rides,
            decimal passengerStartLat,
            decimal passengerStartLon,
            decimal passengerEndLat,
            decimal passengerEndLon
        )
        {
            var matchingRides = new List<RideWithDetourInfo>();

            var passengerDirectDistance = CalculateDistanceKm(
                passengerStartLat, passengerStartLon,
                passengerEndLat, passengerEndLon
            );

            foreach (var ride in rides)
            {
                var driverStartLat = ride.FK_StartsAt_Position.Latitude;
                var driverStartLon = ride.FK_StartsAt_Position.Longitude;
                var driverEndLat = ride.FK_EndsAt_Position.Latitude;
                var driverEndLon = ride.FK_EndsAt_Position.Longitude;

                var distanceToPassengerStart = CalculateDistanceKm(
                    driverStartLat, driverStartLon,
                    passengerStartLat, passengerStartLon
                );

                var distanceToPassengerEnd = CalculateDistanceKm(
                    driverEndLat, driverEndLon,
                    passengerEndLat, passengerEndLon
                );

                var startIsOnRoute = distanceToPassengerStart <= MaxDetourToleranceKm;
                var endIsOnRoute = distanceToPassengerEnd <= MaxDetourToleranceKm;

                if (startIsOnRoute || endIsOnRoute)
                {
                    var detourDistance = CalculateDetourDistance(
                        driverStartLat, driverStartLon, driverEndLat, driverEndLon,
                        passengerStartLat, passengerStartLon, passengerEndLat, passengerEndLon
                    );

                    if (detourDistance <= MaxDetourToleranceKm)
                    {
                        matchingRides.Add(new RideWithDetourInfo
                        {
                            Ride = ride,
                            DetourKilometers = detourDistance,
                            MatchType = DetermineMatchType(startIsOnRoute, endIsOnRoute)
                        });
                    }
                }
            }

            return matchingRides.OrderBy(r => r.DetourKilometers).ToList();
        }

        private double CalculateDetourDistance(
            decimal driverStartLat, decimal driverStartLon,
            decimal driverEndLat, decimal driverEndLon,
            decimal passengerStartLat, decimal passengerStartLon,
            decimal passengerEndLat, decimal passengerEndLon
        )
        {
            var dist1 = CalculateDistanceKm(driverStartLat, driverStartLon, passengerStartLat, passengerStartLon);
            var dist2 = CalculateDistanceKm(passengerStartLat, passengerStartLon, passengerEndLat, passengerEndLon);
            var dist3 = CalculateDistanceKm(passengerEndLat, passengerEndLon, driverEndLat, driverEndLon);

            var totalNewRoute = dist1 + dist2 + dist3;

            var originalRoute = CalculateDistanceKm(driverStartLat, driverStartLon, driverEndLat, driverEndLon);

            return Math.Max(0, totalNewRoute - originalRoute);
        }

        private string DetermineMatchType(bool startMatches, bool endMatches)
        {
            if (startMatches && endMatches) return "BOTH";
            if (startMatches) return "START";
            return "END";
        }
    }

}
