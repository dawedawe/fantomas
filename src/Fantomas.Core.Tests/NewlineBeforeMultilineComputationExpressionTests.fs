module Fantomas.Core.Tests.NewlineBeforeMultilineComputationExpressionTests

open NUnit.Framework
open FsUnit
open Fantomas.Core.Tests.TestHelpers
open Fantomas.Core

let config =
    { config with
        NewlineBeforeMultilineComputationExpression = false
        MaxArrayOrListWidth = 40 }

[<Test>]
let ``prefer computation expression name on same line`` () =
    formatSourceString
        """
let t =
    task {
        let! thing = otherThing ()
        return 5
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let t = task {
    let! thing = otherThing ()
    return 5
}
"""

[<Test>]
let ``prefer computation expression name on same line handling short expression`` () =
    formatSourceString
        """
let t =
    task {
        return ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let t = task { return () }
"""

[<Test>]
let ``application parenthesis expr dotIndexedSet with computation expression`` () =
    formatSourceString
        """
app(meh).[x] <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
app(meh).[x] <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``application unit dotIndexedSet with computation expression`` () =
    formatSourceString
        """
app().[x] <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
app().[x] <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``dotIndexedSet with computation expression`` () =
    formatSourceString
        """
myMutable.[x] <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
myMutable.[x] <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``dotSet with computation expression`` () =
    formatSourceString
        """
App().foo <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
App().foo <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``app paren lambda with computation expression`` () =
    formatSourceString
        """
List.map (fun x ->
    task {
        // some computation here
        ()
    })
"""
        config
    |> prepend newline
    |> should
        equal
        """
List.map (fun x -> task {
    // some computation here
    ()
})
"""

[<Test>]
let ``app paren lambda with computation expression and other args`` () =
    formatSourceString
        """
List.map (fun x ->
    task {
        // some computation here
        ()
    }) b c
"""
        config
    |> prepend newline
    |> should
        equal
        """
List.map
    (fun x -> task {
        // some computation here
        ()
    })
    b
    c
"""

[<Test>]
let ``dotGetApp with lambda with computation expression`` () =
    formatSourceString
        """
Bar
    .Foo(fun x ->
                    task {
                        // some computation here
                        ()
                    }).Bar()
"""
        config
    |> prepend newline
    |> should
        equal
        """
Bar
    .Foo(fun x -> task {
        // some computation here
        ()
    })
    .Bar()
"""

[<Test>]
let ``lambda with computation expression`` () =
    formatSourceString
        """
fun x ->
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
fun x -> task {
    // some computation here
    ()
}
"""

[<Test>]
let ``letOrUseBang with computation expression`` () =
    formatSourceString
        """
task {
    let! meh =
        task {
            // comment
            return 42
        }
    ()
}
"""
        config
    |> prepend newline
    |> should
        equal
        """
task {
    let! meh = task {
        // comment
        return 42
    }

    ()
}
"""

[<Test>]
let ``longIdentSet with computation expression`` () =
    formatSourceString
        """
myMutable <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
myMutable <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``paren lambda with computation expression`` () =
    formatSourceString
        """
(fun x ->
    task {
        // some computation here
        ()
    })
"""
        config
    |> prepend newline
    |> should
        equal
        """
(fun x -> task {
    // some computation here
    ()
})
"""

[<Test>]
let ``synExprApp with named argument with computation expression`` () =
    formatSourceString
        """
let v =
    SomeConstructor(
        v =
                task {
                    // some computation here
                    ()
                }
    )
"""
        config
    |> prepend newline
    |> should
        equal
        """
let v =
    SomeConstructor(
        v = task {
            // some computation here
            ()
        }
    )
"""

[<Test>]
let ``synExprNew with named argument with computation expression`` () =
    formatSourceString
        """
let v =
    new FooBar(
        v =
                task {
                    // some computation here
                    ()
                }
    )
"""
        config
    |> prepend newline
    |> should
        equal
        """
let v =
    new FooBar(
        v = task {
            // some computation here
            ()
        }
    )
"""

[<Test>]
let ``set with computation expression`` () =
    formatSourceString
        """
myMutable[x] <-
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
myMutable[x] <- task {
    // some computation here
    ()
}
"""

[<Test>]
let ``synbinding function with computation expression`` () =
    formatSourceString
        """
let x y =
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let x y = task {
    // some computation here
    ()
}
"""

[<Test>]
let ``synbinding function with computation expression with return type`` () =
    formatSourceString
        """
let x y: Task<unit> =
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let x y : Task<unit> = task {
    // some computation here
    ()
}
"""

[<Test>]
let ``type member function with computation expression`` () =
    formatSourceString
        """
type Foo() =
    member this.Bar x =
        task {
            // some computation here
            ()
        }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo() =
    member this.Bar x = task {
        // some computation here
        ()
    }
"""

[<Test>]
let ``type member function with computation expression with return type`` () =
    formatSourceString
        """
type Foo() =
    member this.Bar x : Task<unit> =
        task {
            // some computation here
            ()
        }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo() =
    member this.Bar x : Task<unit> = task {
        // some computation here
        ()
    }
"""

[<Test>]
let ``synbinding value with computation expression`` () =
    formatSourceString
        """
let t =
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let t = task {
    // some computation here
    ()
}
"""

[<Test>]
let ``type member value with computation expression`` () =
    formatSourceString
        """
type Foo() =
    member this.Bar =
        task {
            // some computation here
            ()
        }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo() =
    member this.Bar = task {
        // some computation here
        ()
    }
"""

[<Test>]
let ``andBang with computation expression`` () =
    formatSourceString
        """
task {
    let! abc = def ()
    and! meh =
        task {
            // comment
            return 42
        }
    ()
}
"""
        config
    |> prepend newline
    |> should
        equal
        """
task {
    let! abc = def ()

    and! meh = task {
        // comment
        return 42
    }

    ()
}
"""

[<Test>]
let ``synMatchClause in match expression with computation expression`` () =
    formatSourceString
        """
match x with
| _ ->
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
match x with
| _ -> task {
    // some computation here
    ()
  }
"""

[<Test>]
let ``synMatchClause in try/with expression with computation expression`` () =
    formatSourceString
        """
try
    foo()
with
| ex ->
    task {
        // some computation here
        ()
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
try
    foo ()
with ex -> task {
    // some computation here
    ()
}
"""

[<Test>]
let ``yieldOrReturnBang with computation expression`` () =
    formatSourceString
        """
myComp {
    yield!
       seq {
            // meh
            return 0 .. 2
       }
    return!
       seq {
            // meh
            return 0 .. 2
       }
}
"""
        config
    |> prepend newline
    |> should
        equal
        """
myComp {
    yield! seq {
        // meh
        return 0..2
    }

    return! seq {
        // meh
        return 0..2
    }
}
"""

[<Test>]
let ``yieldOrReturn with computation expression`` () =
    formatSourceString
        """
myComp {
    yield
       seq {
            // meh
            return 0 .. 2
       }
    return
       seq {
            // meh
            return 0 .. 2
       }
}
"""
        config
    |> prepend newline
    |> should
        equal
        """
myComp {
    yield seq {
        // meh
        return 0..2
    }

    return seq {
        // meh
        return 0..2
    }
}
"""

[<Test>]
let ``prefer computation expression name on same line, with trivia`` () =
    formatSourceString
        """
let t =
    //
    task {
        let! thing = otherThing ()
        return 5
    }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let t =
    //
    task {
        let! thing = otherThing ()
        return 5
    }
"""

[<Test>]
let ``fsharp_multiline_bracket_style = stroustrup has not influence`` () =
    formatSourceString
        """
fun _ -> task { // foo
                () }
"""
        { FormatConfig.Default with
            MultilineBracketStyle = Stroustrup }
    |> prepend newline
    |> should
        equal
        """
fun _ ->
    task { // foo
        ()
    }
"""
