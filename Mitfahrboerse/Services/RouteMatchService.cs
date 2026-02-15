using Mitfahrboerse.Models;

namespace Mitfahrboerse.Services
{
    public class RouteMatchService : IRouteMatchService
    {
        private const double EarthRadiusKm = 6371.0;
        private const double MaxDetourToleranceKm = 30; // Maximaler akzeptabler Umweg

        /// <summary>
        /// Haversine-Formel zur Berechnung der Distanz zwischen zwei GPS-Koordinaten
        /// </summary>
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

            // Direkte Distanz für den Passagier
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

                // Prüfe, ob Passagier-Startpunkt auf der Route liegt
                var distanceToPassengerStart = CalculateDistanceKm(
                    driverStartLat, driverStartLon,
                    passengerStartLat, passengerStartLon
                );

                // Prüfe, ob Passagier-Zielort auf der Route liegt
                var distanceToPassengerEnd = CalculateDistanceKm(
                    driverEndLat, driverEndLon,
                    passengerEndLat, passengerEndLon
                );

                // Prüfe, ob Start im Bereich des Fahrers liegt (mit Toleranz)
                var startIsOnRoute = distanceToPassengerStart <= MaxDetourToleranceKm;
                // Prüfe, ob Ziel im Bereich des Fahrers liegt
                var endIsOnRoute = distanceToPassengerEnd <= MaxDetourToleranceKm;

                if (startIsOnRoute || endIsOnRoute)
                {
                    // Berechne den Umweg: (Fahrer-Route + Umweg zum Passagier) - direkte Passagier-Route
                    var detourDistance = CalculateDetourDistance(
                        driverStartLat, driverStartLon, driverEndLat, driverEndLon,
                        passengerStartLat, passengerStartLon, passengerEndLat, passengerEndLon
                    );

                    // Nur hinzufügen, wenn Umweg akzeptabel ist
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

            // Sortiere nach Umweg (aufsteigend - weniger Umweg zuerst)
            return matchingRides.OrderBy(r => r.DetourKilometers).ToList();
        }

        // In RouteMatchService.cs
        private double CalculateDetourDistance(
            decimal driverStartLat, decimal driverStartLon,
            decimal driverEndLat, decimal driverEndLon,
            decimal passengerStartLat, decimal passengerStartLon,
            decimal passengerEndLat, decimal passengerEndLon
        )
        {
            // Distance from Driver Start -> Passenger Start
            var dist1 = CalculateDistanceKm(driverStartLat, driverStartLon, passengerStartLat, passengerStartLon);
            // Distance from Passenger Start -> Passenger End
            var dist2 = CalculateDistanceKm(passengerStartLat, passengerStartLon, passengerEndLat, passengerEndLon);
            // Distance from Passenger End -> Driver End
            var dist3 = CalculateDistanceKm(passengerEndLat, passengerEndLon, driverEndLat, driverEndLon);

            // Total route for driver with the stop: Start -> P_Start -> P_End -> End
            var totalNewRoute = dist1 + dist2 + dist3;

            // Original direct distance for the driver
            var originalRoute = CalculateDistanceKm(driverStartLat, driverStartLon, driverEndLat, driverEndLon);

            // Detour is the difference
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
