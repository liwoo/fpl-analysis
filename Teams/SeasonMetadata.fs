module Teams.SeasonMetadata

//#r "nuget: FSharp.Data"
//#r "nuget: dotenv.net"
//#load "Outcome.fs"

open System
open Microsoft.FSharp.Collections
open FSharp.Data
open Teams.Rop
open dotenv.net

//DotEnv.Load()

let rapidKey =
    Environment.GetEnvironmentVariable("RAPID_KEY")

let rapidHost =
    Environment.GetEnvironmentVariable("RAPID_HOST")

[<Literal>]
let seasonMetadataTemplate =
    __SOURCE_DIRECTORY__
    + @"/Templates/player_season.json"

type SeasonMetadata = JsonProvider<seasonMetadataTemplate>

let headers =
    [ "x-rapidapi-key", rapidKey
      "x-rapidapi-host", rapidHost ]


type TournamentSeason =
    { TournamentId: int
      SeasonId: int
      SeasonYear: string }

let GetSeasonMetadata (playerId: int) =
    printfn $"Fetching season metadata for player %i{playerId}"
    let url =
        Http.RequestString(
            $"https://sofascore.p.rapidapi.com/players/get-statistics-seasons?playerId={playerId}",
            headers = headers
        )

    try
        let data = SeasonMetadata.Parse url
        let validSeasons = [ "20/21"; "19/20"; "18/19" ]

        let validTournaments =
            [ 17
              18
              35
              262
              34
              45
              8
              238
              35
              36
              20
              39
              40
              325
              172
              203
              215 ]

        let Meta =
            data.UniqueTournamentSeasons
            |> Seq.map
                (fun meta ->
                    meta.Seasons
                    |> Seq.map
                        (fun season ->
                            { TournamentId = meta.UniqueTournament.Id
                              SeasonId = season.Id
                              SeasonYear =
                                  match season.Year.String with
                                  | Some (year) -> year
                                  | _ -> "UNKNOWN" })

                    )
            |> Seq.reduce Seq.append
            |> Array.ofSeq
            |> Array.filter (fun meta -> validSeasons |> List.contains meta.SeasonYear)
            |> Array.filter
                (fun meta ->
                    validTournaments
                    |> List.contains meta.TournamentId)
            |> Array.map (fun meta -> $"%i{meta.TournamentId}-%i{meta.SeasonId}")
            |> String.concat "_"

        Meta |> Outcome.Success

    with
    | e -> Outcome.Failure e.Message
