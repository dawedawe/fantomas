﻿module Fantomas.Daemon

open System
open System.Diagnostics
open System.IO
open System.IO.Abstractions
open System.Threading
open System.Threading.Tasks
open StreamJsonRpc
open Thoth.Json.Net
open Fantomas.FCS.Text
open Fantomas.Client.Contracts
open Fantomas.Client.LSPFantomasServiceTypes
open Fantomas.Core
open Fantomas.EditorConfig

type FantomasDaemon(sender: Stream, reader: Stream) as this =
    let rpc: JsonRpc = JsonRpc.Attach(sender, reader, this)
    let traceListener = new DefaultTraceListener()

    do
        // hook up request/response logging for debugging
        rpc.TraceSource <- TraceSource(typeof<FantomasDaemon>.Name, SourceLevels.Verbose)
        rpc.TraceSource.Listeners.Add traceListener |> ignore<int>

    let disconnectEvent = new ManualResetEvent(false)

    let exit () = disconnectEvent.Set() |> ignore

    let fs = FileSystem()

    do rpc.Disconnected.Add(fun _ -> exit ())

    interface IDisposable with
        member this.Dispose() =
            traceListener.Dispose()
            disconnectEvent.Dispose()

    /// returns a hot task that resolves when the stream has terminated
    member this.WaitForClose = rpc.Completion

    [<JsonRpcMethod(Methods.Version)>]
    member _.Version() : string = CodeFormatter.GetVersion()

    [<JsonRpcMethod(Methods.FormatDocument, UseSingleObjectParameterDeserialization = true)>]
    member _.FormatDocumentAsync(request: FormatDocumentRequest) : Task<FormatDocumentResponse> =
        task {
            if
                IgnoreFile.isIgnoredFile
                    (IgnoreFile.find fs (IgnoreFile.loadIgnoreList fs) request.FilePath)
                    request.FilePath
            then
                return FormatDocumentResponse.IgnoredFile request.FilePath
            else
                let config =
                    match request.Config with
                    | Some configProperties ->
                        let config = readConfiguration request.FilePath
                        parseOptionsFromEditorConfig config configProperties
                    | None -> readConfiguration request.FilePath

                let cursor =
                    request.Cursor
                    |> Option.map (fun cursor -> CodeFormatter.MakePosition(cursor.Line, cursor.Column))

                try
                    let! formatResponse =
                        match cursor with
                        | None -> CodeFormatter.FormatDocumentAsync(request.IsSignatureFile, request.SourceCode, config)
                        | Some cursor ->
                            CodeFormatter.FormatDocumentAsync(
                                request.IsSignatureFile,
                                request.SourceCode,
                                config,
                                cursor
                            )

                    if formatResponse.Code = request.SourceCode then
                        return FormatDocumentResponse.Unchanged request.FilePath
                    else
                        let cursor =
                            formatResponse.Cursor
                            |> Option.map (fun cursorPos -> FormatCursorPosition(cursorPos.Line, cursorPos.Column))

                        return FormatDocumentResponse.Formatted(request.FilePath, formatResponse.Code, cursor)
                with ex ->
                    return FormatDocumentResponse.Error(request.FilePath, ex.Message)
        }

    [<JsonRpcMethod(Methods.FormatSelection, UseSingleObjectParameterDeserialization = true)>]
    member _.FormatSelectionAsync(request: FormatSelectionRequest) : Task<FormatSelectionResponse> =
        task {
            let config =
                match request.Config with
                | Some configProperties ->
                    let config = readConfiguration request.FilePath
                    parseOptionsFromEditorConfig config configProperties
                | None -> readConfiguration request.FilePath

            let selection =
                let r = request.Range

                Range.mkRange
                    request.FilePath
                    (Position.mkPos r.StartLine r.StartColumn)
                    (Position.mkPos r.EndLine r.EndColumn)

            try
                let! formatted, actualSelection =
                    CodeFormatter.FormatSelectionAsync(request.IsSignatureFile, request.SourceCode, selection, config)

                let actualSelection =
                    FormatSelectionRange(
                        actualSelection.StartLine,
                        actualSelection.StartColumn,
                        actualSelection.EndLine,
                        actualSelection.EndColumn
                    )

                return FormatSelectionResponse.Formatted(request.FilePath, formatted, actualSelection)
            with ex ->
                return FormatSelectionResponse.Error(request.FilePath, ex.Message)
        }

    [<JsonRpcMethod(Methods.Configuration)>]
    member _.Configuration() : string =
        let settings =
            Reflection.getRecordFields FormatConfig.Default
            |> Array.toList
            |> List.choose (fun (recordField, defaultValue) ->
                let optionalField key value =
                    value |> Option.toList |> List.map (fun v -> key, Encode.string v)

                let meta =
                    List.concat
                        [| optionalField "category" recordField.Category
                           optionalField "displayName" recordField.DisplayName
                           optionalField "description" recordField.Description |]

                let type' =
                    match defaultValue with
                    | :? bool as b ->
                        Some(
                            Encode.object
                                [ yield "type", Encode.string "boolean"
                                  yield "defaultValue", Encode.string (if b then "true" else "false")
                                  yield! meta ]
                        )
                    | :? int as i ->
                        Some(
                            Encode.object
                                [ yield "type", Encode.string "number"
                                  yield "defaultValue", Encode.string (string<int> i)
                                  yield! meta ]
                        )
                    | :? MultilineFormatterType as m ->
                        Some(
                            Encode.object
                                [ yield "type", Encode.string "multilineFormatterType"
                                  yield "defaultValue", Encode.string (MultilineFormatterType.ToConfigString m)
                                  yield! meta ]
                        )
                    | :? EndOfLineStyle as e ->
                        Some(
                            Encode.object
                                [ yield "type", Encode.string "endOfLineStyle"
                                  yield "defaultValue", Encode.string (EndOfLineStyle.ToConfigString e)
                                  yield! meta ]
                        )
                    | :? MultilineBracketStyle as m ->
                        Some(
                            Encode.object
                                [ yield "type", Encode.string "multilineBracketStyle"
                                  yield "defaultValue", Encode.string (MultilineBracketStyle.ToConfigString m)
                                  yield! meta ]
                        )
                    | _ -> None

                type' |> Option.map (fun t -> toEditorConfigName recordField.PropertyName, t))
            |> Encode.object

        let enumOptions =
            Encode.object
                [ "multilineFormatterType",
                  Encode.list
                      [ (MultilineFormatterType.ToConfigString MultilineFormatterType.CharacterWidth
                         |> Encode.string)
                        (MultilineFormatterType.ToConfigString MultilineFormatterType.NumberOfItems
                         |> Encode.string) ]
                  "endOfLineStyle",
                  Encode.list
                      [ (EndOfLineStyle.ToConfigString EndOfLineStyle.LF |> Encode.string)
                        (EndOfLineStyle.ToConfigString EndOfLineStyle.CRLF |> Encode.string) ]
                  "multilineBracketStyle",
                  Encode.list
                      [ (MultilineBracketStyle.ToConfigString Aligned |> Encode.string)
                        (MultilineBracketStyle.ToConfigString Cramped |> Encode.string)
                        (MultilineBracketStyle.ToConfigString Stroustrup |> Encode.string) ] ]

        Encode.object [ "settings", settings; "enumOptions", enumOptions ]
        |> Encode.toString 4
