module ``Post transaction should``

open Payments.WebApi.IntegrationTests
open Payments.WebApi.TransactionPost
open Xunit
open Payments.WebApi.TransactionInitialize
open System
open Payments.WebApi.IntegrationTests.TestHelpers
open FsUnit.Xunit
open System.Net
open TransactionHelper

let validTransaction transactionId =
    { TransactionId = transactionId
      CustomerId = Guid.NewGuid()
      Amount = 123m
      StartedAt = DateTime.Now.AddDays(-1) }

let initializeAndAcknowledgeTransactionWithProvider transaction =
    let response =
        createPostRequest "/transactions/initialize" transaction |> testRequest
    response.StatusCode |> should equal HttpStatusCode.OK

[<Fact>]
let ``post success to acknowledged with provider transaction`` () =
    let transaction = Guid.NewGuid() |> validTransaction
    initializeAndAcknowledgeTransactionWithProvider transaction

    let postTransactionDto: PostTransactionDto =
        { TransactionId = transaction.TransactionId
          Succeeded = true }

    let response =
        createPostRequest "/transactions/post" postTransactionDto |> testRequest

    response.StatusCode |> should equal HttpStatusCode.OK
    
    assertTransaction
        { TransactionId = transaction.TransactionId
          CustomerId = transaction.CustomerId
          StartedAt = transaction.StartedAt
          FinishedAt = None
          ProviderReference = None
          Amount = transaction.Amount
          Status = "Succeeded" }

[<Fact>]
let ``post failure to acknowledged with provider transaction`` () =
    let transaction = Guid.NewGuid() |> validTransaction
    initializeAndAcknowledgeTransactionWithProvider transaction

    let postTransactionDto: PostTransactionDto =
        { TransactionId = transaction.TransactionId
          Succeeded = false }

    let response =
        createPostRequest "/transactions/post" postTransactionDto |> testRequest

    response.StatusCode |> should equal HttpStatusCode.OK
    
    assertTransaction
        { TransactionId = transaction.TransactionId
          CustomerId = transaction.CustomerId
          StartedAt = transaction.StartedAt
          FinishedAt = None
          ProviderReference = None
          Amount = transaction.Amount
          Status = "Failed" }
