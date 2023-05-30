module ``initialize transaction should``

open Payments.WebApi.IntegrationTests
open Xunit
open Payments.WebApi.TransactionInitialize
open System
open Payments.WebApi.IntegrationTests.TestHelpers
open FsUnit.Xunit
open System.Net
open TransactionHelper

let transactionWithNegativeAmount =
    { TransactionId = Guid.NewGuid()
      CustomerId = Guid.NewGuid()
      Amount = -1m
      StartedAt = DateTime.Now }

let transactionFromFuture =
    { TransactionId = Guid.NewGuid()
      CustomerId = Guid.NewGuid()
      Amount = 1m
      StartedAt = DateTime.Now.AddDays(10) }

let transactionFromFutureWithNegativeAmount =
    { TransactionId = Guid.NewGuid()
      CustomerId = Guid.NewGuid()
      Amount = -1m
      StartedAt = DateTime.Now.AddDays(10) }

let invalidInitializeRequests: obj[] list =
    [ [| transactionWithNegativeAmount |]
      [| transactionFromFuture |]
      [| transactionFromFutureWithNegativeAmount |] ]    
    
[<Theory>]
[<MemberData(nameof invalidInitializeRequests)>]
let ``reject invalid request`` request =
    let response = createPostRequest "/transactions/initialize" request
                      |> testRequest
    response.StatusCode |> should equal HttpStatusCode.BadRequest    
    
let validTransactionThatCausesInfrastructureError =
    { TransactionId = Guid.NewGuid()
      CustomerId = Guid.NewGuid()
      Amount = 1m
      StartedAt = DateTime.Now.AddDays(-1) }

[<Fact>]
let ``accept valid initialize transaction request and handle infrastructure error while calling provider`` () =
    let response =
        createPostRequest "/transactions/initialize" validTransactionThatCausesInfrastructureError
        |> testRequest

    response.StatusCode |> should equal HttpStatusCode.GatewayTimeout
        
let validTransaction =
    { TransactionId = Guid.NewGuid()
      CustomerId = Guid.NewGuid()
      Amount = 123m
      StartedAt = DateTime.Now.AddDays(-1) }

[<Fact>]
let ``accept valid initialize transaction request and successfully acknowledge it with provider`` () =
    let response = createPostRequest "/transactions/initialize" validTransaction
                    |> testRequest
                    
    response.StatusCode |> should equal HttpStatusCode.OK

    assertTransaction
        { TransactionId = validTransaction.TransactionId
          CustomerId = validTransaction.CustomerId
          StartedAt = validTransaction.StartedAt
          FinishedAt = None
          ProviderReference = None
          Amount = validTransaction.Amount
          Status = "Acknowledged" }