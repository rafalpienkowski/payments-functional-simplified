module Payments.WebApi.Transaction

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Npgsql.FSharp
open Payments.WebApi.Database

type Transaction =
    { TransactionId: Guid
      CustomerId: Guid
      StartedAt: DateTime
      FinishedAt: DateTime option
      ProviderReference: string option
      Amount: decimal
      Status: string }

let getAllTransactions (connectionString: string) : Transaction list =
    connectionString
    |> Sql.connect
    |> Sql.query "SELECT * FROM transactions"
    |> Sql.execute (fun read ->
        { TransactionId = read.uuid "transaction_id"
          CustomerId = read.uuid "customer_id"
          StartedAt = read.dateTime "started_at"
          FinishedAt = read.dateTimeOrNone "finished_at"
          ProviderReference = read.stringOrNone "provider_reference"
          Amount = read.decimal "amount"
          Status = read.string "status" })

let readTransactions: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let transactions = getConnectionString ctx |> getAllTransactions
        json transactions next ctx