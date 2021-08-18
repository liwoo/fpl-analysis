module Teams.PlayerWriter

open System.IO
open Teams.TeamData
open Teams.Rop
open FSharp.Data

type Team = { Name: string; SofaScoreId: int }

let teams =
    [ { Name = "Everton"; SofaScoreId = 48 } ]

[<Literal>]
let sample = "1, 'Liwu', 'F', 'Left', 'Chinyonga'"

type Players =
    CsvProvider<Schema="SofaScoreId (int), Name (string), Position (string), PreferredFoot (string), Team (string)", Sample=sample, HasHeaders=true>

let path =
    __SOURCE_DIRECTORY__ + @"/Data/sample.csv"

let writeTeamPlayers (team: TeamPlayers) =
    let writer = new StreamWriter(path, true)
    let teamName = team.Team
    printfn $"saving players from %s{teamName}..."

    let rows =
        team.Players
        |> Seq.map
            (fun player ->
                Players.Row(player.SofaScoreId, player.Name, player.Position, player.PreferredFoot, teamName))
        |> Array.ofSeq

    let playerCsv = new Players(rows)
    playerCsv.Save(writer)



for team in teams do
    let teamData = GetTeamData team.SofaScoreId team.Name

    match teamData with
    | Success (team) -> writeTeamPlayers team
    | _ -> printfn "Whoops"
    
