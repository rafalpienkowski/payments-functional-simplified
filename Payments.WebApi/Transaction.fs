module Payments.WebApi.Transaction

open System
open Giraffe
open Microsoft.AspNetCore.Http

type TransactionError =
    | Validation of string list
    | Provider of string list
    | Database of string list

type TransactionStatus =
    | Initialized = 1
    | Acknowledged = 2

let transactionStatusString status =
    match status with
    | TransactionStatus.Initialized -> "Initialized"
    | TransactionStatus.Acknowledged -> "Acknowledged"
    | _ -> ArgumentOutOfRangeException() |> raise

type TransactionId = private TransactionId of Guid
type CustomerId = private CustomerId of Guid
type Amount = private Amount of decimal
type StartDate = private StartDate of DateTime
type ProviderReference = private ProviderReference of string
type FinishDate = FinishDate of DateTime

type Transaction =
    { Id: TransactionId
      CustomerId: CustomerId
      StartedAt: StartDate
      FinishedAt: FinishDate option
      ProviderReference: ProviderReference
      Amount: Amount
      Status: TransactionStatus }

// Implicit dependency
type AcknowledgedWithProvider = TransactionId -> Amount -> Result<ProviderReference, string list>

module ConstrainedType =
    let createGuid fieldName ctor value =
        if value = Guid.Empty then
            Error $"%s{fieldName} invalid value"
        else
            Ok(ctor value)

    let createNotEmptyString fieldName ctor value =
        if String.IsNullOrEmpty(value) then
            Error $"%s{fieldName} can not be null or empty"
        else
            Ok(ctor value)

    let createPastDateTime fieldName ctor value =
        if value > DateTime.UtcNow then
            Error $"%s{fieldName} can not be from the future"
        else
            Ok(ctor value)

module TransactionId =
    let value (TransactionId transactionId) = transactionId

    let from value =
        ConstrainedType.createGuid "TransactionId" TransactionId value

module Amount =
    let value (Amount amount) = amount

    let from value =
        if value <= 0m then
            Error "Amount must be greater than 0"
        else
            Ok(Amount value)

module CustomerId =
    let value (CustomerId customerId) = customerId

    let from value =
        ConstrainedType.createGuid "CustomerId" CustomerId value

module StartDate =
    let value (StartDate startDate) = startDate

    let from value =
        ConstrainedType.createPastDateTime "StartDate" StartDate value

module ProviderReference =
    let newValue = ProviderReference(Guid.NewGuid().ToString("N"))

    let from value =
        ConstrainedType.createNotEmptyString "ProviderReference" ProviderReference value

    let value (ProviderReference providerReference) = providerReference


let produceErrorResponse (error: TransactionError) (next: HttpFunc) (ctx: HttpContext) : HttpFuncResult =
    match error with
    | TransactionError.Validation parsingError -> RequestErrors.badRequest (json parsingError) next ctx
    | TransactionError.Provider _ -> ServerErrors.serviceUnavailable (json "Ups something went wrong") next ctx
    | TransactionError.Database _ -> ServerErrors.internalError (json "Database unavailable") next ctx

let produceResponse operationResult next ctx : HttpFuncResult =
    match operationResult with
    | Ok _ -> json {| status = "Accepted" |} next ctx
    | Error errorValue -> produceErrorResponse errorValue next ctx
