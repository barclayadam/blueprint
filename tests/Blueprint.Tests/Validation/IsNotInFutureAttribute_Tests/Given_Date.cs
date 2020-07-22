using System;
using Blueprint.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Validation.IsNotInFutureAttribute_Tests
{
    public class Given_Date
    {
        [Test]
        public void When_DateTime_Is_In_Future_Then_Invalid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInFutureAttribute.IsValid(SystemTime.UtcNow.AddDays(1));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_In_Past_Then_Valid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInFutureAttribute.IsValid(SystemTime.UtcNow.AddDays(-1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_DateTime_Is_Invalid_String_Then_Is_Invalid()
        {
            // Arrange
            var futureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);
            var dateTime = "Tenth Of June 2011";

            // Act
            var isValid = futureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);
            const int dateInteger = 10102010;

            // Act
            var exception = Assert.Catch<InvalidOperationException>(() => notInFutureAttribute.IsValid(dateInteger));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_DateTime_Is_String_In_Future_Then_Inalid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);
            var dateTime = DateTime.Now.AddDays(1).Date.ToString();

            // Act
            var isValid = notInFutureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_String_In_Past_Then_Valid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.DateTime);
            var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

            // Act
            var isValid = notInFutureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_In_Future_Then_Invalid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInFutureAttribute.IsValid(SystemTime.UtcNow.AddDays(1));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_In_Past_Then_Valid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInFutureAttribute.IsValid(SystemTime.UtcNow.AddDays(-1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_Invalid_String_Then_Is_Invalid()
        {
            // Arrange
            var futureAttribute = new NotInFutureAttribute(TemporalCheck.Date);
            var dateTime = "Tenth Of June 2011";

            // Act
            var isValid = futureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.Date);
            const int dateInteger = 10102010;

            // Act
            var exception = Assert.Catch<InvalidOperationException>(() => notInFutureAttribute.IsValid(dateInteger));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Date_Is_String_In_Future_Then_Inalid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.Date);
            var dateTime = DateTime.Now.AddDays(1).Date.ToString();

            // Act
            var isValid = notInFutureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_String_In_Past_Then_Valid()
        {
            // Arrange
            var notInFutureAttribute = new NotInFutureAttribute(TemporalCheck.Date);
            var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

            // Act
            var isValid = notInFutureAttribute.IsValid(dateTime);

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
