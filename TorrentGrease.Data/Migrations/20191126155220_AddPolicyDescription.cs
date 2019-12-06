using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace TorrentGrease.Data.Migrations
{
    public partial class AddPolicyDescription : Migration
    {
        private const string _UpdatePolicyScript = @"

PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT Id, Name, Name || ' description' as Description, [Order], [Enabled]
                                          FROM Policy;

DROP TABLE Policy;

CREATE TABLE Policy (
    Id          INTEGER    NOT NULL
                           CONSTRAINT PK_Policy PRIMARY KEY AUTOINCREMENT,
    Name        TEXT (50)  NOT NULL UNIQUE,
    Description TEXT (300) NOT NULL,
    [Enabled]   INTEGER    NOT NULL,
    [Order]     INTEGER    NOT NULL
);

INSERT INTO Policy (
                       Id,
                       Name,
                       Description,
                       [Enabled],
                       [Order]
                   )
                   SELECT Id,
                          Name,
                          Description,
                          [Enabled],
                          [Order]
                     FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

PRAGMA foreign_keys = 1;

";

        private const string _UpdateTrackerScript = @"
PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM Tracker;

DROP TABLE Tracker;

CREATE TABLE Tracker (
    Id   INTEGER   NOT NULL
                   CONSTRAINT PK_Tracker PRIMARY KEY AUTOINCREMENT,
    Name TEXT (50) NOT NULL
                   UNIQUE
);

INSERT INTO Tracker (
                        Id,
                        Name
                    )
                    SELECT Id,
                           Name
                      FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

PRAGMA foreign_keys = 1;
";

        private const string _UpdateActionScript = @"
PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT *
                                          FROM [Action];

DROP TABLE [Action];

CREATE TABLE [Action] (
    Id            INTEGER   NOT NULL
                            CONSTRAINT PK_Action PRIMARY KEY AUTOINCREMENT,
    Name          TEXT (50) NOT NULL UNIQUE,
    [Order]       INTEGER   NOT NULL,
    ActionType    TEXT      NOT NULL,
    Configuration TEXT      NOT NULL,
    PolicyId      INTEGER   NOT NULL,
    CONSTRAINT FK_Action_Policy_PolicyId FOREIGN KEY (
        PolicyId
    )
    REFERENCES Policy (Id) ON DELETE RESTRICT
);

INSERT INTO [Action] (
                         Id,
                         Name,
                         [Order],
                         ActionType,
                         Configuration,
                         PolicyId
                     )
                     SELECT Id,
                            Name,
                            [Order],
                            ActionType,
                            Configuration,
                            PolicyId
                       FROM sqlitestudio_temp_table;

DROP TABLE sqlitestudio_temp_table;

CREATE INDEX IX_Action_PolicyId ON[Action] (
    [PolicyId]
);

CREATE UNIQUE INDEX IX_Action_Order ON[Action] (
    [Order]
);

PRAGMA foreign_keys = 1;
";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add column description and limit Name column to 50length and unique
            migrationBuilder.Sql(_UpdatePolicyScript);

            //Drop trackerUrls column for now and add UC on Name
            migrationBuilder.Sql(_UpdateTrackerScript);

            //limit Name column to 50length and unique
            migrationBuilder.Sql(_UpdateActionScript);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException();
        }
    }
}
