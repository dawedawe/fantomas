module Fantomas.Core.Tests.CrampedMultilineBracketStyleTests

open NUnit.Framework
open FsUnit
open Fantomas.Core.Tests.TestHelpers
open Fantomas.Core

[<Test>]
let ``record declaration`` () =
    formatSourceString "type AParameters = { a : int }" config
    |> prepend newline
    |> should
        equal
        """
type AParameters = { a: int }
"""

[<Test>]
let ``record declaration with implementation visibility attribute`` () =
    formatSourceString "type AParameters = private { a : int; b: float }" config
    |> prepend newline
    |> should
        equal
        """
type AParameters = private { a: int; b: float }
"""

[<Test>]
let ``record signatures`` () =
    formatSignatureString
        """
module RecordSignature
/// Represents simple XML elements.
type Element =
    {
        /// The attribute collection.
        Attributes : IDictionary<Name,string>

        /// The children collection.
        Children : seq<INode>

        /// The qualified name.
        Name : Name
    }

    interface INode

    /// Constructs an new empty Element.
    static member Create : name: string * ?uri: string -> Element

    /// Replaces the children.
    static member WithChildren : children: #seq<#INode> -> self: Element -> Element

    /// Replaces the children.
    static member ( - ) : self: Element * children: #seq<#INode> -> Element

    /// Replaces the attributes.
    static member WithAttributes : attrs: #seq<string*string> -> self: Element -> Element

    /// Replaces the attributes.
    static member ( + ) : self: Element * attrs: #seq<string*string> -> Element

    /// Replaces the children with a single text node.
    static member WithText : text: string -> self: Element-> Element

    /// Replaces the children with a single text node.
    static member ( -- ) : self: Element * text: string -> Element"""
        config
    |> prepend newline
    |> should
        equal
        """
module RecordSignature

/// Represents simple XML elements.
type Element =
    {
        /// The attribute collection.
        Attributes: IDictionary<Name, string>

        /// The children collection.
        Children: seq<INode>

        /// The qualified name.
        Name: Name
    }

    interface INode

    /// Constructs an new empty Element.
    static member Create: name: string * ?uri: string -> Element

    /// Replaces the children.
    static member WithChildren: children: #seq<#INode> -> self: Element -> Element

    /// Replaces the children.
    static member (-): self: Element * children: #seq<#INode> -> Element

    /// Replaces the attributes.
    static member WithAttributes: attrs: #seq<string * string> -> self: Element -> Element

    /// Replaces the attributes.
    static member (+): self: Element * attrs: #seq<string * string> -> Element

    /// Replaces the children with a single text node.
    static member WithText: text: string -> self: Element -> Element

    /// Replaces the children with a single text node.
    static member (--): self: Element * text: string -> Element
"""

[<Test>]
let ``records with update`` () =
    formatSourceString
        """
type Car = {
    Make : string
    Model : string
    mutable Odometer : int
    }

let myRecord3 = { myRecord2 with Y = 100; Z = 2 }"""
        config
    |> prepend newline
    |> should
        equal
        """
type Car =
    { Make: string
      Model: string
      mutable Odometer: int }

let myRecord3 = { myRecord2 with Y = 100; Z = 2 }
"""

// the current behavior results in a compile error since the if is not aligned properly
[<Test>]
let ``should not break inside of if statements in records`` () =
    formatSourceString
        """let XpkgDefaults() =
    {
        ToolPath = "./tools/xpkg/xpkg.exe"
        WorkingDir = "./";
        TimeOut = TimeSpan.FromMinutes 5.
        Package = null
        Version = if not isLocalBuild then buildVersion else "0.1.0.0"
        OutputPath = "./xpkg"
        Project = null
        Summary = null
        Publisher = null
        Website = null
        Details = "Details.md"
        License = "License.md"
        GettingStarted = "GettingStarted.md"
        Icons = []
        Libraries = []
        Samples = [];
    }

    """
        { config with
            MaxIfThenElseShortWidth = 52 }
    |> should
        equal
        """let XpkgDefaults () =
    { ToolPath = "./tools/xpkg/xpkg.exe"
      WorkingDir = "./"
      TimeOut = TimeSpan.FromMinutes 5.
      Package = null
      Version = if not isLocalBuild then buildVersion else "0.1.0.0"
      OutputPath = "./xpkg"
      Project = null
      Summary = null
      Publisher = null
      Website = null
      Details = "Details.md"
      License = "License.md"
      GettingStarted = "GettingStarted.md"
      Icons = []
      Libraries = []
      Samples = [] }
"""

[<Test>]
let ``should not add redundant newlines when using a record in a DU`` () =
    formatSourceString
        """
let rec make item depth =
    if depth > 0 then
        Tree({ Left = make (2 * item - 1) (depth - 1)
               Right = make (2 * item) (depth - 1) }, item)
    else Tree(defaultof<_>, item)"""
        config
    |> prepend newline
    |> should
        equal
        """
let rec make item depth =
    if depth > 0 then
        Tree(
            { Left = make (2 * item - 1) (depth - 1)
              Right = make (2 * item) (depth - 1) },
            item
        )
    else
        Tree(defaultof<_>, item)
"""

[<Test>]
let ``record inside DU constructor`` () =
    formatSourceString
        """let a = Tree({ Left = make (2 * item - 1) (depth - 1); Right = make (2 * item) (depth - 1) }, item)
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a =
    Tree(
        { Left = make (2 * item - 1) (depth - 1)
          Right = make (2 * item) (depth - 1) },
        item
    )
"""

[<Test>]
let ``should keep unit of measures in record and DU declaration`` () =
    formatSourceString
        """
type rate = {Rate:float<GBP*SGD/USD>}
type rate2 = Rate of float<GBP/SGD*USD>
"""
        config
    |> prepend newline
    |> should
        equal
        """
type rate = { Rate: float<GBP * SGD / USD> }
type rate2 = Rate of float<GBP / SGD * USD>
"""

[<Test>]
let ``should keep comments on records`` () =
    formatSourceString
        """
let newDocument = //somecomment
    { program = Encoding.Default.GetBytes(document.Program) |> Encoding.UTF8.GetString
      content = Encoding.Default.GetBytes(document.Content) |> Encoding.UTF8.GetString
      created = document.Created.ToLocalTime() }
    |> JsonConvert.SerializeObject
"""
        { config with
            MaxInfixOperatorExpression = 75 }
    |> prepend newline
    |> should
        equal
        """
let newDocument = //somecomment
    { program = Encoding.Default.GetBytes(document.Program) |> Encoding.UTF8.GetString
      content = Encoding.Default.GetBytes(document.Content) |> Encoding.UTF8.GetString
      created = document.Created.ToLocalTime() }
    |> JsonConvert.SerializeObject
"""

[<Test>]
let ``|> should be on the next line if preceding expression is multiline`` () =
    formatSourceString
        """
let newDocument = //somecomment
    { program = "Loooooooooooooooooooooooooong"
      content = "striiiiiiiiiiiiiiiiiiinnnnnnnnnnng"
      created = document.Created.ToLocalTime() }
    |> JsonConvert.SerializeObject
"""
        config
    |> prepend newline
    |> should
        equal
        """
let newDocument = //somecomment
    { program = "Loooooooooooooooooooooooooong"
      content = "striiiiiiiiiiiiiiiiiiinnnnnnnnnnng"
      created = document.Created.ToLocalTime() }
    |> JsonConvert.SerializeObject
"""

[<Test>]
let ``should preserve inherit parts in records`` () =
    formatSourceString
        """
type MyExc =
    inherit Exception
    new(msg) = {inherit Exception(msg)}
"""
        config
    |> prepend newline
    |> should
        equal
        """
type MyExc =
    inherit Exception
    new(msg) = { inherit Exception(msg) }
"""

[<Test>]
let ``should preserve inherit parts in records with field`` () =
    formatSourceString
        """
type MyExc =
    inherit Exception
    new(msg) = {inherit Exception(msg)
                X = 1}
"""
        config
    |> prepend newline
    |> should
        equal
        """
type MyExc =
    inherit Exception
    new(msg) = { inherit Exception(msg); X = 1 }
"""

[<Test>]
let ``should preserve inherit parts in records multiline`` () =
    formatSourceString
        """
type MyExc =
    inherit Exception
    new(msg) = {inherit Exception(msg)
                XXXXXXXXXXXXXXXXXXXXXXXX = 1
                YYYYYYYYYYYYYYYYYYYYYYYY = 2}
"""
        config
    |> prepend newline
    |> should
        equal
        """
type MyExc =
    inherit Exception

    new(msg) =
        { inherit Exception(msg)
          XXXXXXXXXXXXXXXXXXXXXXXX = 1
          YYYYYYYYYYYYYYYYYYYYYYYY = 2 }
"""

[<Test>]
let ``anon record`` () =
    formatSourceString
        """let r: {| Foo: int; Bar: string |} =
    {| Foo = 123
       Bar = "" |}
"""
        { config with MaxRecordWidth = 10 }
    |> prepend newline
    |> should
        equal
        """
let r
    : {| Foo: int
         Bar: string |} =
    {| Foo = 123
       Bar = "" |}
"""

[<Test>]
let ``anon record - struct`` () =
    formatSourceString
        """let r: struct {| Foo: int; Bar: string |} =
    struct {| Foo = 123
              Bar = "" |}
"""
        { config with MaxRecordWidth = 10 }
    |> prepend newline
    |> should
        equal
        """
let r
    : struct {| Foo: int
                Bar: string |} =
    struct {| Foo = 123
              Bar = "" |}
"""

[<Test>]
let ``anon record with multiline assignments`` () =
    formatSourceString
        "
let r =
    {|
        Foo =
            a && // && b
            c
        Bar =
        \"\"\"
Fooey
\"\"\"
    |}
"
        config
    |> prepend newline
    |> should
        equal
        "
let r =
    {| Foo =
        a
        && // && b
        c
       Bar =
        \"\"\"
Fooey
\"\"\" |}
"

[<Test>]
let ``meaningful space should be preserved, 353`` () =
    formatSourceString
        """to'.WithCommon(fun o' ->
        { dotnetOptions o' with WorkingDirectory =
                                  Path.getFullName "RegressionTesting/issue29"
                                Verbosity = Some DotNet.Verbosity.Minimal }).WithParameters"""
        config
    |> prepend newline
    |> should
        equal
        """
to'
    .WithCommon(fun o' ->
        { dotnetOptions o' with
            WorkingDirectory = Path.getFullName "RegressionTesting/issue29"
            Verbosity = Some DotNet.Verbosity.Minimal })
    .WithParameters
"""

[<Test>]
let ``record with long string inside array`` () =
    formatSourceString
        "
type Database =

    static member Default () =
        Database.Lowdb
            .defaults(
                { Version = CurrentVersion
                  Questions =
                    [| { Id = 0
                         AuthorId = 1
                         Title = \"What is the average wing speed of an unladen swallow?\"
                         Description =
                             \"\"\"
Hello, yesterday I saw a flight of swallows and was wondering what their **average wing speed** is?

If you know the answer please share it.
                             \"\"\"
                         Answers =
                            [| { Id = 0
                                 CreatedAt = DateTime.Parse \"2017-09-14T19:57:33.103Z\"
                                 AuthorId = 0
                                 Score = 2
                                 Content =
                                    \"\"\"
> What do you mean, an African or European Swallow?
>
> Monty Python’s: The Holy Grail

Ok I must admit, I use google to search the question and found a post explaining the reference :).

I thought you were asking it seriously, well done.
                                    x\"\"\" }
                               { Id = 1
                                 CreatedAt = DateTime.Parse \"2017-09-14T20:07:27.103Z\"
                                 AuthorId = 2
                                 Score = 1
                                 Content =
                                    \"\"\"
Maxime,

I believe you found [this blog post](http://www.saratoga.com/how-should-i-know/2013/07/what-is-the-average-air-speed-velocity-of-a-laden-swallow/).

And so Robin, the conclusion of the post is:

> In the end, it’s concluded that the airspeed velocity of a (European) unladen swallow is about 24 miles per hour or 11 meters per second.
                                    \"\"\" }
                            |]
                         CreatedAt = DateTime.Parse \"2017-09-14T17:44:28.103Z\" }
                       { Id = 1
                         AuthorId = 0
                         Title = \"Why did you create Fable?\"
                         Description =
                             \"\"\"
Hello Alfonso,

I wanted to know why you created Fable. Did you always plan to use F#? Or were you thinking in others languages?
                             \"\"\"
                         Answers = [| |]
                         CreatedAt = DateTime.Parse \"2017-09-12T09:27:28.103Z\" } |]
                  Users =
                    [| { Id = 0
                         Firstname = \"Maxime\"
                         Surname = \"Mangel\"
                         Avatar = \"maxime_mangel.png\" }
                       { Id = 1
                         Firstname = \"Robin\"
                         Surname = \"Munn\"
                         Avatar = \"robin_munn.png\" }
                       { Id = 2
                         Firstname = \"Alfonso\"
                         Surname = \"Garciacaro\"
                         Avatar = \"alfonso_garciacaro.png\" }
                       { Id = 3
                         Firstname = \"Guest\"
                         Surname = \"\"
                         Avatar = \"guest.png\" }
                          |]
                }
            ).write()
        Logger.debug \"Database restored\"

"
        config
    |> prepend newline
    |> should
        equal
        "
type Database =

    static member Default() =
        Database.Lowdb
            .defaults(
                { Version = CurrentVersion
                  Questions =
                    [| { Id = 0
                         AuthorId = 1
                         Title = \"What is the average wing speed of an unladen swallow?\"
                         Description =
                           \"\"\"
Hello, yesterday I saw a flight of swallows and was wondering what their **average wing speed** is?

If you know the answer please share it.
                             \"\"\"
                         Answers =
                           [| { Id = 0
                                CreatedAt = DateTime.Parse \"2017-09-14T19:57:33.103Z\"
                                AuthorId = 0
                                Score = 2
                                Content =
                                  \"\"\"
> What do you mean, an African or European Swallow?
>
> Monty Python’s: The Holy Grail

Ok I must admit, I use google to search the question and found a post explaining the reference :).

I thought you were asking it seriously, well done.
                                    x\"\"\" }
                              { Id = 1
                                CreatedAt = DateTime.Parse \"2017-09-14T20:07:27.103Z\"
                                AuthorId = 2
                                Score = 1
                                Content =
                                  \"\"\"
Maxime,

I believe you found [this blog post](http://www.saratoga.com/how-should-i-know/2013/07/what-is-the-average-air-speed-velocity-of-a-laden-swallow/).

And so Robin, the conclusion of the post is:

> In the end, it’s concluded that the airspeed velocity of a (European) unladen swallow is about 24 miles per hour or 11 meters per second.
                                    \"\"\" } |]
                         CreatedAt = DateTime.Parse \"2017-09-14T17:44:28.103Z\" }
                       { Id = 1
                         AuthorId = 0
                         Title = \"Why did you create Fable?\"
                         Description =
                           \"\"\"
Hello Alfonso,

I wanted to know why you created Fable. Did you always plan to use F#? Or were you thinking in others languages?
                             \"\"\"
                         Answers = [||]
                         CreatedAt = DateTime.Parse \"2017-09-12T09:27:28.103Z\" } |]
                  Users =
                    [| { Id = 0
                         Firstname = \"Maxime\"
                         Surname = \"Mangel\"
                         Avatar = \"maxime_mangel.png\" }
                       { Id = 1
                         Firstname = \"Robin\"
                         Surname = \"Munn\"
                         Avatar = \"robin_munn.png\" }
                       { Id = 2
                         Firstname = \"Alfonso\"
                         Surname = \"Garciacaro\"
                         Avatar = \"alfonso_garciacaro.png\" }
                       { Id = 3
                         Firstname = \"Guest\"
                         Surname = \"\"
                         Avatar = \"guest.png\" } |] }
            )
            .write ()

        Logger.debug \"Database restored\"
"

[<Test>]
let ``multiline string before closing brace`` () =
    formatSourceString
        "
let person =
    let y =
        let x =
            { Story = \"\"\"
            foo
            bar
\"\"\"
            }
        ()
    ()
"
        config
    |> prepend newline
    |> should
        equal
        "
let person =
    let y =
        let x =
            { Story =
                \"\"\"
            foo
            bar
\"\"\"         }

        ()

    ()
"

[<Test>]
let ``multiline string before closing brace with anonymous record`` () =
    formatSourceString
        "
let person =
    let y =
        let x =
            {| Story = \"\"\"
            foo
            bar
\"\"\"
            |}
        ()
    ()
"
        config
    |> prepend newline
    |> should
        equal
        "
let person =
    let y =
        let x =
            {| Story =
                \"\"\"
            foo
            bar
\"\"\"         |}

        ()

    ()
"

[<Test>]
let ``multiline string before closing brace with update record, 3017`` () =
    formatSourceString
        "
let FableSampleExpected :Changelogs = {
    Unreleased = Some {
        ChangelogData.Default with
            Fixed =
                normalizeNewline
                    \"\"\"#### Python
\"\"\"
    }
    Releases = [
        Some {
            ChangelogData.Default with
                Changed =
                    normalizeNewline \"\"
        }
    ]
}
"
        config
    |> prepend newline
    |> should
        equal
        "
let FableSampleExpected: Changelogs =
    { Unreleased =
        Some
            { ChangelogData.Default with
                Fixed =
                    normalizeNewline
                        \"\"\"#### Python
\"\"\"         }
      Releases =
        [ Some
              { ChangelogData.Default with
                  Changed = normalizeNewline \"\" } ] }
"

[<Test>]
let ``issue 457`` () =
    formatSourceString
        """
let x = Foo("").Goo()

let r =
    { s with
        xxxxxxxxxxxxxxxxxxxxx = 1
        yyyyyyyyyyyyyyyyyyyyy = 2 }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let x = Foo("").Goo()

let r =
    { s with
        xxxxxxxxxxxxxxxxxxxxx = 1
        yyyyyyyyyyyyyyyyyyyyy = 2 }
"""

[<Test>]
let ``class member attributes should not introduce newline, 471`` () =
    formatSourceString
        """type Test =
    | String of string

    [<SomeAttribute>]
    member x._Print = ""

    member this.TestMember = ""
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Test =
    | String of string

    [<SomeAttribute>]
    member x._Print = ""

    member this.TestMember = ""
"""

[<Test>]
let ``multiline record should be on new line after DU constructor, 462`` () =
    formatSourceString
        """
let expect =
    Result<Schema, SetError>.Ok { opts =
                                      [ Opts.anyOf
                                          ([ (Optional, Opt.flagTrue [ "first"; "f" ])
                                             (Optional, Opt.value [ "second"; "s" ]) ])
                                        Opts.oneOf
                                            (Optional,
                                             [ Opt.flag [ "third"; "f" ]
                                               Opt.valueWith "new value" [ "fourth"; "ssssssssssssssssssssssssssssssssssssssssssssssssssss" ] ]) ]
                                  args = []
                                  commands = [] }
"""
        { config with MaxArrayOrListWidth = 40 }
    |> prepend newline
    |> should
        equal
        """
let expect =
    Result<Schema, SetError>.Ok
        { opts =
            [ Opts.anyOf (
                  [ (Optional, Opt.flagTrue [ "first"; "f" ])
                    (Optional, Opt.value [ "second"; "s" ]) ]
              )
              Opts.oneOf (
                  Optional,
                  [ Opt.flag [ "third"; "f" ]
                    Opt.valueWith
                        "new value"
                        [ "fourth"
                          "ssssssssssssssssssssssssssssssssssssssssssssssssssss" ] ]
              ) ]
          args = []
          commands = [] }
"""

[<Test>]
let ``short record should remain on the same line after DU constructor`` () =
    formatSourceString
        """
let expect = Result<int,string>.Ok   7"""
        config
    |> prepend newline
    |> should
        equal
        """
let expect = Result<int, string>.Ok 7
"""

[<Test>]
let ``multiline list after DU should be on new line after DU constructor`` () =
    formatSourceString
        """
let expect =
    Result<int,string>.Ok [ "fooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo"
                            "baaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaar"
                            "meh"
                          ]"""
        config
    |> prepend newline
    |> should
        equal
        """
let expect =
    Result<int, string>.Ok
        [ "fooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo"
          "baaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaar"
          "meh" ]
"""

[<Test>]
let ``record with long string, 472`` () =
    formatSourceString
        "
namespace web_core

open WebSharper.UI

module Maintoc =
  let Page =
    { MyPage.Create() with body =
                                [ Doc.Verbatim \"\"\"
This is a very long line in a multi-line string, so long in fact that it is longer than that page width to which I am trying to constrain everything, and so it goes bang.
\"\"\" ] }"
        config
    |> prepend newline
    |> should
        equal
        "
namespace web_core

open WebSharper.UI

module Maintoc =
    let Page =
        { MyPage.Create() with
            body =
                [ Doc.Verbatim
                      \"\"\"
This is a very long line in a multi-line string, so long in fact that it is longer than that page width to which I am trying to constrain everything, and so it goes bang.
\"\"\" ]   }
"

[<Test>]
let ``record type signature with line comment, 517`` () =
    formatSignatureString
        """
module RecordSignature
/// Represents simple XML elements.
type Element =
    { /// The attribute collection.
      Attributes: IDictionary<Name, string>;

      /// The children collection.
      Children: seq<INode>;

      /// The qualified name.
      Name: Name }
"""
        config
    |> prepend newline
    |> should
        equal
        """
module RecordSignature

/// Represents simple XML elements.
type Element =
    {
        /// The attribute collection.
        Attributes: IDictionary<Name, string>

        /// The children collection.
        Children: seq<INode>

        /// The qualified name.
        Name: Name
    }
"""

[<Test>]
let ``don't duplicate newlines in object expression, 601`` () =
    formatSourceString
        """namespace Blah

open System

module Test =
    type ISomething =
        inherit IDisposable
        abstract DoTheThing: string -> unit

    let test (something: IDisposable) (somethingElse: IDisposable) =
        { new ISomething with

            member __.DoTheThing whatever =
                printfn "%s" whatever
                printfn "%s" whatever

            member __.Dispose() =
                something.Dispose()
                somethingElse.Dispose() }
"""
        config
    |> prepend newline
    |> should
        equal
        """
namespace Blah

open System

module Test =
    type ISomething =
        inherit IDisposable
        abstract DoTheThing: string -> unit

    let test (something: IDisposable) (somethingElse: IDisposable) =
        { new ISomething with

            member __.DoTheThing whatever =
                printfn "%s" whatever
                printfn "%s" whatever

            member __.Dispose() =
                something.Dispose()
                somethingElse.Dispose() }
"""

[<Test>]
let ``short record should be a oneliner`` () =
    formatSourceString
        """let a = { B = 7 ; C = 9 }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a = { B = 7; C = 9 }
"""

// This test ensures that the normal flow of Fantomas is resumed when the next expression is being written.
[<Test>]
let ``short record and let binding`` () =
    formatSourceString
        """
let a = { B = 7 ; C = 9 }
let sumOfMember = a.B + a.C
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a = { B = 7; C = 9 }
let sumOfMember = a.B + a.C
"""

[<Test>]
let ``long record should be multiline`` () =
    formatSourceString
        """let myInstance = { FirstLongMemberName = "string value" ; SecondLongMemberName = "other value" }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let myInstance =
    { FirstLongMemberName = "string value"
      SecondLongMemberName = "other value" }
"""

[<Test>]
let ``multiline in record field should short circuit short expression check`` () =
    formatSourceString
        """
let a =
    { B =
          8 // some comment
      C = 9 }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a =
    { B = 8 // some comment
      C = 9 }
"""

[<Test>]
let ``short record type should remain single line`` () =
    formatSourceString "type Foo = { A: int; B:   string }" config
    |> prepend newline
    |> should
        equal
        """
type Foo = { A: int; B: string }
"""

[<Test>]
let ``short record type with comment should go to multiline`` () =
    formatSourceString
        """type Foo = { A: int;
                    // comment
                    B:   string }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo =
    { A: int
      // comment
      B: string }
"""

[<Test>]
let ``short record type with comment after opening brace should go to multiline`` () =
    formatSourceString
        """type Foo = { // comment
                    A: int;
                    B:   string }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo =
    { // comment
      A: int
      B: string }
"""

[<Test>]
let ``short record type with member definitions should be multi line`` () =
    formatSourceString
        "type Foo = { A: int; B:   string } with member this.Foo () = ()"
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type Foo =
    { A: int
      B: string }
    member this.Foo() = ()
"""

[<Test>]
let ``record type definition with members and trivia`` () =
    formatSourceString
        """
type X = {
    Y: int
} with // foo
    member x.Z = ()
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type X =
    { Y: int } // foo
    member x.Z = ()
"""

[<Test>]
let ``short anonymous record with two members`` () =
    formatSourceString
        """let foo =
    {| A = 7
       B = 8 |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
let foo = {| A = 7; B = 8 |}
"""

[<Test>]
let ``short anonymous record with copy expression`` () =
    formatSourceString
        """let foo =
    {| bar with A = 7 |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
let foo = {| bar with A = 7 |}
"""

[<Test>]
let ``longer anonymous record with copy expression`` () =
    formatSourceString
        """let foo =
    {| bar with AMemberWithALongName = aValueWithAlsoALongName |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
let foo =
    {| bar with
        AMemberWithALongName = aValueWithAlsoALongName |}
"""

[<Test>]
let ``short anonymous record type alias`` () =
    formatSourceString
        """
let useAddEntry() =
    fun (input: {| name: string; amount: Amount |}) ->
        // foo
        bar ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
let useAddEntry () =
    fun (input: {| name: string; amount: Amount |}) ->
        // foo
        bar ()
"""

[<Test>]
let ``long anonymous record type alias`` () =
    formatSourceString
        """
let useAddEntry() =
    fun (input: {| name: string; amount: Amount; isIncome: bool; created: string |}) ->
        // foo
        bar ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
let useAddEntry () =
    fun
        (input:
            {| name: string
               amount: Amount
               isIncome: bool
               created: string |}) ->
        // foo
        bar ()
"""

[<Test>]
let ``line comment after { should make record multiline`` () =
    formatSourceString
        """let meh = { // this comment right
    Name = "FOO"; Level = 78 }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let meh =
    { // this comment right
      Name = "FOO"
      Level = 78 }
"""

[<Test>]
let ``line comment after short syntax record type, 774`` () =
    formatSourceString
        """type FormatConfig = {
    PageWidth: int
    Indent: int } // The number of spaces
"""
        config
    |> should
        equal
        """type FormatConfig = { PageWidth: int; Indent: int } // The number of spaces
"""

[<Test>]
let ``line comment after short record instance syntax`` () =
    formatSourceString
        """let formatConfig = {
    PageWidth = 70
    Indent = 8 } // The number of spaces
"""
        config
    |> should
        equal
        """let formatConfig = { PageWidth = 70; Indent = 8 } // The number of spaces
"""

[<Test>]
let ``line comment after short anonymous record instance syntax`` () =
    formatSourceString
        """let formatConfig = {|
    PageWidth = 70
    Indent = 8 |} // The number of spaces
"""
        config
    |> should
        equal
        """let formatConfig = {| PageWidth = 70; Indent = 8 |} // The number of spaces
"""

[<Test>]
let ``no newline before first multiline member`` () =
    formatSourceString
        """
type ShortExpressionInfo =
    { MaxWidth: int
      StartColumn: int
      ConfirmedMultiline: bool }
    member x.IsTooLong maxPageWidth currentColumn =
        currentColumn - x.StartColumn > x.MaxWidth // expression is not too long according to MaxWidth
        || (currentColumn > maxPageWidth) // expression at current position is not going over the page width
    member x.Foo() = ()
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type ShortExpressionInfo =
    { MaxWidth: int
      StartColumn: int
      ConfirmedMultiline: bool }
    member x.IsTooLong maxPageWidth currentColumn =
        currentColumn - x.StartColumn > x.MaxWidth // expression is not too long according to MaxWidth
        || (currentColumn > maxPageWidth) // expression at current position is not going over the page width

    member x.Foo() = ()
"""

[<Test>]
let ``record with static member, 1059`` () =
    formatSourceString
        """
type XX =
  {a:int
   b:int}
  static member foo = 30
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type XX =
    { a: int
      b: int }
    static member foo = 30
"""

[<Test>]
let ``record with typed static member`` () =
    formatSourceString
        """
type XX =
  {a:int
   b:int}
  static member foo : int = 30
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type XX =
    { a: int
      b: int }
    static member foo: int = 30
"""

[<Test>]
let ``record with private static member`` () =
    formatSourceString
        """
type XX =
    { a: int
      b: int }
    static member private foo: int = 30
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = true }
    |> prepend newline
    |> should
        equal
        """
type XX =
    { a: int
      b: int }

    static member private foo: int = 30
"""

[<Test>]
let ``keep comments after closing brace of single line records, 1096`` () =
    formatSourceString
        """
let formatConfig = { PageWidth = 70; Indent = 8 } // The number of spaces
type FormatConfig = { PageWidth: int; Indent: int } // The number of spaces
()
"""
        config
    |> prepend newline
    |> should
        equal
        """
let formatConfig = { PageWidth = 70; Indent = 8 } // The number of spaces
type FormatConfig = { PageWidth: int; Indent: int } // The number of spaces
()
"""

[<Test>]
let ``comment after closing brace in nested record`` () =
    formatSourceString
        """
let person =
    { Name = "James"
      Address = { Street = "Bakerstreet"; Number = 42 }  // end address
    } // end person
"""
        config
    |> prepend newline
    |> should
        equal
        """
let person =
    { Name = "James"
      Address = { Street = "Bakerstreet"; Number = 42 } // end address
    } // end person
"""

[<Test>]
let ``comment after closing brace of multi-line record type, 1124`` () =
    formatSourceString
        """
module Test =
    type t =
      { long_enough : string
        also_long_enough : string } // Hi I am a comment
"""
        config
    |> prepend newline
    |> should
        equal
        """
module Test =
    type t =
        { long_enough: string
          also_long_enough: string } // Hi I am a comment
"""

[<Test>]
let ``multiline SynPat.Record in match clause, 1173`` () =
    formatSourceString
        """
match foo with
| { Bar = bar
    Level = 12
    Vibes = plenty
    Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " } ->
    "7"
| _ -> "8"
"""
        config
    |> prepend newline
    |> should
        equal
        """
match foo with
| { Bar = bar
    Level = 12
    Vibes = plenty
    Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " } ->
    "7"
| _ -> "8"
"""

[<Test>]
let ``multiline SynPat.Record in let binding destructuring`` () =
    formatSourceString
        """
let internal sepSemi (ctx: Context) =
    let { Config = { SpaceBeforeSemicolon = before; SpaceAfterSemicolon = after } } = ctx

    match before, after with
    | false, false -> str ";"
    | true, false -> str " ;"
    | false, true -> str "; "
    | true, true -> str " ; "
    <| ctx
"""
        config
    |> prepend newline
    |> should
        equal
        """
let internal sepSemi (ctx: Context) =
    let { Config = { SpaceBeforeSemicolon = before
                     SpaceAfterSemicolon = after } } =
        ctx

    match before, after with
    | false, false -> str ";"
    | true, false -> str " ;"
    | false, true -> str "; "
    | true, true -> str " ; "
    <| ctx
"""

[<Test>]
let ``record with an access modifier and a static member, 1300, 657`` () =
    formatSourceString
        """
type RequestParser<'ctx, 'a> =
    internal
        { consumedFields: Set<ConsumedFieldName>
          parse: 'ctx -> Request -> Async<Result<'a, Error list>>
          prohibited: ProhibitedRequestGetter list }

        static member internal Create
            (
                consumedFields, parse: 'ctx -> Request -> Async<Result<'a, Error list>>
            ) : RequestParser<'ctx, 'a> =
            { consumedFields = consumedFields
              parse = parse
              prohibited = [] }

"""
        config
    |> prepend newline
    |> should
        equal
        """
type RequestParser<'ctx, 'a> =
    internal
        { consumedFields: Set<ConsumedFieldName>
          parse: 'ctx -> Request -> Async<Result<'a, Error list>>
          prohibited: ProhibitedRequestGetter list }

    static member internal Create
        (consumedFields, parse: 'ctx -> Request -> Async<Result<'a, Error list>>)
        : RequestParser<'ctx, 'a> =
        { consumedFields = consumedFields
          parse = parse
          prohibited = [] }
"""

[<Test>]
let ``access modifier on short type record inside module, 1404`` () =
    formatSourceString
        """
module Foo =
    type Stores =
        private {
            ModeratelyLongName : int
        }

    type private Bang = abstract Baz : int
"""
        { config with
            MaxLineLength = 40
            SpaceBeforeUppercaseInvocation = true }
    |> prepend newline
    |> should
        equal
        """
module Foo =
    type Stores =
        private
            { ModeratelyLongName: int }

    type private Bang =
        abstract Baz: int
"""

[<Test>]
let ``add space before parenthesis when function identifier is SynExpr.Const, 1476`` () =
    formatSourceString
        """
let x =
      { actual = 6
         y = x }

let y =
      { actual = 6
        y = x }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let x = { actual = 6 y = x }

let y = { actual = 6; y = x }
"""

[<Test>]
let ``multiline records in pattern list should not have semicolon by default, 1793`` () =
    formatSourceString
        """
match entities with
| [ { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
      Type = Elephant } ] -> ()
| _ -> ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
match entities with
| [ { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
      Type = Elephant } ] -> ()
| _ -> ()
"""

[<Test>]
let ``multiline records in pattern list`` () =
    formatSourceString
        """
match entities with
| [ { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
      Type = Elephant } ] -> ()
| _ -> ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
match entities with
| [ { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
      Type = Elephant }
    { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
      Type = Elephant } ] -> ()
| _ -> ()
"""

[<Test>]
let ``multiline records in pattern array should not have semicolon by default`` () =
    formatSourceString
        """
match entities with
| [| { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
       Type = Elephant }
     { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
       Type = Elephant }
     { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
       Type = Elephant } |] -> ()
| _ -> ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
match entities with
| [| { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d1"
       Type = Elephant }
     { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d2"
       Type = Elephant }
     { Key = "0031ff53-e59b-49e3-8e0f-53f72a3890d3"
       Type = Elephant } |] -> ()
| _ -> ()
"""

[<Test>]
let ``update record should indent from curly brace, 1876`` () =
    formatSourceString
        """
let rainbow2 =
    { rainbow with Boss = "Jeffrey" ; Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let rainbow2 =
    { rainbow with
        Boss = "Jeffrey"
        Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""

[<Test>]
let ``update record should indent from curly brace, indent size 2`` () =
    formatSourceString
        """
let rainbow2 =
  { rainbow with Boss = "Jeffrey" ; Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
let rainbow2 =
  { rainbow with
      Boss = "Jeffrey"
      Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""

[<Test>]
let ``update record should indent from curly brace, indent size 3`` () =
    formatSourceString
        """
let rainbow2 =
   { rainbow with Boss = "Jeffrey" ; Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""
        { config with IndentSize = 3 }
    |> prepend newline
    |> should
        equal
        """
let rainbow2 =
   { rainbow with
      Boss = "Jeffrey"
      Lackeys = [ "Zippy"; "George"; "Bungle" ] }
"""

[<Test>]
let ``record with comments above field`` () =
    formatSourceString
        """
{ Foo =
    // bar
    someValue}
"""
        config
    |> prepend newline
    |> should
        equal
        """
{ Foo =
    // bar
    someValue }
"""

[<Test>]
let ``record with comments above field, indent 2`` () =
    formatSourceString
        """
{ Foo =
    // bar
    someValue}
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
{ Foo =
    // bar
    someValue }
"""

[<Test>]
let ``record with comments above field, indent 3`` () =
    formatSourceString
        """
{ Foo =
    // bar
    someValue}
"""
        { config with IndentSize = 3 }
    |> prepend newline
    |> should
        equal
        """
{ Foo =
   // bar
   someValue }
"""

[<Test>]
let ``anonymous record with multiline field`` () =
    formatSourceString
        """
{| Foo =
              //  meh
              someValue |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
{| Foo =
    //  meh
    someValue |}
"""

[<Test>]
let ``anonymous record with multiline field, indent 2`` () =
    formatSourceString
        """
{| Foo =
              //  meh
              someValue |}
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
{| Foo =
    //  meh
    someValue |}
"""

[<Test>]
let ``anonymous record with multiline field, indent 3`` () =
    formatSourceString
        """
{| Foo =
              //  meh
              someValue |}
"""
        { config with IndentSize = 3 }
    |> prepend newline
    |> should
        equal
        """
{| Foo =
   //  meh
   someValue |}
"""

[<Test>]
let ``anonymous record with multiline field, indent 5`` () =
    formatSourceString
        """
{| Foo =
              //  meh
              someValue |}
"""
        { config with IndentSize = 5 }
    |> prepend newline
    |> should
        equal
        """
{| Foo =
     //  meh
     someValue |}
"""

[<Test>]
let ``a foo`` () =
    formatSourceString
        """
{| Foo =
              someValue
                //
                a |}
"""
        { config with IndentSize = 3 }
    |> prepend newline
    |> should
        equal
        """
{| Foo =
   someValue
      //
      a |}
"""

[<Test>]
let ``long record field assigment`` () =
    formatSourceString
        """
{ A =
    // one indent starting from {
    someFunctionCall
        arg1
        arg2
  B =
      // one indent starting from label B
      someFunctionCall
          arg1
          arg2 }
"""
        config
    |> prepend newline
    |> should
        equal
        """
{ A =
    // one indent starting from {
    someFunctionCall arg1 arg2
  B =
    // one indent starting from label B
    someFunctionCall arg1 arg2 }
"""

[<Test>]
let ``anonymous update record, indent_size 3`` () =
    formatSourceString
        """
{| f with Foo =
                        //  meh
                          someValue |}
"""
        { config with IndentSize = 3 }
    |> prepend newline
    |> should
        equal
        """
{| f with
      Foo =
         //  meh
         someValue |}
"""

[<Test>]
let ``restore correct indent after update record expression`` () =
    formatSourceString
        """
let parse (checker: FSharpChecker) (parsingOptions: FSharpParsingOptions) { FileName = fileName; Source = source } =
    let allDefineOptions, defineHashTokens = TokenParser.getDefines source

    allDefineOptions
    |> List.map (fun conditionalCompilationDefines ->
        async {
            let parsingOptionsWithDefines =
                { parsingOptions with
                      ConditionalCompilationDefines = conditionalCompilationDefines
                      SourceFiles = Array.map safeFileName parsingOptions.SourceFiles }
            // Run the first phase (untyped parsing) of the compiler
            let sourceText =
                FSharp.Compiler.Text.SourceText.ofString source

            let! untypedRes = checker.ParseFile(fileName, sourceText, parsingOptionsWithDefines)

            if untypedRes.ParseHadErrors then
                let errors =
                    untypedRes.Diagnostics
                    |> Array.filter (fun e -> e.Severity = FSharpDiagnosticSeverity.Error)

                if not <| Array.isEmpty errors then
                    raise
                    <| FormatException(
                        sprintf "Parsing failed with errors: %A\nAnd options: %A" errors parsingOptionsWithDefines
                    )

            return (untypedRes.ParseTree, conditionalCompilationDefines, defineHashTokens)
        })
    |> Async.Parallel
"""
        config
    |> prepend newline
    |> should
        equal
        """
let parse (checker: FSharpChecker) (parsingOptions: FSharpParsingOptions) { FileName = fileName; Source = source } =
    let allDefineOptions, defineHashTokens = TokenParser.getDefines source

    allDefineOptions
    |> List.map (fun conditionalCompilationDefines ->
        async {
            let parsingOptionsWithDefines =
                { parsingOptions with
                    ConditionalCompilationDefines = conditionalCompilationDefines
                    SourceFiles = Array.map safeFileName parsingOptions.SourceFiles }
            // Run the first phase (untyped parsing) of the compiler
            let sourceText = FSharp.Compiler.Text.SourceText.ofString source

            let! untypedRes = checker.ParseFile(fileName, sourceText, parsingOptionsWithDefines)

            if untypedRes.ParseHadErrors then
                let errors =
                    untypedRes.Diagnostics
                    |> Array.filter (fun e -> e.Severity = FSharpDiagnosticSeverity.Error)

                if not <| Array.isEmpty errors then
                    raise
                    <| FormatException(
                        sprintf "Parsing failed with errors: %A\nAnd options: %A" errors parsingOptionsWithDefines
                    )

            return (untypedRes.ParseTree, conditionalCompilationDefines, defineHashTokens)
        })
    |> Async.Parallel
"""

[<Test>]
let ``anonymous records with comments on record fields, 2067`` () =
    formatSourceString
        """
{|
    // The foo value.
    FooValue = fooValue
    // The bar value.
    BarValue = barValue
|}
"""
        config
    |> prepend newline
    |> should
        equal
        """
{|
   // The foo value.
   FooValue = fooValue
   // The bar value.
   BarValue = barValue |}
"""

[<Test>]
let ``keep copyInfo for record fixed at starting column, 2109`` () =
    formatSourceString
        """
let defaultTestOptions fwk common (o: DotNet.TestOptions) =
    { o.WithCommon(
          (fun o2 ->
              { o2 with
                    Verbosity = Some DotNet.Verbosity.Normal })
          >> common
      ) with
          NoBuild = true
          Framework = fwk // Some "netcoreapp3.0"
          Configuration = DotNet.BuildConfiguration.Debug }
"""
        { config with
            MaxInfixOperatorExpression = 50
            MaxRecordWidth = 55 }
    |> prepend newline
    |> should
        equal
        """
let defaultTestOptions fwk common (o: DotNet.TestOptions) =
    { o.WithCommon(
          (fun o2 -> { o2 with Verbosity = Some DotNet.Verbosity.Normal })
          >> common
      ) with
        NoBuild = true
        Framework = fwk // Some "netcoreapp3.0"
        Configuration = DotNet.BuildConfiguration.Debug }
"""

[<Test>]
let ``comment after equals in record field`` () =
    formatSourceString
        """
{ A = //comment 
      B }
"""
        config
    |> prepend newline
    |> should
        equal
        """
{ A = //comment
    B }
"""

[<Test>]
let ``comment after equals in anonymous record field`` () =
    formatSourceString
        """
{| A = //comment 
      B |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
{| A = //comment
    B |}
"""

[<Test>]
let ``comment after equals sign in record type definition`` () =
    formatSourceString
        """
type Foo =   // comment
    { Bar: int }
"""
        config
    |> prepend newline
    |> should
        equal
        """
type Foo = // comment
    { Bar: int }
"""

[<Test>]
let ``comment after equals sign in anonymous record field, 1921`` () =
    formatSourceString
        """
let a =
    {| Foo = //
        2 |}
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a =
    {| Foo = //
        2 |}
"""

[<Test>]
let ``update record in infix operation, 2355`` () =
    formatSourceString
        """
let rec meh () = ()
and seekReadParamExtras (retRes, paramsRes) (idx:int) =
    if seq = 0 then
        retRes := { !retRes with
                        //Marshal=(if hasMarshal then Some (fmReader (TaggedIndex(hfm_ParamDef, idx))) else None);
                        CustomAttrs = cas }
    else
        paramsRes.[seq - 1] <-
            { paramsRes.[seq - 1] with
                //Marshal=(if hasMarshal then Some (fmReader (TaggedIndex(hfm_ParamDef, idx))) else None)
                Default = (if hasDefault then USome (seekReadConstant (TaggedIndex(HasConstantTag.ParamDef, idx))) else UNone)
                Name = readStringHeapOption nameIdx
                Attributes = enum<ParameterAttributes> flags
                CustomAttrs = cas }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let rec meh () = ()

and seekReadParamExtras (retRes, paramsRes) (idx: int) =
    if seq = 0 then
        retRes
        := { !retRes with
               //Marshal=(if hasMarshal then Some (fmReader (TaggedIndex(hfm_ParamDef, idx))) else None);
               CustomAttrs = cas }
    else
        paramsRes.[seq - 1] <-
            { paramsRes.[seq - 1] with
                //Marshal=(if hasMarshal then Some (fmReader (TaggedIndex(hfm_ParamDef, idx))) else None)
                Default =
                    (if hasDefault then
                         USome(seekReadConstant (TaggedIndex(HasConstantTag.ParamDef, idx)))
                     else
                         UNone)
                Name = readStringHeapOption nameIdx
                Attributes = enum<ParameterAttributes> flags
                CustomAttrs = cas }
"""

[<Test>]
let ``equality comparison with a `with` expression should format correctly with default alignment, 2507`` () =
    formatSourceString
        """
let compareThings (first: Thing) (second: Thing) =
    first = { second with
                Foo = first.Foo
                Bar = first.Bar
            }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let compareThings (first: Thing) (second: Thing) =
    first = { second with
                Foo = first.Foo
                Bar = first.Bar }
"""

[<Test>]
let ``multiline record field type annotation`` () =
    formatSourceString
        """
type ExprFolder<'State> =
    { exprIntercept: ('State -> Expr -> 'State) -> ('State -> Expr -> 'State) -> 'State -> Exp -> 'State }
"""
        { config with MaxLineLength = 80 }
    |> prepend newline
    |> should
        equal
        """
type ExprFolder<'State> =
    { exprIntercept:
        ('State -> Expr -> 'State)
            -> ('State -> Expr -> 'State)
            -> 'State
            -> Exp
            -> 'State }
"""

[<Test>]
let ``multiline anonymous record field type annotation`` () =
    formatSourceString
        """
type ExprFolder<'State> =
    {| exprIntercept: ('State -> Expr -> 'State) -> ('State -> Expr -> 'State) -> 'State -> Exp -> 'State |}
"""
        { config with MaxLineLength = 80 }
    |> prepend newline
    |> should
        equal
        """
type ExprFolder<'State> =
    {| exprIntercept:
        ('State -> Expr -> 'State)
            -> ('State -> Expr -> 'State)
            -> 'State
            -> Exp
            -> 'State |}
"""

[<Test>]
let ``comment in bracket ranges of anonymous type`` () =
    formatSourceString
        """
let x = {| // test1
    Y = 42
    Z = "string"
    Foo = "Bar"
    // test2
    |}

let y = {|
    Y = 42
    // test
|}

let z = {|
    Y = 42
|}

let a = {| // test1
    foo with
        Level = 7
        Square = 9
        // test2
|}
"""
        config
    |> prepend newline
    |> should
        equal
        """
let x =
    {| // test1
       Y = 42
       Z = "string"
       Foo = "Bar"
    // test2
    |}

let y =
    {| Y = 42
    // test
    |}

let z = {| Y = 42 |}

let a =
    {| // test1
    foo with
        Level = 7
        Square = 9
    // test2
    |}
"""

[<Test>]
let ``multiline field body expression where indent_size = 2, 2801`` () =
    formatSourceString
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
    let range =
      { Start =
          { Line = range.StartLine - 1
            Character = range.StartColumn }
        End =
          { Line = range.EndLine - 1
            Character = range.EndColumn } }

    [| { Range = range; NewText = formatted } |]
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
  let range =
    { Start =
        { Line = range.StartLine - 1
          Character = range.StartColumn }
      End =
        { Line = range.EndLine - 1
          Character = range.EndColumn } }

  [| { Range = range; NewText = formatted } |]
"""

[<Test>]
let ``multiline field body expression where indent_size = 2, anonymous record`` () =
    formatSourceString
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
    let range =
      {| Start =
           { Line = range.StartLine - 1
             Character = range.StartColumn }
         End =
           { Line = range.EndLine - 1
             Character = range.EndColumn } |}

    [| { Range = range; NewText = formatted } |]
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
  let range =
    {| Start =
        { Line = range.StartLine - 1
          Character = range.StartColumn }
       End =
        { Line = range.EndLine - 1
          Character = range.EndColumn } |}

  [| { Range = range; NewText = formatted } |]
"""

[<Test>]
let ``multiline field body expression where indent_size = 2, update record`` () =
    formatSourceString
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
    let range =
      { x with 
            Start =
              { Line = range.StartLine - 1
                Character = range.StartColumn }
            End =
              { Line = range.EndLine - 1
                Character = range.EndColumn } }

    [| { Range = range; NewText = formatted } |]
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
  let range =
    { x with
        Start =
          { Line = range.StartLine - 1
            Character = range.StartColumn }
        End =
          { Line = range.EndLine - 1
            Character = range.EndColumn } }

  [| { Range = range; NewText = formatted } |]
"""

let ``multiline field body expression where indent_size = 2, inherit record`` () =
    formatSourceString
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
  let range =
    { inherit Foo()
      Start =
        { Line = range.StartLine - 1
          Character = range.StartColumn }
      End =
        { Line = range.EndLine - 1
          Character = range.EndColumn } }
  [| { Range = range; NewText = formatted } |]
"""
        { config with IndentSize = 2 }
    |> prepend newline
    |> should
        equal
        """
let handlerFormattedRangeDoc (lines: NamedText, formatted: string, range: FormatSelectionRange) =
  let range =
    { inherit Foo()
      Start =
        { Line = range.StartLine - 1
          Character = range.StartColumn }
      End =
        { Line = range.EndLine - 1
          Character = range.EndColumn } }

  [| { Range = range; NewText = formatted } |]
"""

[<Test>]
let ``trivia after opening brace in inherit expression, 2803`` () =
    formatSourceString
        """
let range =
    { // foo
      // bar
      inherit // reason
        Foo()
      X = y
      Z =
        someReallyLongExpressionThatIsLongerThanTheLineLength
            aLongArgument
            //
            anotherLongArgument
            fooBar }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let range =
    { // foo
      // bar
      inherit // reason
          Foo()
      X = y
      Z =
        someReallyLongExpressionThatIsLongerThanTheLineLength
            aLongArgument
            //
            anotherLongArgument
            fooBar }
"""
