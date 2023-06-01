module Payments.WebApi.Settings

open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Payments.WebApi.TransactionView
open Payments.WebApi.TransactionInitialize
open Payments.WebApi.TransactionPost
open Payments.WebApi.Provider

let webApp =
    choose
        [ route "/ping" >=> json {| Response = "pong" |}
          route "/transactions" >=> GET >=> readTransactions
          route "/transactions/initialize"
          >=> POST
          >=> startTransaction acknowledgeWithProvider 
          route "/transactions/post"
          >=> POST
          >=> postTransactionHandler ]
(*
          route "/transactions/confirm"
          >=> POST
          >=> confirmTransactionHandler fetchStream appendStream ]
          *)

let configureApp (app: IApplicationBuilder) = app.UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

    let jsonOptions = JsonFSharpOptions.Default().ToJsonSerializerOptions()

    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(jsonOptions))
    |> ignore
