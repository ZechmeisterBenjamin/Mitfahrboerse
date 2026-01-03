namespace Mitfahrboerse.Interfaces;

public interface IPointRelevantItem
{
    short Status { get; set; }
    bool IsProcessed { get; set; }
}