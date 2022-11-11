/*
Copyright 2022 WOCC Team

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using MCGalaxy.Tasks;
using MCGalaxy.Events;
using FPSMO;
using MCGalaxy.Events.PlayerEvents;
using System;
using MCGalaxy.Network;
using MCGalaxy.SQL;
using FPSMO.Commands;
using FPSMO.Weapons;

namespace MCGalaxy
{
    public class FPSMO : Plugin
    {
        FPSMOGame game = FPSMOGame.Instance;

        public override string creator { get { return "Opapinguin, D_Flat, Razorboot, Panda"; } }

        public override string name { get { return "FPSMO"; } }

        public override string MCGalaxy_Version { get { return "1.9.4.0"; } }

        public override void Load(bool startup)
        {
            /************
             * DATABASE *
             ************/
            #region Database
            if (!Database.TableExists("Rounds"))
            {
                Database.CreateTable("Rounds", new ColumnDesc[]
                {
                    new ColumnDesc("ID", ColumnType.UInt32),
                    new ColumnDesc("Map", ColumnType.VarChar)
                });
            }

            if (!Database.TableExists("Results"))
            {
                Database.CreateTable("Results", new ColumnDesc[] {
                    new ColumnDesc("ID", ColumnType.UInt32),
                    new ColumnDesc("RoundID", ColumnType.UInt32),
                    new ColumnDesc("Team", ColumnType.VarChar),
                    new ColumnDesc("Player", ColumnType.VarChar),
                    new ColumnDesc("Kills", ColumnType.UInt8),
                    new ColumnDesc("Deaths", ColumnType.UInt8)
                });
            }

            /********************
             * DATABASE INDEXES *
             ********************/
            // TODO : Implement

            /******************
             * DATABASE VIEWS *
             ******************/
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

            #endregion
            /***********
             * COMMANDS *
             ************/
            #region Commands
            Command.Register(new CmdQueue());
            Command.Register(new CmdShootGun());

            #endregion Commands

            game.Start();   // TODO: Make autostart optional?
        }

        public override void Unload(bool shutdown)
        {
            Command.Unregister(Command.Find("Queue"));
            Command.Unregister(Command.Find("FPSMOShootGun"));

            game.Stop();
        }
    }

}
