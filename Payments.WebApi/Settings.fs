module Payments.WebApi.Settings

open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Payments.WebApi.Transaction

let webApp =
    choose
        [ route "/ping" >=> json {| Response = "pong" |}
          route "/transactions" >=> GET >=> readTransactions ]
          
          (*
          route "/transactions/initialize"
          >=> POST
          >=> initializeTransactionHandler startStream acknowledgeWithProvider appendStream
          route "/transactions/post"
          >=> POST
          >=> postTransactionHandler fetchStream appendStream
          route "/transactions/confirm"
          >=> POST
          >=> confirmTransactionHandler fetchStream appendStream ]
          *)

let configureApp (app: IApplicationBuilder) = app.UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore
    
    let jsonOptions =
        JsonFSharpOptions.Default()
            .ToJsonSerializerOptions()
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(jsonOptions)) |> ignore