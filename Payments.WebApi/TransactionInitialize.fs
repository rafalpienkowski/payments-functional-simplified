module Payments.WebApi.TransactionInitialize

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Payments.WebApi.Transaction
open FsToolkit.ErrorHandling
open Payments.WebApi.Database

type InitializeTransactionDto =
    { TransactionId: Guid
      CustomerId: Guid
      Amount: decimal
      StartedAt: DateTime }

type private StartTransaction =
    { TransactionId: TransactionId
      Amount: Amount
      CustomerId: CustomerId
      StartedAt: StartDate }

let private validateDto (dto: InitializeTransactionDto) =
    validation {
        let! transactionId = TransactionId.from dto.TransactionId
        and! customerId = CustomerId.from dto.CustomerId
        and! amount = Amount.from dto.Amount
        and! startedAt = StartDate.from dto.StartedAt

        return
            { TransactionId = transactionId
              CustomerId = customerId
              Amount = amount
              StartedAt = startedAt }
    }

let startTransaction (acknowledgeWithProvider: AcknowledgedWithProvider) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! request = ctx.BindJsonAsync<InitializeTransactionDto>()

            let operationResult =
                result {
                    let! startTransaction = validateDto request |> Result.mapError TransactionError.Validation

                    let! reference =
                        acknowledgeWithProvider startTransaction.TransactionId startTransaction.Amount
                        |> Result.mapError TransactionError.Provider

                    let transaction =
                        { Id = startTransaction.TransactionId
                          CustomerId = startTransaction.CustomerId
                          Amount = startTransaction.Amount
                          StartedAt = startTransaction.StartedAt
                          ProviderReference = reference
                          Status = TransactionStatus.Acknowledged
                          FinishedAt = None }

                    return! createTransaction ctx transaction
                    
                }

            return! produceResponse operationResult next ctx
        }
