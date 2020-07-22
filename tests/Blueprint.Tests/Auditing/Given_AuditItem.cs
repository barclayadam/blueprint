using System;
using System.Linq;
using Blueprint.Auditing;
using Blueprint.SqlServer;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Blueprint.Tests.Auditing
{
    public class Given_AuditItem : With_Local_Database
    {
        [Test]
        public void When_AuditItem_With_Details_That_Is_Not_Denormalised_Then_Persisted()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var resultMessage = "Result Message";
            var username = "Username";
            var details = new { Property = "Something Interesting" };
            var expectedResult = "{\"Property\":\"Something Interesting\"}";
            var wasSuccessful = true;

            var auditItem = new AuditItem(correlationId, wasSuccessful, resultMessage, username, details);
            var auditor = CreateAuditor();

            // Act
            auditor.Write(auditItem);

            // Assert
            var count =
                Query<int>(
                    @"SELECT COUNT(*) FROM AuditTrail
                                      WHERE CorrelationId = @CorrelationId AND 
                                            WasSuccessful = @WasSuccessful AND
                                            ResultMessage LIKE @ResultMessage AND 
                                            Username = @Username AND 
                                            MessageData LIKE @ExpectedResult",
                    new
                    {
                        CorrelationId = correlationId,
                        wasSuccessful,
                        resultMessage,
                        username,
                        expectedResult
                    }).Single();

            count.Should().Be(1);
        }

        [Test]
        public void When_AuditItem_With_Details_That_Has_DoNotAuditAttribute_Then_Not_Serialized()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var resultMessage = "Result Message";
            var username = "Username";
            var details = new ClassWithDoNotAuditProperty { DoAuditProperty = "Something boring", DoNotAuditProperty = "Something Interesting" };
            var expectedResult = "{\"DoAuditProperty\":\"Something boring\"}";
            var wasSuccessful = true;

            var auditItem = new AuditItem(correlationId, wasSuccessful, resultMessage, username, details);
            var auditor = CreateAuditor();

            // Act
            auditor.Write(auditItem);

            // Assert
            var count =
                Query<int>(
                    @"SELECT COUNT(*) FROM AuditTrail
                                      WHERE CorrelationId = @CorrelationId AND 
                                            WasSuccessful = @WasSuccessful AND
                                            ResultMessage LIKE @ResultMessage AND 
                                            Username = @Username AND 
                                            MessageData LIKE @ExpectedResult",
                    new
                    {
                        CorrelationId = correlationId,
                        wasSuccessful,
                        resultMessage,
                        username,
                        expectedResult
                    }).Single();

            count.Should().Be(1);
        }

        [Test]
        public void When_AuditItem_With_Details_That_Has_SensitiveAttribute_Then_Not_Serialized()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var resultMessage = "Result Message";
            var username = "Username";
            var details = new ClassWithSensitiveProperty { DoAuditProperty = "Something boring", DoNotAuditProperty = "Something Interesting" };
            var expectedResult = "{\"DoAuditProperty\":\"Something boring\"}";
            var wasSuccessful = true;

            var auditItem = new AuditItem(correlationId, wasSuccessful, resultMessage, username, details);
            var auditor = CreateAuditor();

            // Act
            auditor.Write(auditItem);

            // Assert
            var count =
                Query<int>(
                    @"SELECT COUNT(*) FROM AuditTrail
                                      WHERE CorrelationId = @CorrelationId AND 
                                            WasSuccessful = @WasSuccessful AND
                                            ResultMessage LIKE @ResultMessage AND 
                                            Username = @Username AND 
                                            MessageData LIKE @ExpectedResult",
                    new
                    {
                        CorrelationId = correlationId,
                        wasSuccessful,
                        resultMessage,
                        username,
                        expectedResult
                    }).Single();

            count.Should().Be(1);
        }

        private SqlServerAuditor CreateAuditor()
        {
            return new SqlServerAuditor(CreateFactory(), new OptionsWrapper<SqlServerAuditorConfiguration>(new SqlServerAuditorConfiguration
            {
                QualifiedTableName = "AuditTrail"
            }));
        }
    }

    public class ClassWithDoNotAuditProperty
    {
        public string DoAuditProperty { get; set; }

        [DoNotAudit]
        public string DoNotAuditProperty { get; set; }
    }

    public class ClassWithSensitiveProperty
    {
        public string DoAuditProperty { get; set; }

        [Sensitive]
        public string DoNotAuditProperty { get; set; }
    }
}
