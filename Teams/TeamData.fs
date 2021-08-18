module Teams.TeamData

//#r "nuget: FSharp.Data"
//#r "nuget: dotenv.net"
open System
open Microsoft.FSharp.Collections
open FSharp.Data
open Teams.Rop

let rapidKey = Environment.GetEnvironmentVariable("RAPID_KEY")
let rapidHost = Environment.GetEnvironmentVariable("RAPID_HOST")

type Player =
    { SofaScoreId: int
      Name: string
      Position: string
      PreferredFoot: string }

type TeamPlayers = { Team: string; Players: Player [] }

[<Literal>]
let teamTemplate =
    __SOURCE_DIRECTORY__ + @"/Templates/arsenal.json"

type Team = JsonProvider<teamTemplate>


let headers =
    [ "x-rapidapi-key", rapidKey 
      "x-rapidapi-host",  rapidHost]

let GetTeamData (teamId: int) (name: string) =
    let url =
        Http.RequestString($"https://sofascore.p.rapidapi.com/teams/get-squad?teamId={teamId}", headers = headers)

    try
        let data = Team.Parse url

        let players =
            data.Players
            |> Seq.map
                (fun player ->
                    { Name = player.Player.Name
                      Position = player.Player.Position
                      SofaScoreId = player.Player.Id
                      PreferredFoot = player.Player.PreferredFoot })
            |> Array.ofSeq

        { Team = name; Players = players }
        |> Outcome.Success
    with
    | e -> Outcome.Failure ((" ", (e.Message.Split(" ") |> Array.rev).[0..15]) |> System.String.Join)

//let teamData = GetTeamData 6


