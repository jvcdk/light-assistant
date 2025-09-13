using LightAssistant.Interfaces;
using Newtonsoft.Json;

namespace LightAssistant;

using RunTimeData = Controller.Controller.RunTimeData;
using IDataStorage = Controller.Controller.IDataStorage;

internal class DataStorage : IDataStorage
{
    private readonly string _filepath;
    private readonly IConsoleOutput _consoleOutput;

    internal DataStorage(string filepath, IConsoleOutput consoleOutput)
    {
        _filepath = filepath;
        _consoleOutput = consoleOutput;

        if (string.IsNullOrWhiteSpace(_filepath)) {
            _consoleOutput.ErrorLine("WARNING: DataPath (specified in config file) was empty or whitespace. This does not work.");
            _consoleOutput.ErrorLine("WARNING: Configuration data will not be saved!!!");
        }
    }

    public RunTimeData LoadData()
    {
        RunTimeData? data;
        string errMsg = "Unknown reason.";
        try {
            var strData = File.ReadAllText(_filepath);
            data = JsonConvert.DeserializeObject<RunTimeData>(strData);
        }
        catch (Exception ex) {
            errMsg = ex.Message;
            data = null;
        }

        if (data == null) {
            _consoleOutput.ErrorLine($"Could not load configuration data from file '{_filepath}'. Message:" + errMsg);
            data = new RunTimeData();
        }

        return data;
    }

    public async Task SaveData(RunTimeData data)
    {
        if (string.IsNullOrWhiteSpace(_filepath))
            return; // Error message already printed

        try {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(_filepath, json);
        }
        catch (Exception ex) {
            _consoleOutput.ErrorLine($"Could not save configuration data to file '{_filepath}'. Message:" + ex.Message);
        }

    }
}