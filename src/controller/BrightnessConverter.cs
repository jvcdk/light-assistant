namespace LightAssistant.Controller;

internal class BrightnessConverter
{
    internal const double MinMidBrightness = 0.01;
    internal const double MaxMidBrightness = 0.99;

    private double _gamma = 5; // Range: _gamma > double.Epsilon.
    internal double MidBrightness {
        get {
            return ApplyGamma(0.5);
        }
        set {
            value = Math.Clamp(value, MinMidBrightness, MaxMidBrightness);
            _gamma = Math.Log(value) / Math.Log(0.5);
        }
    }

    private readonly int MaxRawBrightness;

    internal BrightnessConverter(int maxRawBrightness)
    {
        MaxRawBrightness = maxRawBrightness;
    }

    internal int NormToRaw(double normBrightness) {
        if (normBrightness < MinVisibleNormBrightness)
            return 0;

        return (int)Math.Round(ApplyGamma(normBrightness) * (MaxRawBrightness - 1) + 1);
    }

    internal double RawToNorm(int rawBrightness) {
        if (rawBrightness <= 0)
            return 0;

        var result = UnApplyGamma((rawBrightness - 1) / (double)(MaxRawBrightness - 1));
        return Math.Clamp(result, MinVisibleNormBrightness, 1);
    } 

    internal int NormToRawRaw(double normBrightness) {
        if (normBrightness < MinVisibleNormBrightness)
            return 0;

        return (int)Math.Round(normBrightness * (MaxRawBrightness - 1) + 1);
    }

    internal double MinVisibleNormBrightness => float.Epsilon;

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
