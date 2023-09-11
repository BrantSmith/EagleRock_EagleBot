namespace EagleRock_EagleBot.Models
{
    public class EagleBotData
    {
        public required Guid Id { get; set; }
        public required Coordinate Location { get; set; }
        public required DateTimeOffset Timestamp { get; set; }
        public required string RoadName { get; set; }
        public required DirectionOfTraffic Direction { get; set; }
        public required double RateOfTrafficFlow { get; set; }
        public required double AvgVehicleSpeed { get; set; }
    }
}