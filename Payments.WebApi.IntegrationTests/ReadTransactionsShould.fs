module ``read transactions should``

open System.Net
open System.Net.Http
open Xunit
open FsUnit.Xunit
open Payments.WebApi.IntegrationTests.TestHelpers


[<Fact>]
let ``return transactions`` () =
    let response = testRequest (new HttpRequestMessage(HttpMethod.Get, "/transactions"))
    response.StatusCode |> should equal HttpStatusCode.OK

