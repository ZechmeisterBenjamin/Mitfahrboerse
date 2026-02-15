using Mitfahrboerse.Models;

namespace Mitfahrboerse.Services;


public interface IRouteMatchService
{
    double CalculateDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2);

    List<RideWithDetourInfo> FindMatchingRides(
        List<t_Ride> rides,
        decimal passengerStartLat,
        decimal passengerStartLon,
        decimal passengerEndLat,
        decimal passengerEndLon
    );
}

public class RideWithDetourInfo
{
    public t_Ride Ride { get; set; }
    public double DetourKilometers { get; set; }
    public string MatchType { get; set; }
}
