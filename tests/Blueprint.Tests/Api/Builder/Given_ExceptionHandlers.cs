using System;
using System.Collections.Generic;
using Blueprint.Api;
using Blueprint.Api.CodeGen;
using Blueprint.Api.Errors;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.Builder
{
    public class Given_ExceptionHandlers
    {
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

            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithMiddleware(middleware));

            // Assert
            var code = executor.WhatCodeDidIGenerateFor<TestApiCommand>();
            code.Should().Contain("catch (Blueprint.Api.Errors.NotFoundException");
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

            var executor = TestApiOperationExecutor.Create(o => o
                .WithHandler(handler)
                .WithMiddleware(middleware));

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

            public bool Matches(ApiOperationDescriptor operation)
            {
                return true;
            }

            public void Build(MiddlewareBuilderContext context)
            {
                context.RegisterUnhandledExceptionHandler(exceptionType, create);
            }
        }
    }
}
