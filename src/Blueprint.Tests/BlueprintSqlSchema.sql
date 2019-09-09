CREATE TABLE AuditTrail (
    [CorrelationId] [nvarchar](150) NULL,
    [WasSuccessful] [bit] NOT NULL,
    [ResultMessage] [nvarchar](4000) NOT NULL,
    [Username] [nvarchar](100) NOT NULL,
    [Timestamp] [datetime] NOT NULL,
    [MessageType] [nvarchar](50) NOT NULL,
    [MessageData] [nvarchar](4000) NOT NULL
)

GO

CREATE TABLE Tasks (
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CorrelationId] [nvarchar](150) NULL,
    [TaskType] [nvarchar](1000) NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
    [TaskData] [nvarchar](4000) NOT NULL,
    [CreationDateTime] [datetime] NOT NULL,
    [LastUpdatedDateTime] [datetime] NULL,
    [LastErrorMessage] [nvarchar](4000) NULL,
    [FailedAttempts] int NOT NULL
) 
