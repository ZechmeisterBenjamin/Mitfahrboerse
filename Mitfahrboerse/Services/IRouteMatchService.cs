using Mitfahrboerse.Models;

namespace Mitfahrboerse.Services;

/// <summary>
/// Service für intelligente Fahrtsuche basierend auf Routenabgleich.
/// Findet Fahrten, die über Start- und Zielort des Passagiers führen.
/// </summary>
public interface IRouteMatchService
{
    /// <summary>
    /// Berechnet die Distanz zwischen zwei Positionen in Kilometern (Luftlinie).
    /// </summary>
    double CalculateDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2);

    /// <summary>
    /// Findet alle Fahrten, die über die angegebenen Start- und Zielkoordinaten führen.
    /// </summary>
    /// <param name="rides">Alle verfügbaren Fahrten</param>
    /// <param name="passengerStartLat">Latitude des Startpunkts des Passagiers</param>
    /// <param name="passengerStartLon">Longitude des Startpunkts des Passagiers</param>
    /// <param name="passengerEndLat">Latitude des Zielorts des Passagiers</param>
    /// <param name="passengerEndLon">Longitude des Zielorts des Passagiers</param>
    /// <returns>Liste von Fahrten mit berechneten Umweg-Kilometern, nach Umweg sortiert</returns>
    List<RideWithDetourInfo> FindMatchingRides(
        List<t_Ride> rides,
        decimal passengerStartLat,
        decimal passengerStartLon,
        decimal passengerEndLat,
        decimal passengerEndLon
    );
}

/// <summary>
/// DTO für eine Fahrt mit zusätzlichen Informationen zum Umweg
/// </summary>
public class RideWithDetourInfo
{
    public t_Ride Ride { get; set; }
    /// <summary>
    /// Zusätzliche Kilometer, die der Fahrer fahren müsste (Umweg)
    /// </summary>
    public double DetourKilometers { get; set; }
    /// <summary>
    /// Art der Übereinstimmung: "START", "END", "BOTH"
    /// </summary>
    public string MatchType { get; set; }
}
