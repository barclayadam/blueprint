﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.CodeGen;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Configuration;
using Blueprint.Errors;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core
{
    public class Given_ExceptionHandlers
    {
        [Test]
        public async Task When_Unhandled_Exception_Then_Populates_Exception_Data_With_Operation_Properties_For_ErrorLogger()
        {
            // Arrange
            var handler = new TestApiOperationHandler<TestApiCommand>(new Exception("Oops"));
            var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

            // Act
            var result = await executor.ExecuteWithNoUnwrapAsync(new TestApiCommand
            {
                AStringProperty = "the string value",
                ASensitiveStringProperty = "the sensitive value"
            });

            // Assert
            var exceptionResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            var exceptionData = exceptionResult.Exception.Data;

            exceptionData.Keys.Should().Contain($"{nameof(TestApiCommand)}.{nameof(TestApiCommand.AStringProperty)}");

            exceptionData.Keys.Should().NotContain($"{nameof(TestApiCommand)}.{nameof(TestApiCommand.ASensitiveStringProperty)}");
            exceptionData.Keys.Should().NotContain($"{nameof(TestApiCommand)}.{nameof(TestApiCommand.ADoNotAuditProperty)}");
            exceptionData.Keys.Should().NotContain($"{nameof(TestApiCommand)}.{nameof(TestApiCommand.ANakedPasswordProperty)}");
        }

        [Test]
        public async Task When_Unhandled_Exception_Then_Populates_Exception_Data_With_Operation_Properties_When_Generic_Message()
        {
            // Arrange
            var handler = new TestApiOperationHandler<OperationWrapperImpl<TestApiCommand>>(new Exception("Oops"));
            var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

            // Act
            var result = await executor.ExecuteWithNoUnwrapAsync(new OperationWrapperImpl<TestApiCommand>
            {
                Operation = new TestApiCommand
                {
                    AStringProperty = "the string value",
                    ASensitiveStringProperty = "the sensitive value"
                }
            });

            // Assert
            var exceptionResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            var exceptionData = exceptionResult.Exception.Data;

            var commandName = $"OperationWrapperImpl<{nameof(TestApiCommand)}>";
            exceptionData.Keys.Should().Contain($"{commandName}.{nameof(OperationWrapperImpl<TestApiCommand>.Operation)}");
        }

        [Test]
        public void When_Single_Custom_Exception_Handler_Registered_Then_Compiles()
        {
            // Arrange
            var handler = new TestApiOperationHandler<TestApiCommand>(12345);

            // Act
            var middleware = new ExceptionHandlingRegisteringMiddleware(
                typeof(NotFoundException), (e) =>
                {
                    return new Frame[]
                    {
                        LogFrame.Critical("Exception happened, oops"),
                        new ReturnFrame(new Variable(typeof(object), "null")),
                    };
                });

            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddleware(middleware, MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<TestApiCommand>();
            code.Should().Contain("catch (Blueprint.Errors.NotFoundException");
            code.Should().Contain("Exception happened, oops");
        }

        [Test]
        public void When_Additional_Base_Exception_Handler_Registered_Then_Compiles()
        {
            // Arrange
            var handler = new TestApiOperationHandler<TestApiCommand>(12345);

            // Act
            var middleware = new ExceptionHandlingRegisteringMiddleware(
                typeof(Exception), (e) =>
                {
                    return new Frame[]
                    {
                        LogFrame.Critical("Exception happened, oops"),
                    };
                });

            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithHandler(handler)
                .Pipeline(p => p.AddMiddleware(middleware, MiddlewareStage.Execution)));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<TestApiCommand>();
            code.Should().Contain("Exception happened, oops");
        }

        public class ExceptionHandlingRegisteringMiddleware : IMiddlewareBuilder
        {
            private readonly Type exceptionType;
            private readonly Func<Variable, IEnumerable<Frame>> create;

            public ExceptionHandlingRegisteringMiddleware(Type exceptionType, Func<Variable, IEnumerable<Frame>> create)
            {
                this.exceptionType = exceptionType;
                this.create = create;
            }

            public bool SupportsNestedExecution => true;

            public bool Matches(ApiOperationDescriptor operation)
            {
                return true;
            }

            public void Build(MiddlewareBuilderContext context)
            {
                context.RegisterUnhandledExceptionHandler(exceptionType, create);
            }
        }

        public interface IOperationWrapper<T>
        {
            T Operation { get; set; }
        }

        public class OperationWrapperImpl<T> : IOperationWrapper<T>
        {
            public T Operation { get; set; }
        }
    }
}
