namespace Transmax_EagleRock_EagleBot.Models
{
    public class EagleBotData
    {
        public Guid Id { get; set; }
        public Coordinate Location { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string RoadName { get; set; }
        public DirectionOfTraffic Direction { get; set; }
        public double RateOfTrafficFlow { get; set; }
        public double AvgVehicleSpeed { get; set; }
}
}