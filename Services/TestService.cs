using MagicOnion;
using MagicOnion.Server;
using PjOutGame.Shared;

namespace PjOutGame.Server.Services;

public class TestService : ServiceBase<ITestService>, ITestService
{
    public async UnaryResult<int> DoubleNumber(int number)
    {
        return number * 2;
    }

    public async UnaryResult<string> ReverseString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var charArray = input.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public async UnaryResult<int> AddNumbers(int a, int b)
    {
        return a + b;
    }

    public async UnaryResult<DateTime> GetCurrentTime()
    {
        return DateTime.UtcNow;
    }
}

