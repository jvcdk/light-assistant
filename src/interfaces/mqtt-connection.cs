namespace LightAssistant.Interfaces;

internal interface IMqttConnection
{
    Task ConnectAsync();
    Task Publish(string topic, string message);
    void SubscribeToTopic(string topic, Action<IReadOnlyList<string>, string> callback);
}
