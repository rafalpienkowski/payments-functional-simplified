module Payments.WebApi.Provider

open Payments.WebApi.Transaction

let acknowledgeWithProvider: AcknowledgedWithProvider =
    fun _ amount ->
        if Amount.value amount < 10m then
            Error([ "Something went wrong" ])
        else
            Ok(ProviderReference.newValue)
