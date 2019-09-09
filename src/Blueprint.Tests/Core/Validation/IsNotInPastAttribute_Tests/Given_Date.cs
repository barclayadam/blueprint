using System;
using Blueprint.Core;
using Blueprint.Core.Validation;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Validation.IsNotInPastAttribute_Tests
{
    public class Given_Date
    {
        [Test]
        public void When_DateTime_Is_In_Future_Then_Valid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddDays(1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_DateTime_Is_In_Past_Then_Invalid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddDays(-1));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_Invalid_String_Then_Is_Invalid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);
            var dateTime = "Tenth Of June 2011";

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);
            const int dateInteger = 10102010;

            // Act
            var exception = Assert.Catch<InvalidOperationException>(() => notInPastAttribute.IsValid(dateInteger));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_DateTime_Is_One_Second_In_Future_Then_Valid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddSeconds(1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_DateTime_Is_One_Second_In_Past_Then_Invalid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddSeconds(-1));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_DateTime_Is_String_In_Future_Then_Valid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);
            var dateTime = DateTime.Now.AddDays(1).Date.ToString();

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_DateTime_Is_String_In_Past_Then_Inalid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.DateTime);
            var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_In_Future_Then_Valid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddDays(1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_In_Past_Then_Invalid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddDays(-1));

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_Invalid_String_Then_Is_Invalid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);
            var dateTime = "Tenth Of June 2011";

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }

        [Test]
        public void When_Date_Is_Not_DateTime_Or_String_Then_Exception_Is_Thrown()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);
            const int dateInteger = 10102010;

            // Act
            var exception = Assert.Catch<InvalidOperationException>(() => notInPastAttribute.IsValid(dateInteger));

            // Assert
            exception.Should().NotBeNull();
        }

        [Test]
        public void When_Date_Is_One_Second_In_Future_Then_Valid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddSeconds(1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_One_Second_In_Past_Then_Valid()
        {
            // Arrange
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);

            // Act
            var isValid = notInPastAttribute.IsValid(SystemTime.UtcNow.AddSeconds(-1));

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_String_In_Future_Then_Valid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);
            var dateTime = DateTime.Now.AddDays(1).Date.ToString();

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void When_Date_Is_String_In_Past_Then_Invalid()
        {
            // Arrange 
            var notInPastAttribute = new NotInPastAttribute(TemporalCheck.Date);
            var dateTime = DateTime.Now.AddDays(-1).Date.ToString();

            // Act
            var isValid = notInPastAttribute.IsValid(dateTime);

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}