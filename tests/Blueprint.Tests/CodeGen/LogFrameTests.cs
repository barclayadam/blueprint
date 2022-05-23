using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Blueprint.CodeGen;
using Blueprint.Compiler.Tests.Scenarios;
using Blueprint.Utilities;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using VerifyNUnit;

namespace Blueprint.Tests.CodeGen;

public class LogFrameTests
{
    [TestCaseSource(nameof(WithExceptionMethods))]
    public async Task When_LogFrame_Uses_ExceptionVariable_And_No_Params(LogFrameTestCase testCase)
    {
        // Act
        var result = CodegenScenario.ForAction<ApiException, ILogger>((t, m) =>
        {
            m.Frames.Append(testCase.CreateFrame(m.FindVariable(typeof(ApiException)), "Unhandled exception with message"));
        });

        // Assert
        await Verifier.Verify(result.Code);
    }
    
    [TestCaseSource(nameof(WithExceptionMethods))]
    public async Task When_LogFrame_Uses_ExceptionVariable_And_Params(LogFrameTestCase testCase)
    {
        // Act
        var result = CodegenScenario.ForAction<ApiException, ILogger>((t, m) =>
        {
            m.Frames.Append(testCase.CreateFrame(m.FindVariable(typeof(ApiException)), "Unhandled exception with message {Message}", new object[] { "The static string message" }));
        });

        // Assert
        await Verifier.Verify(result.Code);
    }
    
    [TestCaseSource(nameof(AllMethods))]
    public async Task When_LogFrame_Has_No_Params(LogFrameTestCase testCase)
    {
        // Act
        var result = CodegenScenario.ForAction<ILogger>((t, m) =>
        {
            m.Frames.Append(testCase.CreateFrame("A static log frame message"));
        });

        // Assert
        await Verifier.Verify(result.Code);
    }
    
    [TestCaseSource(nameof(AllMethods))]
    public async Task When_LogFrame_Has_Static_String_Param(LogFrameTestCase testCase)
    {
        // Act
        var result = CodegenScenario.ForAction<ILogger>((t, m) =>
        {
            m.Frames.Append(testCase.CreateFrame("A dynamic log frame message {WithParam}", new object[] { "The param text" }));
        });

        // Assert
        await Verifier.Verify(result.Code);
    }
    
    [TestCaseSource(nameof(AllMethods))]
    public async Task When_LogFrame_Has_Dynamic_Variable_Param(LogFrameTestCase testCase)
    {
        // Act
        var result = CodegenScenario.ForAction<ILogger, string>((t, m) =>
        {
            m.Frames.Append(testCase.CreateFrame("A dynamic log frame message {WithParam}", new object[] { m.FindVariable(typeof(string)) }));
        });

        // Assert
        await Verifier.Verify(result.Code);
    }

    private static IEnumerable<object[]> AllMethods()
    {
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Debug)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Information)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Warning)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Error)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Critical)) };
    }

    private static IEnumerable<object[]> WithExceptionMethods()
    {
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Warning)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Error)) };
        yield return new object[] { new LogFrameTestCase(nameof(LogFrame.Critical)) };
    }

    public class LogFrameTestCase
    {
        private readonly string name;
        
        public LogFrameTestCase(string name)
        {
            this.name = name;
        }

        public LogFrame CreateFrame(params object[] parameters)
        {
            var types = parameters.Select(p => p.GetType()).ToArray();
            
            if (parameters[^1].GetType() != typeof(object[]))
            {
                types = types.ConcatSingle(typeof(object[]));
                parameters = parameters.ConcatSingle(Array.Empty<object>());
            }
            
            var method = typeof(LogFrame).GetMethod(name, BindingFlags.Static | BindingFlags.Public, null, types, null);
            
            if (method == null)
            {
                throw new InvalidOperationException("Cannot find LogFrame method with name " + name);
            }
            
            return (LogFrame)method.Invoke(null, parameters);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
