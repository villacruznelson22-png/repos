using System.Reflection;
using Xunit.Sdk;

namespace WholesalePOS.Infrastructure.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestDatabaseAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        TestDatabase.ResetAsync()
            .GetAwaiter()
            .GetResult();
    }
}