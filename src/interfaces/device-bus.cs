namespace LightAssistant.Interfaces;

internal interface IDeviceBus
{
    Task ConnectAsync();
    void Publish(string topic, string message);
    Task Subscribe(string topic, Action<string, string> callback);
}
