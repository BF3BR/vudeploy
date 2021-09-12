CREATE TABLE Players (
    Id            TEXT (64) PRIMARY KEY ON CONFLICT FAIL
                            UNIQUE ON CONFLICT FAIL
                            NOT NULL ON CONFLICT FAIL,
    ZeusId        TEXT (64) UNIQUE ON CONFLICT FAIL
                            NOT NULL ON CONFLICT FAIL,
    Name          TEXT (64),
    PreviousNames TEXT
);

CREATE TABLE Matches (
    MatchId   TEXT (64) PRIMARY KEY ON CONFLICT FAIL
                        UNIQUE ON CONFLICT FAIL
                        NOT NULL ON CONFLICT FAIL,
    ServerId  TEXT (64) NOT NULL ON CONFLICT FAIL,
    StartTime DATETIME  NOT NULL ON CONFLICT FAIL,
    EndTime   DATETIME  NOT NULL ON CONFLICT FAIL,
    Winners   TEXT      NOT NULL ON CONFLICT FAIL,
    Players   TEXT      NOT NULL ON CONFLICT FAIL
);

CREATE TABLE MatchStats (
    StatsId    TEXT (64) PRIMARY KEY ON CONFLICT FAIL
                         UNIQUE ON CONFLICT FAIL
                         NOT NULL ON CONFLICT FAIL,
    PlayerId   TEXT (64) REFERENCES Players (Id) MATCH [FULL]
                         NOT NULL ON CONFLICT FAIL,
    MatchId    TEXT (64) REFERENCES Matches (MatchId) MATCH [FULL]
                         NOT NULL ON CONFLICT FAIL,
    Damage     BIGINT    DEFAULT (0),
    Headshots  BIGINT    DEFAULT (0),
    Kills      BIGINT    DEFAULT (0),
    Deaths     BIGINT    DEFAULT (0),
    Knockdowns BIGINT    DEFAULT (0),
    Score      BIGINT    DEFAULT (0),
    Accuracy   DOUBLE    DEFAULT (0),
    TeamId     INTEGER   NOT NULL ON CONFLICT FAIL,
    SquadId    INTEGER   NOT NULL ON CONFLICT FAIL
);
