using System;
using System.Collections.Generic;

namespace FPS.DB
{
	internal static class Query
	{
		internal static Dictionary<string, string> Create = new Dictionary<string, string>
		{
			{
				"FPS_MapData",
@"CREATE TABLE FPS_MapData (
	name VARCHAR(256) PRIMARY KEY NOT NULL,
	countdown_duration_seconds INTEGER,
	round_duration_seconds INTEGER
);"
			},
			{
				"FPS_MapPool",
@"CREATE TABLE FPS_MapPool (
	map_name VARCHAR(256) PRIMARY KEY
);"
            },
			{
				"FPS_Round",
@"CREATE TABLE FPS_Round (
	id VARCHAR(256) PRIMARY KEY,
	map_name VARCHAR(256)
);"
            },
			{
				"FPS_SpawnPoint",
@"CREATE TABLE FPS_SpawnPoint (
	map_name VARCHAR(256),
	x INTEGER,
	y INTEGER,
	z INTEGER,
	team VARCHAR(4),
	PRIMARY KEY (map_name, x, y, z)
);"
            },
			{
				"FPS_Player",
@"CREATE TABLE FPS_Player (
	name VARCHAR(64) PRIMARY KEY,
	xp INTEGER,
	level INTEGER,
	favourite_map_name VARCHAR(256)
);"
            },
			{
				"FPS_Rating",
@"CREATE TABLE FPS_Rating (
	player_name VARCHAR(64),
	map_name VARCHAR(256),
	rating INTEGER,
	PRIMARY KEY (player_name, map_name)
);"
            },
			{
				"FPS_PlayerAchievement",
@"CREATE TABLE FPS_PlayerAchievement (
	player_name VARCHAR(64),
	achievement_name VARCHAR(64),
	PRIMARY KEY (player_name, achievement_name)
);"
			},
			{
				"FPS_Death",
@"CREATE TABLE FPS_Death (
	id INTEGER PRIMARY KEY,
	round_id INTEGER NOT NULL,
	victim VARCHAR(64) NOT NULL,
	killer VARCHAR(64),
	reason VARCHAR(16)
);"
            }
		};
    }
}

