namespace LightAssistant.Controller;

internal class BrightnessConverter
{
    private readonly double _gamma = 10; // Range: _gamma > double.Epsilon. TODO: Make a user interface for configuring gamma.

    internal readonly int MaxRawBrightness;

    internal BrightnessConverter(int maxRawBrightness)
    {
        MaxRawBrightness = maxRawBrightness;
    }

    internal int NormToRaw(double normBrightness) => (int)Math.Round(ApplyGamma(normBrightness) * MaxRawBrightness);
    internal double RawToNorm(int rawBrightness) => UnApplyGamma(rawBrightness / (double)MaxRawBrightness);

    internal double MinVisibleNormBrightness => RawToNorm(1);

    private double UnApplyGamma(double value) => Math.Pow(value, 1.0 / _gamma);
    private double ApplyGamma(double value) => Math.Pow(value, _gamma);

    internal bool TryCalcNextStep(ref double brightnessNorm, int direction)
    {
        var raw = NormToRaw(brightnessNorm);
        var next = raw + direction;
        if (next < 0) {
            brightnessNorm = 0;
            return false;
        }
        if (next > MaxRawBrightness) {
            brightnessNorm = 1;
            return false;
        }
        brightnessNorm = RawToNorm(next);
        return true;
    }
}
