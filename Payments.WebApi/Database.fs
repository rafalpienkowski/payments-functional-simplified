module Payments.WebApi.Database

open Npgsql.FSharp
open Payments.WebApi.Transaction
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

let getConnectionString (ctx: HttpContext) =
    let config = ctx.GetService<IConfiguration>()
    config.GetConnectionString "Database"

let checkIfQuerySucceeded rowsAffected =
    if rowsAffected = 0 then
        Error ["Database unavailable"]
    else
        Ok rowsAffected

let createTransaction (ctx: HttpContext) (transaction: Transaction): Result<int, string list> =
    try
        getConnectionString ctx
            |> Sql.connect
            |> Sql.query
                "INSERT INTO transactions(transaction_id, customer_id, amount, started_at, provider_reference, status)
                VALUES (@TransactionId, @CustomerId, @Amount, @StartedAt, @ProviderReference, @Status)"
            |> Sql.parameters
                [ "@TransactionId", Sql.uuid (TransactionId.value transaction.Id)
                  "@CustomerId", Sql.uuid (CustomerId.value transaction.CustomerId)
                  "@Amount", Sql.decimal (Amount.value transaction.Amount)
                  "@StartedAt", Sql.timestamp (StartDate.value transaction.StartedAt)
                  "@ProviderReference", Sql.string (ProviderReference.value transaction.ProviderReference)
                  "@Status", Sql.string (transactionStatusString TransactionStatus.Acknowledged) ]
            |> Sql.executeNonQuery
            |> checkIfQuerySucceeded
            
    with ex -> Error([ex.Message])
