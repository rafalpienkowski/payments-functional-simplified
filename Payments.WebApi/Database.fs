module Payments.WebApi.Database

open Npgsql
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
        Error(TransactionError.Database["Database unavailable"])
    else
        Ok rowsAffected

let createTransaction (ctx: HttpContext) (transaction: Transaction) : Result<int, TransactionError> =
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
              "@Status", Sql.string (enumToString TransactionStatus.Acknowledged) ]
        |> Sql.executeNonQuery
        |> checkIfQuerySucceeded

    with
    | :? NpgsqlException as npgsqlex ->
        if npgsqlex.SqlState = "23505" then
            Error(TransactionError.Validation([ "Transaction already exists" ]))
        else
            Error(TransactionError.Database([ npgsqlex.Message ]))
    | ex -> Error(TransactionError.Database([ ex.Message ]))

let normalizeTransaction (transactions: ProcessingTransaction list) : Result<ProcessingTransaction, string list> =
    if transactions.Length = 0 then
        Error( ["There is no transaction in the database" ])
    elif transactions.Length > 1 then
        Error([ "There is more than one transaction in the database" ])
    else
        Ok(transactions.Head)

let rec getTransactionById
    (connection: NpgsqlConnection)
    (id: TransactionId)
    : Result<ProcessingTransaction, string list> =
        try
            connection
            |> Sql.existingConnection
            |> Sql.query "SELECT * FROM transactions WHERE transaction_id = @transactionId"
            |> Sql.parameters [ "transactionId", Sql.uuid (TransactionId.value id) ]
            |> Sql.execute (fun read ->
                { TransactionId = read.uuid "transaction_id" |> TransactionId.fromDatabase
                  Status = read.string "status" |> stringToEnum<TransactionStatus> })
            |> normalizeTransaction
        with
        | ex -> Error([ ex.Message ])

let rec updateTransactionStatusById
    (connection: NpgsqlConnection)
    (transactionId: TransactionId)
    (status: TransactionStatus) =
        try
            connection
                |> Sql.existingConnection
                |> Sql.query "UPDATE transactions SET status = @status WHERE transaction_id = @transactionId"
                |> Sql.parameters
                    [ "status", Sql.string (status |> enumToString)
                      "transactionId", Sql.uuid (TransactionId.value transactionId) ]
            |> Sql.executeNonQuery
            |> checkIfQuerySucceeded
        with
        | ex -> Error(TransactionError.Database([ ex.Message ]))