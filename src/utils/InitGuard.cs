namespace LightAssistant.Utils;

public class InitGuard
{
    private const int Yes = 1;
    private const int No = 0;
    private int _hasBeenInit = No;

    public bool Check()
    {
        var hasBeenInit = Interlocked.Exchange(ref _hasBeenInit, Yes);
        return hasBeenInit == Yes;
    }
}
