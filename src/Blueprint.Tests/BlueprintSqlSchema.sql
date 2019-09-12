CREATE TABLE AuditTrail (
    [CorrelationId] [nvarchar](150) NULL,
    [WasSuccessful] [bit] NOT NULL,
    [ResultMessage] [nvarchar](4000) NOT NULL,
    [Username] [nvarchar](100) NOT NULL,
    [Timestamp] [datetime] NOT NULL,
    [MessageType] [nvarchar](50) NOT NULL,
    [MessageData] [nvarchar](4000) NOT NULL
)