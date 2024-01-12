namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    Task ConnectAsync();
    void Publish(string topic, string message);
    void Subscribe(string topic, Action<string, string> callback);
}
