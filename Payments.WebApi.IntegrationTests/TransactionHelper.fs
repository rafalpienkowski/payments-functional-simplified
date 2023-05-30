module Payments.WebApi.IntegrationTests.TransactionHelper

open System.Net.Http
open System.Text.Json
open TestHelpers
open FsUnit.Xunit
open Payments.WebApi.TransactionView

let assertTransaction (expectedTransaction: TransactionView) : unit =
    let httpRequest = new HttpRequestMessage(HttpMethod.Get, "/transactions")
    let response = testRequest httpRequest
    let content = response.Content.ReadAsStringAsync().Result

    let transaction =
        JsonSerializer.Deserialize<TransactionView list> content
        |> List.find (fun t -> t.TransactionId = expectedTransaction.TransactionId)

    transaction.Status |> should equal expectedTransaction.Status
    transaction.Amount |> should equal expectedTransaction.Amount
    transaction.CustomerId |> should equal expectedTransaction.CustomerId
    transaction.StartedAt |> should equal expectedTransaction.StartedAt