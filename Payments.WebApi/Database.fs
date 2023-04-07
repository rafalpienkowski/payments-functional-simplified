module Payments.WebApi.Database

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

let getConnectionString (ctx: HttpContext) =
    let config = ctx.GetService<IConfiguration>()
    config.GetConnectionString "Database"
