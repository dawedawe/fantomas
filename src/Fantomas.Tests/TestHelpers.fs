module Fantomas.Tests.TestHelpers

open System
open System.Diagnostics
open System.IO
open System.Text
open Fantomas

[<RequireQualifiedAccess>]
module String =
    let normalizeNewLine (str: string) =
        str.Replace("\r\n", "\n").Replace("\r", "\n")

type TemporaryFileCodeSample
    internal
    (
        codeSnippet: string,
        ?hasByteOrderMark: bool,
        ?fileName: string,
        ?subFolder: string,
        ?subFolders: string array,
        ?extension: string
    ) =
    let hasByteOrderMark = defaultArg hasByteOrderMark false

    let internalSubFolders =
        match subFolders with
        | Some sf -> Some sf
        | None ->
            match subFolder with
            | Some sf -> Array.singleton sf |> Some
            | None -> None

    let filename =
        let name =
            match fileName with
            | Some fn -> fn
            | None -> Guid.NewGuid().ToString()

        let extension = Option.defaultValue "fs" extension

        match internalSubFolders with
        | Some sf ->
            let tempFolder = Path.Join(Path.GetTempPath(), Path.Join(sf))

            if not (Directory.Exists(tempFolder)) then
                Directory.CreateDirectory(tempFolder) |> ignore

            Path.Join(tempFolder, sprintf "%s.%s" name extension)
        | None -> Path.Join(Path.GetTempPath(), sprintf "%s.%s" name extension)

    do
        (if hasByteOrderMark then
             File.WriteAllText(filename, codeSnippet, Encoding.UTF8)
         else
             File.WriteAllText(filename, codeSnippet))

    member _.Filename: string = filename

    interface IDisposable with
        member this.Dispose() : unit =
            File.Delete(filename)

            internalSubFolders
            |> Option.iter (fun sf ->
                let path = Path.Join(Path.GetTempPath(), sf.[0])
                Directory.Delete(path, true))

type OutputFile internal () =
    let filename = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString() + ".fs")

    member _.Filename: string = filename

    interface IDisposable with
        member this.Dispose() : unit =
            if File.Exists(filename) then
                File.Delete(filename)

type OutputFolder internal () =
    let foldername = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString())

    member _.Foldername: string = foldername

    interface IDisposable with
        member this.Dispose() : unit =
            if Directory.Exists(foldername) then
                Directory.Delete(foldername, true)

type ConfigurationFile internal (content: string) =
    let filename = Path.Join(Path.GetTempPath(), ".editorconfig")

    do File.WriteAllText(filename, content)
    member _.Filename: string = filename

    interface IDisposable with
        member this.Dispose() : unit =
            if File.Exists(filename) then
                File.Delete(filename)

type FantomasIgnoreFile internal (content: string) =
    let filename = Path.Join(Path.GetTempPath(), IgnoreFile.IgnoreFileName)

    do File.WriteAllText(filename, content)
    member _.Filename: string = filename

    interface IDisposable with
        member this.Dispose() : unit =
            if File.Exists(filename) then
                File.Delete(filename)

type FantomasToolResult =
    { ExitCode: int
      Output: string
      Error: string }

let getFantomasToolStartInfo arguments : ProcessStartInfo =
    let pwd = Path.GetDirectoryName(typeof<TemporaryFileCodeSample>.Assembly.Location)

    let configuration =
#if DEBUG
        "debug"
#else
        "release"
#endif

    let fantomasDll =
        Path.Combine(pwd, "..", "..", "Fantomas", configuration, "fantomas.dll")
        |> Path.GetFullPath

    if not (File.Exists fantomasDll) then
        failwithf $"The fantomas dll at \"%s{fantomasDll}\" does not exist!"

    let argumentArray = fantomasDll :: arguments
    let startInfo = ProcessStartInfo("dotnet", argumentArray)
    startInfo.UseShellExecute <- false
    startInfo.WorkingDirectory <- Path.GetTempPath()
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo

let runFantomasTool arguments : FantomasToolResult =
    use p = getFantomasToolStartInfo arguments |> Process.Start

    let output = p.StandardOutput.ReadToEnd()
    let error = p.StandardError.ReadToEnd()
    p.WaitForExit()

    { ExitCode = p.ExitCode
      Output = output
      Error = error }

let checkCode (files: string list) : FantomasToolResult =
    let arguments = "--check" :: files
    runFantomasTool arguments

let formatCode (files: string list) : FantomasToolResult = runFantomasTool files
