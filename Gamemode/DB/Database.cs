using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGalaxy;
using FPSMO.Entities;
using MCGalaxy.SQL;
using System.Data;

namespace FPSMO.DB
{
    internal static class FPSMODatabase
    {
        /*****************************
         * FIRST TIME INITIALIZATION *
         *****************************/
        #region Initialization
        public static void InitializeDatabase()
        {
            if (!Database.TableExists("Rounds"))
            {
                Database.CreateTable("Rounds", new ColumnDesc[]
                {
                    new ColumnDesc("RoundID", ColumnType.UInt32),
                    new ColumnDesc("Map", ColumnType.VarChar)
                });
            }

            if (!Database.TableExists("Results"))
            {
                Database.CreateTable("Results", new ColumnDesc[] {
                    new ColumnDesc("RoundID", ColumnType.UInt32),
                    new ColumnDesc("Team", ColumnType.VarChar),
                    new ColumnDesc("Player", ColumnType.VarChar),
                    new ColumnDesc("Kills", ColumnType.UInt32),
                    new ColumnDesc("Deaths", ColumnType.UInt32)
                });
            }


            Database.Execute(
@"CREATE VIEW IF NOT EXISTS PlayerStats AS
SELECT Player, SUM(Kills) as TotalKills, SUM(Deaths) as TotalDeaths
FROM Results
GROUP BY Player;"
            );

            Database.Execute(
@"CREATE VIEW IF NOT EXISTS TeamResults AS
SELECT RoundID, Team, SUM(Kills) AS TotalKills
FROM Results
GROUP BY RoundID, Team;"
            );

            Database.Execute(
@"CREATE VIEW IF NOT EXISTS WinningTeam AS
	SELECT RoundID,
	CASE
		WHEN WinnersCount > 1 THEN 'TIE'
		ELSE Team
	END AS Team
	FROM
	(
		SELECT _TeamResults.RoundID, COUNT(*) AS WinnersCount, _TeamResults.team AS team
		FROM
		(
			SELECT RoundID, MAX(TotalKills) as BestTeamKillsCount
			FROM TeamResults
			GROUP BY RoundID
		) _RoundInfo
		JOIN TeamResults _TeamResults ON _TeamResults.RoundID = _RoundInfo.RoundID
		WHERE _RoundInfo.BestTeamKillsCount = _TeamResults.TotalKills
		GROUP BY _RoundInfo.RoundID
	)
;"
            );

        }

        #endregion
        /******************
         * HELPER METHODS *
         ******************/
        /// <summary>
        /// Saves the playerData into the Rounds and Results tables 
        /// </summary>
        public static void SaveData(uint roundID, string level, List<PlayerData> data)
        {
            foreach (PlayerData pd in data)
            {
                Database.AddRow("Results", "RoundID, Team, Player, Kills, Deaths",
                                        roundID, pd.team, pd.name, pd.kills, pd.deaths);
            }

            Database.AddRow("Rounds", "RoundID, Map",
                                        roundID, level);
        }

        /// <summary>
        /// Reads the player statistics from the database
        /// </summary>
        public static PlayerStats LoadPlayerStats(Player p)
        {
            PlayerStats stats = new PlayerStats();

            string[] statsStringified = Database.GetRows("PlayerStats", "TotaKills, TotalDeaths", "WHERE PLAYER=@0", p.FullName)[0];

            int.TryParse(statsStringified[0], out stats.totalDeaths);
            int.TryParse(statsStringified[1], out stats.totalKills);

            return stats;
        }
    }
}
