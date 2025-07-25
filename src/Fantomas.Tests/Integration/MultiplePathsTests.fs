module Fantomas.Tests.Integration.MultiplePathsTests

open NUnit.Framework
open FsUnit
open Fantomas.Tests.TestHelpers
open System.IO

[<Literal>]
let UserCode = "let a  =   9"

[<Literal>]
let FormattedCode = "let a = 9\n"

let Verbosity = [ "--verbosity"; "d" ]

let private fileContentMatches (expectedContent: string) (actualPath: string) : unit =
    if File.Exists(actualPath) then
        let actualContent = File.ReadAllText(actualPath)
        actualContent |> should equal expectedContent
    else
        Assert.Fail(sprintf "File \"%s\" does not exist" actualPath)

[<Test>]
let ``format multiple paths`` () =
    use config = new ConfigurationFile("[*]\nend_of_line = lf")

    use fileFixtureOne = new TemporaryFileCodeSample(UserCode)

    use fileFixtureTwo = new TemporaryFileCodeSample(UserCode)

    let { ExitCode = exitCode } =
        formatCode [ fileFixtureOne.Filename; fileFixtureTwo.Filename ]

    exitCode |> should equal 0

    fileContentMatches FormattedCode fileFixtureOne.Filename
    fileContentMatches FormattedCode fileFixtureTwo.Filename

[<Test>]
let ``format multiple paths recursively`` () =
    use config = new ConfigurationFile("[*]\nend_of_line = lf")

    use fileFixtureOne = new TemporaryFileCodeSample(UserCode)

    use fileFixtureTwo = new TemporaryFileCodeSample(UserCode)

    use fileFixtureThree = new TemporaryFileCodeSample(UserCode, subFolder = "sub")

    let arguments =
        Verbosity @ [ fileFixtureOne.Filename; fileFixtureTwo.Filename; "sub" ]

    let { ExitCode = exitCode; Output = output } = runFantomasTool arguments

    exitCode |> should equal 0
    output |> should contain fileFixtureOne.Filename
    output |> should contain fileFixtureTwo.Filename

    let subFolder =
        sprintf "sub%c%s" Path.DirectorySeparatorChar (Path.GetFileName(fileFixtureThree.Filename))

    output |> should contain subFolder
