namespace EagleRock_EagleBot.Services.Interfaces
{
    public interface IMessageProducer
    {
        void SendMessage<T>(T message);
    }
}
