namespace Mitfahrboerse.Models
{
    public class Admin
    {
        public t_Offer Offer { get; set; }
        public int RideCount { get; set; }
        public double Distance { get; set; }
        public int PassangerCount { get; set; }
    }
}
