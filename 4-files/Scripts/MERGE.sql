/* =================== MERGE =================== */

CREATE DATABASE MyDB
GO

USE MyDB
GO

------------------------------------------------------------------------------------
/* Stock Portfolio Example */

CREATE TABLE Stock(Symbol varchar(10) PRIMARY KEY, Qty int CHECK (Qty > 0))
CREATE TABLE Trade(Symbol varchar(10) PRIMARY KEY, Delta int CHECK (Delta <> 0))
GO

INSERT INTO Stock VALUES ('MSFT', 10)
INSERT INTO Stock VALUES ('WMT', 5)

INSERT INTO Trade VALUES('MSFT', 5)
INSERT INTO Trade VALUES('WMT', -5)
INSERT INTO Trade VALUES('GE', 3)

SELECT * FROM Stock
SELECT * FROM Trade

MERGE Stock
 USING Trade
 ON Stock.Symbol = Trade.Symbol
 WHEN MATCHED AND (Stock.Qty + Trade.Delta = 0) THEN
   -- delete stock if entirely sold
   DELETE
 WHEN MATCHED THEN
   -- update stock quantity (delete takes precedence over update)
   UPDATE SET Stock.Qty += Trade.Delta
 WHEN NOT MATCHED BY TARGET THEN
   -- add newly purchased stock
  INSERT VALUES (Trade.Symbol, Trade.Delta);

SELECT * FROM Stock

DROP TABLE Stock
DROP TABLE Trade

------------------------------------------------------------------------------------
/* Table Replication Example */

CREATE TABLE [Original](Id int PRIMARY KEY, Name varchar(10), Number int)
CREATE TABLE [Replica](Id int PRIMARY KEY, Name varchar(10), Number int)
GO

CREATE PROCEDURE uspSyncReplica AS

 MERGE [Replica] AS r
  USING [Original] AS o ON o.Id = r.Id
  WHEN MATCHED AND (o.Name != r.Name OR o.Number != r.Number) THEN
    UPDATE SET r.Name = o.Name, r.Number = o.Number
  WHEN NOT MATCHED THEN
    INSERT VALUES(o.Id, o.Name, o.Number)
  WHEN NOT MATCHED BY SOURCE THEN
    DELETE
 OUTPUT $action, inserted.*, deleted.*;

GO

SELECT * FROM [Original]
SELECT * FROM [Replica]

-- Add two rows
INSERT [Original] VALUES(1, 'Sara', 10)
INSERT [Original] VALUES(2, 'Steven', 20)
GO

SELECT * FROM [Original]
SELECT * FROM [Replica]

EXEC uspSyncReplica
GO

SELECT * FROM [Original]
SELECT * FROM [Replica]

-- Mix of INSERT, UPDATE, and DELETE
INSERT INTO [Original] VALUES(3, 'Andrew', 100)
UPDATE [Original] SET Name = 'Stephen', Number += 10 WHERE Id = 2
DELETE FROM [Original] WHERE Id = 1
GO

SELECT * FROM [Original]
SELECT * FROM [Replica]

EXEC uspSyncReplica

SELECT * FROM [Original]
SELECT * FROM [Replica]

DROP PROC uspSyncReplica
DROP TABLE [Original]
DROP TABLE [Replica]
