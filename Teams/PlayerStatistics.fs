module Teams.PlayerStatistics

#r "nuget: FSharp.Data"
#r "nuget: dotenv.net"
#load "Outcome.fs"

open System
open Microsoft.FSharp.Collections
open FSharp.Data
open Teams.Rop
open dotenv.net

DotEnv.Load()

let rapidKey =
    Environment.GetEnvironmentVariable("RAPID_KEY")

let rapidHost =
    Environment.GetEnvironmentVariable("RAPID_HOST")

[<Literal>]
let playerStatsTemplate =
    __SOURCE_DIRECTORY__
    + @"/Templates/player_stats.json"

type SeasonMetadata = JsonProvider<playerStatsTemplate>

let headers =
    [ "x-rapidapi-key", rapidKey
      "x-rapidapi-host", rapidHost ]

type PlayerStats =
    { MinutesPlayed: int
      SavesMade: int
      PenaltiesSaved: int
      CleanSheets: int
      BlocksMade: int
      TacklesWon: int
      DribbledPast: int
      SuccessfulTakeons: int
      Assists: int
      GoalsScored: int
      BigMisses: int
      WoodworkHits: int
      BigChancesCreated: int
      PenaltiesScored: int
      PenaltiesMissed: int
      YellowCards: int
      RedCards: int
      MatchesStarted: int
      XG: int
      XA: int
      ShotConversion: float }

type TournamentSeasonParams =
    { TournamentId: int
      SeasonId: int
      PlayerId: int }

let CreateParams (playerId: int) (seasonMetadata: string) =
    try
        seasonMetadata.Split("_")
        |> Array.map
            (fun seasons ->
                { TournamentId = seasons.Split("-").[0] |> int
                  SeasonId = seasons.Split("-").[1] |> int
                  PlayerId = playerId })
    with
    | e -> [{ TournamentId = 0; SeasonId = 0; PlayerId = 0 }] |> Array.ofList

let LogError (e: string) (parameters: TournamentSeasonParams) =
    printfn
        $"Failed to fetch stats for player %i{parameters.PlayerId} for Tournament %i{parameters.TournamentId} and Season %i{parameters.SeasonId}"

    e

let GetSeasonStats (parameters: TournamentSeasonParams) =
    let url =
        Http.RequestString(
            $"https://sofascore.p.rapidapi.com/players/get-statistics?playerId=%i{parameters.PlayerId}&tournamentId=%i{parameters.TournamentId}&seasonId=%i{parameters.SeasonId}&type=overall",
            headers = headers
        )
        
    try
        let data = SeasonMetadata.Parse url
        let stats = data.Statistics

        { MinutesPlayed = stats.MinutesPlayed
          SavesMade = stats.Saves
          PenaltiesSaved = stats.PenaltySave
          CleanSheets = stats.CleanSheet
          BlocksMade = stats.BlockedShots
          TacklesWon = stats.TacklesWon
          DribbledPast = stats.DribbledPast
          SuccessfulTakeons = stats.SuccessfulDribbles
          Assists = stats.Assists
          GoalsScored = stats.Goals
          WoodworkHits = stats.HitWoodwork
          BigChancesCreated = stats.KeyPasses
          PenaltiesScored = stats.PenaltyGoals
          PenaltiesMissed =
              stats.AttemptPenaltyMiss
              + stats.AttemptPenaltyPost
          BigMisses = stats.BigChancesMissed
          XG =
              (stats.ShotsFromInsideTheBox + stats.ShotsOnTarget)
              / 2
          YellowCards = stats.YellowCards
          RedCards = stats.RedCards
          MatchesStarted = stats.MatchesStarted
          XA =
              (stats.BigChancesCreated + stats.AccurateCrosses)
              / 2
          ShotConversion = (float stats.Goals / float stats.ShotsOnTarget) }
        |> Outcome.Success

    with
    | e -> Outcome.Failure(LogError e.Message parameters)

let emptyStats =
    { MinutesPlayed = 0
      SavesMade = 0
      PenaltiesSaved = 0
      CleanSheets = 0
      BlocksMade = 0
      TacklesWon = 0
      DribbledPast = 0
      SuccessfulTakeons = 0
      Assists = 0
      GoalsScored = 0
      WoodworkHits = 0
      BigChancesCreated = 0
      PenaltiesScored = 0
      PenaltiesMissed = 0
      BigMisses = 0
      XG = 0
      YellowCards = 0
      RedCards = 0
      MatchesStarted = 0
      XA = 0
      ShotConversion = 0.0 }

let GetSeasonStatsSafe (parameters: TournamentSeasonParams) =
    match GetSeasonStats parameters with
    | Success (stats) -> stats
    | _ -> emptyStats

let TotalStatsReducer (state: PlayerStats) (item: PlayerStats) =
    { state with
          MinutesPlayed = state.MinutesPlayed + item.MinutesPlayed
          SavesMade = state.SavesMade + item.SavesMade
          PenaltiesSaved = state.PenaltiesSaved + item.PenaltiesSaved
          CleanSheets = state.CleanSheets + item.CleanSheets
          BlocksMade = state.BlocksMade + item.BlocksMade
          TacklesWon = state.TacklesWon + item.TacklesWon
          DribbledPast = state.DribbledPast + item.DribbledPast
          SuccessfulTakeons = state.SuccessfulTakeons + item.SuccessfulTakeons
          Assists = state.Assists + item.Assists
          GoalsScored = state.GoalsScored + item.GoalsScored
          WoodworkHits = state.WoodworkHits + item.WoodworkHits
          BigChancesCreated = state.BigChancesCreated + item.BigChancesCreated
          PenaltiesScored = state.PenaltiesScored + item.PenaltiesScored
          PenaltiesMissed = state.PenaltiesMissed + item.PenaltiesMissed
          BigMisses = state.BigMisses + item.BigMisses
          XG = state.XG + item.XG
          YellowCards = state.YellowCards + item.YellowCards
          RedCards = state.RedCards + item.RedCards
          MatchesStarted = state.MatchesStarted + item.MatchesStarted
          XA = state.XA + item.XA
          ShotConversion = state.ShotConversion + item.ShotConversion }

let GetPlayerStats (playerId: int) (seasonMetadata: string) =
    let seasonParams = CreateParams playerId seasonMetadata
    printfn $"Starting fetching stats for %i{playerId}..."

    seasonParams
    |> Array.map (GetSeasonStatsSafe)
    |> Array.fold (TotalStatsReducer) emptyStats


// GetPlayerStats 316148 17-29415_8-32501_8-24127
