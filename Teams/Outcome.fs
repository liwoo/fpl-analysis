namespace Teams.Rop

type Outcome<'S> =
    | Success of result: 'S
    | Failure of message: string