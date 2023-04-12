module ``initialize transaction should``

open Xunit

[<Fact>]
let ``reject invalid request``() =
    Assert.Fail