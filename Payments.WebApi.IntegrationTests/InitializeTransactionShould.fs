module ``initialize transaction should``

open Xunit
open Payments.WebApi.TransactionInitialize
open System
open Payments.WebApi.IntegrationTests.TestHelpers
open FsUnit.Xunit
open System.Net

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