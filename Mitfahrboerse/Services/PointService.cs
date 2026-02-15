using Mitfahrboerse.Models;
using Microsoft.EntityFrameworkCore;

namespace Mitfahrboerse.Services;

public interface IPointService
{
    Task AwardPointsForPastRidesAsync();
    int CalculateRidePoints(t_Ride ride);
}

public class PointService : IPointService
{
    private readonly MitfahrboerseDbContext _context;
    private readonly ILogger<PointService> _logger;

    private const int PassengerWeight = 10; 
    private const int DistanceWeight = 1; 
    private const int ExtraEffortWeight = 3; 

    public PointService(MitfahrboerseDbContext context, ILogger<PointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AwardPointsForPastRidesAsync()
    {
        try
        {
            var now = DateTime.Now;

            var pastRides = await _context.t_Rides
                .Where(r => r.RideDateTime < now && !r.IsProcessed)
                .Include(r => r.FK_Driver_Person)
                .Include(r => r.PersonRides)
                .ToListAsync();

            if (!pastRides.Any())
            {
                _logger.LogInformation("No unprocessed past rides found to award points for.");
                return;
            }

            var driverPointsToAdd = new Dictionary<string, int>();
            var ridesToMarkProcessed = new List<int>();

            foreach (var ride in pastRides)
            {
                int points = CalculateRidePoints(ride);

                if (points > 0)
                {
                    if (!driverPointsToAdd.ContainsKey(ride.FK_Driver_PersonId))
                    {
                        driverPointsToAdd[ride.FK_Driver_PersonId] = 0;
                    }
                    driverPointsToAdd[ride.FK_Driver_PersonId] += points;
                    ridesToMarkProcessed.Add(ride.RideId);
                    
                    _logger.LogInformation(
                        $"Ride {ride.RideId}: Driver {ride.FK_Driver_PersonId} earned {points} points " +
                        $"(Passengers: {ride.PersonRides.Count}, Distance: {ride.Distance}km)");
                }
            }

            foreach (var driverId in driverPointsToAdd.Keys)
            {
                var driver = await _context.t_People.FirstOrDefaultAsync(p => p.PersonId == driverId);
                if (driver != null)
                {
                    driver.Points += driverPointsToAdd[driverId];
                    _logger.LogInformation($"Driver {driverId} total points updated to {driver.Points}");
                }
            }

            foreach (var rideId in ridesToMarkProcessed)
            {
                var ride = await _context.t_Rides.FindAsync(rideId);
                if (ride != null)
                {
                    ride.IsProcessed = true;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully awarded points to {driverPointsToAdd.Count} drivers. Processed {ridesToMarkProcessed.Count} rides.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error awarding points: {ex.Message}");
            throw;
        }
    }

    public int CalculateRidePoints(t_Ride ride)
    {
        if (ride == null)
            return 0;

        int points = 0;

        int passengerCount = ride.PersonRides?.Count ?? 0;
        points += passengerCount * PassengerWeight;

        points += ride.Distance * DistanceWeight;

        return points;
    }
}
