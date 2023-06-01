module Payments.WebApi.TransactionPost

open System
open Npgsql
open Payments.WebApi.Transaction
open Payments.WebApi.Database
open Giraffe
open FsToolkit.ErrorHandling
open Microsoft.AspNetCore.Http

type PostTransactionDto =
    { TransactionId: Guid; Succeeded: bool }

let newTransactionStatus succeeded =
    if succeeded then
        TransactionStatus.Succeeded
    else
        TransactionStatus.Failed

let isTransactionAllowedToPost transaction =
    if transaction.Status <> TransactionStatus.Acknowledged then
        Error([ "Transaction is not acknowledged with provider" ])
    else
        Ok()

let postTransaction (connectionString: string) (dto: PostTransactionDto) =

    result {
        let! transactionId =
            TransactionId.from dto.TransactionId
            |> convertResultStringToList
            |> Result.mapError TransactionError.Validation

        use connection = new NpgsqlConnection(connectionString)
        connection.Open()

        use transaction = connection.BeginTransaction()

        let! processingTransaction =
            getTransactionById connection transactionId
            |> Result.mapError TransactionError.Database

        isTransactionAllowedToPost processingTransaction
        |> Result.mapError TransactionError.Validation
        |> ignore

        let updateResult =
            updateTransactionStatusById
                connection
                processingTransaction.TransactionId
                (dto.Succeeded |> newTransactionStatus)

        transaction.Commit()

        return! updateResult
    }


let postTransactionHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {

            let! request = ctx.BindJsonAsync<PostTransactionDto>()
            let connectionString = getConnectionString ctx
            let operationResult = postTransaction connectionString request

            return! produceResponse operationResult next ctx
        }
