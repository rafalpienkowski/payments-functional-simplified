module Payments.WebApi.IntegrationTests.TestHelpers

open System.IO
open System.Text
open Microsoft.AspNetCore.TestHost
open Microsoft.AspNetCore.Hosting
open System.Net.Http
open Microsoft.Extensions.Configuration
open Newtonsoft.Json
open Payments.WebApi.Settings

let appConfig (_: WebHostBuilderContext) (conf: IConfigurationBuilder) : unit =
    let projectDir = Directory.GetCurrentDirectory();
    let configPath = Path.Combine(projectDir, "appsettings.json");
    conf.AddJsonFile(configPath) |> ignore

let getTestHost () =
    WebHostBuilder()
        .UseTestServer()
        .Configure(configureApp)
        .ConfigureServices(configureServices)
        .ConfigureAppConfiguration(appConfig)

let testRequest (request: HttpRequestMessage) =
    let resp =
        task {
            use server = new TestServer(getTestHost ())
            use client = server.CreateClient()
            let! response = request |> client.SendAsync
            return response
        }

    resp.Result

let createPostRequest (url: string) dto =
    let httpPostRequest = new HttpRequestMessage(HttpMethod.Post, url)
    let json = JsonConvert.SerializeObject dto
    let content = new StringContent(json, UnicodeEncoding.UTF8, "application/json")
    httpPostRequest.Content <- content
    
    httpPostRequest