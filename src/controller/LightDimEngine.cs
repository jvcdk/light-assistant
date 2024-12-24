namespace LightAssistant.Controller;

internal class LightDimEngine
{
    private const int LastStepTimeConstantMs = 350;
    private const double RotationsZeroToFull = 2;

    private readonly object _lock = new();

    private long _lastRotateEvent = 0;
    private double _brightnessStep = 0;

    internal double HandleDim(bool isUp, double degrees)
    {
        lock(_lock) {
            var lastDirectionUp = _brightnessStep > 0;
            var isSameDirection = isUp == lastDirectionUp;
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if(isSameDirection) {
                var deltaMs = now - _lastRotateEvent;
                _brightnessStep *= Math.Exp(-deltaMs / LastStepTimeConstantMs);
            }
            else
                _brightnessStep = 0;
            _lastRotateEvent = now;

            var factor = degrees / (360.0 * RotationsZeroToFull);
            if(isUp)
                _brightnessStep += factor;
            else
                _brightnessStep -= factor;
            _brightnessStep = Math.Clamp(_brightnessStep, -0.25, 0.25);

            return _brightnessStep;
        }
    }
}
