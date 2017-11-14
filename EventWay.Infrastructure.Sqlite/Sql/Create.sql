CREATE TABLE Events (
    Ordering      BIGINT        PRIMARY KEY,
    EventId       CHAR (36)     UNIQUE NOT NULL,
    Created       DATETIME      NOT NULL,
    EventType     VARCHAR (450) NOT NULL,
    AggregateType VARCHAR (100) NOT NULL,
    AggregateId   CHAR (36)     NOT NULL,
    Version       INTEGER       NOT NULL,
    Payload       TEXT          NOT NULL,
    Metadata      TEXT,
    Dispatched    BOOLEAN       NOT NULL
                                DEFAULT (0) 
);

Go
CREATE TABLE SnapshotEvents (
    Ordering      BIGINT        PRIMARY KEY,
    EventId       CHAR (36)     UNIQUE NOT NULL,
    Created       DATETIME      NOT NULL,
    EventType     VARCHAR (450) NOT NULL,
    AggregateType VARCHAR (100) NOT NULL,
    AggregateId   CHAR (36)     NOT NULL,
    Version       INTEGER       NOT NULL,
    Payload       TEXT          NOT NULL,
    Metadata      TEXT,
    Dispatched    BOOLEAN       NOT NULL
                                DEFAULT (0) 
);
