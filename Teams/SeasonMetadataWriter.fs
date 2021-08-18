module Teams.SeasonMetadataWriter

open System.IO
open FSharp.Data
open Teams.SeasonMetadata
open Teams.Rop

[<Literal>]
let masterTemplate =
    __SOURCE_DIRECTORY__
    + @"/Templates/player_master.csv"

let path =
    __SOURCE_DIRECTORY__ + @"/Data/player_master.csv"

let transformPath =
    __SOURCE_DIRECTORY__
    + @"/Data/player_master_metadata.csv"

type PlayerMaster = CsvProvider<masterTemplate>

let master = PlayerMaster.Load path
let writer = new StreamWriter(transformPath)


master
    .Map(fun row ->
        PlayerMaster.Row(
            row.Id,
            row.Name,
            row.Position,
            row.``Preferred Foot``,
            row.Team,
            row.``FPL Price``,
            row.``FPL Position``,
            match GetSeasonMetadata row.Id with
            | Success (metadata) -> metadata
            | _ -> "NOT_FOUND"
        ))
    .Save(writer)
