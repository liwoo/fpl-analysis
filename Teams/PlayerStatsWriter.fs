module Teams.PlayerStatsWriter

open System
open System.IO
open FSharp.Data
open Teams.PlayerStatistics
open Teams.Rop

[<Literal>]
let statsMasterTemplate =
    __SOURCE_DIRECTORY__
    + @"/Templates/player_stats_master.csv"

let path =
    __SOURCE_DIRECTORY__
    + @"/Data/player_master_stats.csv"

let transformPath =
    __SOURCE_DIRECTORY__
    + @"/Data/player_stats_master.csv"

type PlayerStatsMaster = CsvProvider<statsMasterTemplate, Schema=",,,,,,,,,,,,,,,,,,,,,,,,,,,,float">

let master = PlayerStatsMaster.Load path
let writer = new StreamWriter(transformPath)

type PlayerStat = { PlayerId: int; Stats: PlayerStats }

let skip = 250
let truncate = 50

let stats =
    master.Rows
    |> Seq.skip skip
    |> Seq.truncate truncate
    |> Seq.map
        (fun player ->
            { PlayerId = player.Id
              Stats = GetPlayerStats player.Id player.``Season Metadata`` })
    |> List.ofSeq

let FetchStat (playerId: int) =
    (stats
     |> List.find (fun stat -> stat.PlayerId = playerId))
        .Stats

master
    .Skip(skip)
    .Truncate(truncate)
    .Map(fun row ->
        PlayerStatsMaster.Row(
            row.Id,
            row.Name,
            row.Position,
            row.``Preferred Foot``,
            row.Team,
            row.``FPL Price``,
            row.``FPL Position``,
            row.``Season Metadata``,
            (FetchStat row.Id).MinutesPlayed,
            (FetchStat row.Id).SavesMade,
            (FetchStat row.Id).PenaltiesSaved,
            (FetchStat row.Id).CleanSheets,
            (FetchStat row.Id).BlocksMade,
            (FetchStat row.Id).TacklesWon,
            (FetchStat row.Id).DribbledPast,
            (FetchStat row.Id).SuccessfulTakeons,
            (FetchStat row.Id).Assists,
            (FetchStat row.Id).GoalsScored,
            (FetchStat row.Id).BigMisses,
            (FetchStat row.Id).WoodworkHits,
            (FetchStat row.Id).BigChancesCreated,
            (FetchStat row.Id).PenaltiesScored,
            (FetchStat row.Id).PenaltiesMissed,
            (FetchStat row.Id).YellowCards,
            (FetchStat row.Id).RedCards,
            (FetchStat row.Id).MatchesStarted,
            (FetchStat row.Id).XG,
            (FetchStat row.Id).XA,
            (FetchStat row.Id).ShotConversion / float (row.``Season Metadata``.Split("_").Length)
        ))
    .Save(writer)
