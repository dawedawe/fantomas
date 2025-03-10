module Fantomas.Core.Tests.OperatorTests

open NUnit.Framework
open FsUnit
open Fantomas.Core.Tests.TestHelpers
open Fantomas.Core

[<Test>]
let ``should keep parens around ? operator definition`` () =
    formatSourceString
        """let (?) f s = f s
    """
        config
    |> should
        equal
        """let (?) f s = f s
"""

[<Test>]
let ``should keep parens around ?<- operator definition`` () =
    formatSourceString
        """let (?<-) f s = f s
    """
        config
    |> should
        equal
        """let (?<-) f s = f s
"""

[<Test>]
let ``should keep parens around ++ infix operator definition`` () =
    formatSourceString
        """let (++) x y = { x with Includes = y :: x.Includes }
    """
        config
    |> should
        equal
        """let (++) x y = { x with Includes = y :: x.Includes }
"""

[<Test>]
let ``should keep parens around inlined ==> operator definition`` () =
    formatSourceString
        """let inline (==>) x y = f x y
    """
        config
    |> should
        equal
        """let inline (==>) x y = f x y
"""

[<Test>]
let ``should keep parens around inlined operator definition`` () =
    formatSourceString
        """let inline (@@) path1 path2 = Path.Combine(path1, path2)
    """
        config
    |> should
        equal
        """let inline (@@) path1 path2 = Path.Combine(path1, path2)
"""

[<Test>]
let ``should pattern match on quotation expression`` () =
    formatSourceString
        """let rec print expr =
    match expr with
    | SpecificCall <@@ (+) @@> (_, _, exprList) ->
        print exprList.Head
        printf " + "
        print exprList.Tail.Head
    | _ -> ()"""
        config
    |> should
        equal
        """let rec print expr =
    match expr with
    | SpecificCall <@@ (+) @@> (_, _, exprList) ->
        print exprList.Head
        printf " + "
        print exprList.Tail.Head
    | _ -> ()
"""

[<Test>]
let ``should break on . operator`` () =
    formatSourceString
        """pattern.Replace(".", @"\.").Replace("$", @"\$").Replace("^", @"\^").Replace("{", @"\{").Replace("[", @"\[").Replace("(", @"\(").Replace(")", @"\)").Replace("+", @"\+")

    """
        { config with MaxLineLength = 80 }
    |> prepend newline
    |> should
        equal
        """
pattern
    .Replace(".", @"\.")
    .Replace("$", @"\$")
    .Replace("^", @"\^")
    .Replace("{", @"\{")
    .Replace("[", @"\[")
    .Replace("(", @"\(")
    .Replace(")", @"\)")
    .Replace("+", @"\+")
"""

// the current behavior results in a compile error since line break is before the parens and not before the .
[<Test>]
let ``should break on . operator and keep indentation`` () =
    formatSourceString
        """let pattern =
    (x + y)
      .Replace(seperator + "**" + seperator, replacementSeparator + "(.|?" + replacementSeparator + ")?" )
      .Replace("**" + seperator, ".|(?<=^|" + replacementSeparator + ")" )
    """
        { config with
            MaxLineLength = 80
            MaxInfixOperatorExpression = 60 }
    |> should
        equal
        """let pattern =
    (x + y)
        .Replace(
            seperator + "**" + seperator,
            replacementSeparator + "(.|?" + replacementSeparator + ")?"
        )
        .Replace("**" + seperator, ".|(?<=^|" + replacementSeparator + ")")
"""

[<Test>]
let ``should keep space between ( and * in *** operator definition`` () =
    formatSourceString
        """let inline ( ***) l1 l2 = pair l2 l1
    """
        config
    |> should
        equal
        """let inline ( *** ) l1 l2 = pair l2 l1
"""

[<Test>]
let ``should keep space between ( and * in *= operator definition`` () =
    formatSourceString
        """let inline ( *=) l v = update (( *) v) l
    """
        config
    |> should
        equal
        """let inline ( *= ) l v = update ((*) v) l
"""

[<Test>]
let ``should not add space around ? operator`` () =
    formatSourceString """let x = y?z.d?c.[2]?d.xpto()""" config
    |> should
        equal
        """let x = y?z.d?c.[2]?d.xpto ()
"""

[<Test>]
let ``should understand ? as an infix operator`` () =
    formatSourceString
        """try
    item.MethodInfo.Method.Invoke(null, ipa)
    |> (fun x -> x?Invoke (true))
    |> fun (t : Task) -> t.Wait()
with _ -> ()"""
        config
    |> prepend newline
    |> should
        equal
        """
try
    item.MethodInfo.Method.Invoke(null, ipa)
    |> (fun x -> x?Invoke (true))
    |> fun (t: Task) -> t.Wait()
with _ ->
    ()
"""

[<Test>]
let ``should not mess up ?<- operator`` () =
    formatSourceString """x?v <- 2""" config
    |> should
        equal
        """x?v <- 2
"""

[<Test>]
let ``should pipeline monadic bind`` () =
    formatSourceString
        """strToInt "1"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
"""
        config
    |> should
        equal
        """strToInt "1"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
>>= strAddLong "A long argument that is ignored" "2"
"""

[<Test>]
let ``should keep >>.~ operator`` () =
    formatSourceString
        """let (>>.~) (g : int) (h : int) : int = g + h
let output = 2 >>.~ 3
    """
        config
    |> should
        equal
        """let (>>.~) (g: int) (h: int) : int = g + h
let output = 2 >>.~ 3
"""

[<Test>]
let ``should not add newline before = operator after |>`` () =
    formatSourceString
        """1 |> max 0 = 1"""
        { config with
            MaxInfixOperatorExpression = 15 }
    |> should
        equal
        """1 |> max 0 = 1
"""

[<Test>]
let ``should not add space around .. operator`` () =
    formatSourceString """[1..10]""" config
    |> should
        equal
        """[ 1..10 ]
"""

[<Test>]
let ``should not add space around .. .. operators`` () =
    formatSourceString """[10 .. -1 .. 1]""" config
    |> should
        equal
        """[ 10..-1..1 ]
"""

[<Test>]
let ``line comment after infix function with parenthesis, 559`` () =
    formatSourceString
        """let watchFiles =
        async {
            printfn "after start"
            use _ =
                !!(serverPath </> "*.fs") ++ (serverPath </> "*.fsproj") // combines fs and fsproj
                |> ChangeWatcher.run (fun changes ->
                                      printfn
                                          "FILE CHANGE %A"
                                          changes
                                      // stopFunc()
                                      //Async.Start (startFunc())
                                      )
            ()
        }
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let watchFiles =
    async {
        printfn "after start"

        use _ =
            !!(serverPath </> "*.fs")
            ++ (serverPath </> "*.fsproj") // combines fs and fsproj
            |> ChangeWatcher.run (fun changes -> printfn "FILE CHANGE %A" changes
            // stopFunc()
            //Async.Start (startFunc())
            )

        ()
    }
"""

[<Test>]
let ``line comment after infix function with string constant, 559`` () =
    formatSourceString
        """let watchFiles =
        async {
            printfn "after start"
            use _ =
                !!(serverPath </> "*.fs") ++ "*.fsproj" // combines fs and fsproj
                |> ChangeWatcher.run (fun changes ->
                                      printfn
                                          "FILE CHANGE %A"
                                          changes
                                      // stopFunc()
                                      //Async.Start (startFunc())
                                      )
            ()
        }
"""
        config
    |> prepend newline
    |> should
        equal
        """
let watchFiles =
    async {
        printfn "after start"

        use _ =
            !!(serverPath </> "*.fs") ++ "*.fsproj" // combines fs and fsproj
            |> ChangeWatcher.run (fun changes -> printfn "FILE CHANGE %A" changes
            // stopFunc()
            //Async.Start (startFunc())
            )

        ()
    }
"""

[<Test>]
let ``short expression before and after pipe`` () =
    formatSourceString "let a = b |> c" config
    |> prepend newline
    |> should
        equal
        """
let a = b |> c
"""

[<Test>]
let ``long expression with pipe should be multiline`` () =
    formatSourceString
        "let a = List.init 40 (fun i -> generateThing i a) |> List.map mapThingToOtherThing"
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let a =
    List.init 40 (fun i -> generateThing i a)
    |> List.map mapThingToOtherThing
"""

[<Test>]
let ``giraffe sample`` () =
    formatSourceString
        """
let WebApp = route "/ping" >=> authorized >=> text "pong"
"""
        { config with
            MaxInfixOperatorExpression = 20 }
    |> prepend newline
    |> should
        equal
        """
let WebApp =
    route "/ping"
    >=> authorized
    >=> text "pong"
"""

[<Test>]
let ``multiple short pipes`` () =
    formatSourceString
        """let result = a && b |>  f |>  g |>   h
"""
        config
    |> prepend newline
    |> should
        equal
        """
let result = a && b |> f |> g |> h
"""

[<Test>]
let ``pipe boolean expression`` () =
    formatSourceString
        """b && c |> someLongExpressionThatShouldMoveThePipeToTheNextLine
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
b
&& c
   |> someLongExpressionThatShouldMoveThePipeToTheNextLine
"""

[<Test>]
let ``two long boolean expressions`` () =
    formatSourceString
        """aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa || bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
"""
        config
    |> prepend newline
    |> should
        equal
        """
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
|| bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb
"""

[<Test>]
let ``equal sign operator should not move to next line`` () =
    formatSourceString
        """let result =
            (typ.GetInterface(typeof<System.Collections.IEnumerable>.FullName) = null)
"""
        config
    |> prepend newline
    |> should
        equal
        """
let result =
    (typ.GetInterface(typeof<System.Collections.IEnumerable>.FullName) = null)
"""

[<Test>]
let ``function call before pipe operator, 754`` () =
    formatSourceString
        "
[<Test>]
let ``attribute on module after namespace`` () =
    formatSourceString false \"\"\"namespace SomeNamespace

[<AutoOpen>]
module Types =
    let a = 5
\"\"\"  config
    |> prepend newline
    |> should equal \"\"\"
namespace SomeNamespace

[<AutoOpen>]
module Types =
    let a = 5
\"\"\"
"
        config
    |> prepend newline
    |> should
        equal
        "
[<Test>]
let ``attribute on module after namespace`` () =
    formatSourceString
        false
        \"\"\"namespace SomeNamespace

[<AutoOpen>]
module Types =
    let a = 5
\"\"\"
        config
    |> prepend newline
    |> should
        equal
        \"\"\"
namespace SomeNamespace

[<AutoOpen>]
module Types =
    let a = 5
\"\"\"
"

[<Test>]
let ``modulo operator on same line, 780`` () =
    formatSourceString
        """let hasUnEvenAmount regex line = (Regex.Matches(line, regex).Count - Regex.Matches(line, "\\\\" + regex).Count) % 2 = 1
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let hasUnEvenAmount regex line =
    (Regex.Matches(line, regex).Count
     - Regex.Matches(line, "\\\\" + regex).Count) % 2 = 1
"""

[<Test>]
let ``parameter after multiline string, 783`` () =
    formatSourceString
        "
let ``match bang`` () =
    formatSourceString false \"\"\"
async {
    match! myAsyncFunction() with
    | Some x -> printfn \"%A\" x
    | None -> printfn \"Function returned None!\"
}\"\"\"   config
    |> prepend newline
    |> should equal \"\"\"
async {
    match! myAsyncFunction () with
    | Some x -> printfn \"%A\" x
    | None -> printfn \"Function returned None!\"
}
\"\"\"
"
        config
    |> prepend newline
    |> should
        equal
        "
let ``match bang`` () =
    formatSourceString
        false
        \"\"\"
async {
    match! myAsyncFunction() with
    | Some x -> printfn \"%A\" x
    | None -> printfn \"Function returned None!\"
}\"\"\"
        config
    |> prepend newline
    |> should
        equal
        \"\"\"
async {
    match! myAsyncFunction () with
    | Some x -> printfn \"%A\" x
    | None -> printfn \"Function returned None!\"
}
\"\"\"
"

[<Test>]
let ``addition via function`` () =
    formatSourceString
        """let a = (+) 7 8
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a = (+) 7 8
"""

[<Test>]
let ``lambda piped to lambda should be multiline, 942`` () =
    formatSourceString
        """
let r (f : 'a -> 'b) (a : 'a) : 'b =
    fun () ->
        f a
    |> fun f -> f ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
let r (f: 'a -> 'b) (a: 'a) : 'b =
    fun () -> f a
    |> fun f -> f ()
"""

[<Test>]
let ``combining lines breaks function precedence 488`` () =
    formatSourceString
        """fun () -> ()
|> Some
"""
        config
    |> prepend newline
    |> should
        equal
        """
fun () -> ()
|> Some
"""

[<Test>]
let ``function with LPAREN_STAR_RPAREN`` () =
    formatSourceString
        """
let private distanceBetweenTwoPoints (latA, lngA) (latB, lngB) =
    if latA = latB && lngA = lngB then
        0.
    else
        let theta = lngA - lngB

        let dist =
            Math.Sin(deg2rad (latA))
            * Math.Sin(deg2rad (latB))
            + (Math.Cos(deg2rad (latA))
               * Math.Cos(deg2rad (latB))
               * Math.Cos(deg2rad (theta)))
            |> Math.Acos
            |> rad2deg
            |> (*) (60. * 1.1515 * 1.609344)

        dist
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let private distanceBetweenTwoPoints (latA, lngA) (latB, lngB) =
    if latA = latB && lngA = lngB then
        0.
    else
        let theta = lngA - lngB

        let dist =
            Math.Sin(deg2rad (latA))
            * Math.Sin(deg2rad (latB))
            + (Math.Cos(deg2rad (latA))
               * Math.Cos(deg2rad (latB))
               * Math.Cos(deg2rad (theta)))
            |> Math.Acos
            |> rad2deg
            |> (*) (60. * 1.1515 * 1.609344)

        dist
"""

[<Test>]
let ``keep comment after or operator, 1095`` () =
    formatSourceString
        """
let f x =
    a
    || // other case
    match n with
    | 17 -> false
    | _ -> true
"""
        config
    |> prepend newline
    |> should
        equal
        """
let f x =
    a
    || // other case
    match n with
    | 17 -> false
    | _ -> true
"""

[<Test>]
let ``keep comment after and operator`` () =
    formatSourceString
        "
let r =
    {| Foo =
           a
           && // && b
           c
       Bar = \"\"\"
Fooey
\"\"\" |}
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
let ``simple math`` () =
    formatSourceString
        """let myValue = a + b * c
"""
        { config with
            MaxInfixOperatorExpression = 5 }
    |> prepend newline
    |> should
        equal
        """
let myValue =
    a
    + b * c
"""

[<Test>]
let ``simple math in one line`` () =
    formatSourceString
        """let myValue = a + b * c
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let myValue = a + b * c
"""

[<Test>]
let ``simple math reversed`` () =
    formatSourceString
        """let myValue = a * b + c
"""
        { config with
            MaxInfixOperatorExpression = 5 }
    |> prepend newline
    |> should
        equal
        """
let myValue =
    a * b
    + c
"""

[<Test>]
let ``multiple sum operators`` () =
    formatSourceString
        """let myValue = a + b * c + d
"""
        { config with
            MaxInfixOperatorExpression = 5 }
    |> prepend newline
    |> should
        equal
        """
let myValue =
    a
    + b * c
    + d
"""

[<Test>]
let ``nested math sample`` () =
    formatSourceString
        """
        let dist =
            aaaaaaaaaaaaaaaaaaaaaaaa
            * bbbbbbbbbbbbbbbbbbbbbbbbb
            + (ccccccccccccccccccccccccc
               * ddddddddddddddddddddddd
               * eeeeeeeeeeeeeeeeeeeeeee)
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
let dist =
    aaaaaaaaaaaaaaaaaaaaaaaa
    * bbbbbbbbbbbbbbbbbbbbbbbbb
    + (ccccccccccccccccccccccccc
       * ddddddddddddddddddddddd
       * eeeeeeeeeeeeeeeeeeeeeee)
"""

[<Test>]
let ``split infix operators according to nested structure in AST, 988`` () =
    formatSourceString
        """
let shouldIncludeRelationship relName =
    req.Includes |> List.exists (fun path ->
      path.Length >= currentIncludePath.Length + 1
      && path |> List.take (currentIncludePath.Length + 1) = currentIncludePath @ [relName]
    )
"""
        config
    |> prepend newline
    |> should
        equal
        """
let shouldIncludeRelationship relName =
    req.Includes
    |> List.exists (fun path ->
        path.Length >= currentIncludePath.Length + 1
        && path |> List.take (currentIncludePath.Length + 1) = currentIncludePath @ [ relName ])
"""

[<Test>]
let ``add in keyword when let binding is part of same operator infix expression, 1461`` () =
    formatSourceString
        """
    let isUnseenByHidingAttribute () =
        not (isObjTy g ty) &&
        isAppTy g ty &&
        isObjTy g minfo.ApparentEnclosingType &&
        let tcref = tcrefOfAppTy g ty
        match tcref.TypeReprInfo with
        | _ -> false
"""
        config
    |> prepend newline
    |> should
        equal
        """
let isUnseenByHidingAttribute () =
    not (isObjTy g ty)
    && isAppTy g ty
    && isObjTy g minfo.ApparentEnclosingType
    && let tcref = tcrefOfAppTy g ty in

       match tcref.TypeReprInfo with
       | _ -> false
"""

[<Test>]
let ``add in keyword when let binding is part of single infix expression`` () =
    formatSourceString
        """
    // Check for the [<ProjectionParameter>] attribute on an argument position
    let isCustomOperationProjectionParameter i (nm: Ident) =
        match tryGetArgInfosForCustomOperator nm with
        | None -> false
        | Some argInfosForOverloads ->
            let vs =
                argInfosForOverloads |> List.map (function
                    | None -> false
                    | Some argInfos ->
                        i < argInfos.Length &&
                        let (_, argInfo) = List.item i argInfos
                        HasFSharpAttribute cenv.g cenv.g.attrib_ProjectionParameterAttribute argInfo.Attribs)
            if List.allEqual vs then vs.[0]
            else
                let opDatas = (tryGetDataForCustomOperation nm).Value
                let (opName, _, _, _, _, _, _, _j, _) = opDatas.[0]
                errorR(Error(FSComp.SR.tcCustomOperationInvalid opName, nm.idRange))
                false
"""
        config
    |> prepend newline
    |> should
        equal
        """
// Check for the [<ProjectionParameter>] attribute on an argument position
let isCustomOperationProjectionParameter i (nm: Ident) =
    match tryGetArgInfosForCustomOperator nm with
    | None -> false
    | Some argInfosForOverloads ->
        let vs =
            argInfosForOverloads
            |> List.map (function
                | None -> false
                | Some argInfos ->
                    i < argInfos.Length
                    && let (_, argInfo) = List.item i argInfos in
                       HasFSharpAttribute cenv.g cenv.g.attrib_ProjectionParameterAttribute argInfo.Attribs)

        if List.allEqual vs then
            vs.[0]
        else
            let opDatas = (tryGetDataForCustomOperation nm).Value
            let (opName, _, _, _, _, _, _, _j, _) = opDatas.[0]
            errorR (Error(FSComp.SR.tcCustomOperationInvalid opName, nm.idRange))
            false
"""

[<Test>]
let ``operator with QMARK_QMARK token, 1533`` () =
    formatSourceString
        """
// Minimal code that displays the root of the issue:
let inline (>??) x = x > x
"""
        config
    |> prepend newline
    |> should
        equal
        """
// Minimal code that displays the root of the issue:
let inline (>??) x = x > x
"""

[<Test>]
let ``multiple let bindings in infix expression, 1548`` () =
    formatSourceString
        """
/// Used to hide/filter members from base classes based on signature
let MethInfosEquivByNameAndSig erasureFlag ignoreFinal g amap m minfo minfo2 =
    MethInfosEquivByNameAndPartialSig erasureFlag ignoreFinal g amap m minfo minfo2 &&
    let (CompiledSig(_, retTy, formalMethTypars, _)) = CompiledSigOfMeth g amap m minfo
    let (CompiledSig(_, retTy2, formalMethTypars2, _)) = CompiledSigOfMeth g amap m minfo2
    match retTy, retTy2 with
    | None, None -> true
    | Some retTy, Some retTy2 -> typeAEquivAux erasureFlag g (TypeEquivEnv.FromEquivTypars formalMethTypars formalMethTypars2) retTy retTy2
    | _ -> false
"""
        config
    |> prepend newline
    |> should
        equal
        """
/// Used to hide/filter members from base classes based on signature
let MethInfosEquivByNameAndSig erasureFlag ignoreFinal g amap m minfo minfo2 =
    MethInfosEquivByNameAndPartialSig erasureFlag ignoreFinal g amap m minfo minfo2
    && let (CompiledSig(_, retTy, formalMethTypars, _)) = CompiledSigOfMeth g amap m minfo in

       let (CompiledSig(_, retTy2, formalMethTypars2, _)) =
           CompiledSigOfMeth g amap m minfo2 in

       match retTy, retTy2 with
       | None, None -> true
       | Some retTy, Some retTy2 ->
           typeAEquivAux erasureFlag g (TypeEquivEnv.FromEquivTypars formalMethTypars formalMethTypars2) retTy retTy2
       | _ -> false
"""

[<Test>]
let ``match lambda in infix expression should indent, 1559`` () =
    formatSourceString
        """
let foo () =
    blah
    |> function
        | x -> ()
"""
        config
    |> prepend newline
    |> should
        equal
        """
let foo () =
    blah
    |> function
        | x -> ()
"""

[<Test>]
let ``match lambda in multi pipe infix expression, 614`` () =
    formatSourceString
        """
let expected =
    b
    |> function
       | Some c -> c
       | None -> 0
    |> id
"""
        config
    |> prepend newline
    |> should
        equal
        """
let expected =
    b
    |> function
        | Some c -> c
        | None -> 0
    |> id
"""

[<Test>]
let ``dollar in custom operator, 1598`` () =
    formatSourceString
        """
/// Destructure and apply a tuple to an arbitrary value.
/// E.g. `myFn $ (arg1, arg2)` in JS becomes `myFn(arg1, arg2)`
let ($) (callee: obj) (args: obj): 'a = jsNative
"""
        config
    |> prepend newline
    |> should
        equal
        """
/// Destructure and apply a tuple to an arbitrary value.
/// E.g. `myFn $ (arg1, arg2)` in JS becomes `myFn(arg1, arg2)`
let ($) (callee: obj) (args: obj) : 'a = jsNative
"""

[<Test>]
let ``if/then/else in infix should always be multiline, 1609`` () =
    formatSourceString
        """
module Foo =
    let bar () =

        if not <| RuntimeInformation.IsOSPlatform OSPlatform.Windows then
            raise <| PlatformNotSupportedException ("Blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah")

        lazy (
            let foo = bar
            if ret then
                ""
            else
                ""
            |> log.LogInformation
            ret
        )
"""
        { config with
            MaxLineLength = 100
            SpaceBeforeUppercaseInvocation = true
            SpaceBeforeClassConstructor = true
            SpaceBeforeMember = true
            SpaceBeforeColon = true
            SpaceBeforeSemicolon = true
            MultilineBracketStyle = Aligned
            NewlineBetweenTypeDefinitionAndMembers = true
            AlignFunctionSignatureToIndentation = true
            AlternativeLongMemberDefinitions = true
            MultiLineLambdaClosingNewline = true
            ExperimentalKeepIndentInBranch = true
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
module Foo =
    let bar () =

        if
            not
            <| RuntimeInformation.IsOSPlatform OSPlatform.Windows
        then
            raise
            <| PlatformNotSupportedException (
                "Blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah"
            )

        lazy
            (let foo = bar

             if ret then "" else ""
             |> log.LogInformation

             ret)
"""

[<Test>]
let ``combining two empty list with at`` () =
    formatSourceString
        """
[] @ []
"""
        config
    |> prepend newline
    |> should
        equal
        """
[] @ []
"""

[<Test>]
let ``appending two lists with at, 1719`` () =
    formatSourceString
        """
[ 2.; 4. ] @ [ 2.; 4. ]
"""
        config
    |> prepend newline
    |> should
        equal
        """
[ 2.; 4. ] @ [ 2.; 4. ]
"""

[<Test>]
let ``list concat chain using operators, 1188`` () =
    formatSourceString
        """
[1 .. 86] @ [89 .. 699] @ [901 .. 912] @ [988]
"""
        config
    |> prepend newline
    |> should
        equal
        """
[ 1..86 ] @ [ 89..699 ] @ [ 901..912 ] @ [ 988 ]
"""

[<Test>]
let ``comment above piped match expression, 1711`` () =
    formatSourceString
        """
module Foo =

    let bar =
        baz
        |> (
            // Hi!
            match false with
            | true -> id
            | false -> id
        )
"""
        config
    |> prepend newline
    |> should
        equal
        """
module Foo =

    let bar =
        baz
        |> (
            // Hi!
            match false with
            | true -> id
            | false -> id)
"""

[<Test>]
let ``qualified name to active pattern, 1937`` () =
    formatSourceString
        """
StringPosition.(|TrimStart|)
(|TrimStart|)
StringPosition.(|TrimStart|_|)
(|TrimStart|_|)
StringPosition.(|A|B|)
(|A|B|)

let f (|A|B|) = (|A|B|)
"""
        config
    |> prepend newline
    |> should
        equal
        """
StringPosition.(|TrimStart|)
(|TrimStart|)
StringPosition.(|TrimStart|_|)
(|TrimStart|_|)
StringPosition.(|A|B|)
(|A|B|)

let f (|A|B|) = (|A|B|)
"""

[<Test>]
let ``custom range operator definition, 1842`` () =
    formatSourceString
        """
[<AutoOpen>] //making auto open allows us not to have to fully qualify module properties
module DecompilationTests
open Xunit
open Swensen.Unquote

module TopLevelOpIsolation3 =
    let (..) x y z = Seq.singleton (x + y + z)
    [<Fact>]
    let ``issue 91: op_Range first class syntax for seq return type but arg mismatch`` () =
        <@ (..) 1 2 3 @> |> decompile =! "TopLevelOpIsolation3.(..) 1 2 3"

    let (.. ..) x y z h = Seq.singleton (x + y + z + h)
    [<Fact>]
    let ``issue 91: op_RangeStep first class syntax for seq return type but arg mismatch`` () =
        <@ (.. ..) 1 2 3 4 @> |> decompile =! "TopLevelOpIsolation3.(.. ..) 1 2 3 4"
"""
        { config with
            MaxInfixOperatorExpression = 50 }
    |> prepend newline
    |> should
        equal
        """
[<AutoOpen>] //making auto open allows us not to have to fully qualify module properties
module DecompilationTests

open Xunit
open Swensen.Unquote

module TopLevelOpIsolation3 =
    let (..) x y z = Seq.singleton (x + y + z)

    [<Fact>]
    let ``issue 91: op_Range first class syntax for seq return type but arg mismatch`` () =
        <@ (..) 1 2 3 @> |> decompile
        =! "TopLevelOpIsolation3.(..) 1 2 3"

    let (.. ..) x y z h = Seq.singleton (x + y + z + h)

    [<Fact>]
    let ``issue 91: op_RangeStep first class syntax for seq return type but arg mismatch`` () =
        <@ (.. ..) 1 2 3 4 @> |> decompile
        =! "TopLevelOpIsolation3.(.. ..) 1 2 3 4"
"""

[<Test>]
let ``add space around binary operators in units of measure, 2207`` () =
    formatSourceString
        """
type Test =
    { WorkHoursPerWeek: uint<hr*(staff weeks)> }
    static member create =
     { WorkHoursPerWeek = 40u<hr*(staff weeks)> }
"""
        { config with
            NewlineBetweenTypeDefinitionAndMembers = false }
    |> prepend newline
    |> should
        equal
        """
type Test =
    { WorkHoursPerWeek: uint<hr * (staff weeks)> }
    static member create = { WorkHoursPerWeek = 40u<hr * (staff weeks)> }
"""

[<Test>]
let ``custom operator starting with *, 2434`` () =
    formatSourceString
        """
let ( */) = (+)
"""
        config
    |> prepend newline
    |> should
        equal
        """
let ( */ ) = (+)
"""

[<Test>]
let ``piped lambda on a single line`` () =
    formatSourceString
        """
let a : (unit -> int) list =
    fun () -> failwith "" : int
    |> List.singleton
    |> id
"""
        config
    |> prepend newline
    |> should
        equal
        """
let a: (unit -> int) list = (fun () -> failwith "": int) |> List.singleton |> id
"""

[<Test>]
let ``piped tuple on a single line`` () =
    formatSourceString
        """
fun i -> sprintf "%i" i, fun () -> i
|> List.init foo
|> Map.ofList
"""
        config
    |> prepend newline
    |> should
        equal
        """
(fun i -> sprintf "%i" i, fun () -> i) |> List.init foo |> Map.ofList
"""

[<Test>]
let ``lambda piped into non newlineInfixApp`` () =
    formatSourceString
        """
fun sum count -> sum / float count
<*| sum xs
<*| count
"""
        config
    |> prepend newline
    |> should
        equal
        """
(fun sum count -> sum / float count) <*| sum xs <*| count
"""

[<Test>]
let ``single star operator, 2452`` () =
    formatSourceString
        """
let (*) x y = x + y
"""
        config
    |> prepend newline
    |> should
        equal
        """
let (*) x y = x + y
"""

[<Test>]
let ``trivia after match lambda in infix operation, 2454`` () =
    formatSourceString
        """
id
>> function
    | -1 -> -1 // first comment
    | 0 -> 0 // second comment
    | _ -> 1 // third comment
"""
        config
    |> prepend newline
    |> should
        equal
        """
id
>> function
    | -1 -> -1 // first comment
    | 0 -> 0 // second comment
    | _ -> 1 // third comment
"""

[<Test>]
let ``multiple list concat operators, 2472`` () =
    formatSourceString
        """
let allDecls = inheritsL @ iimplsLs @ ctorLs @ instanceValLs @ methLs @ ilFieldsL @ propLs @ eventLs @ staticValLs @ nestedTypeLs
"""
        { config with
            MaxInfixOperatorExpression = 0 }
    |> prepend newline
    |> should
        equal
        """
let allDecls =
    inheritsL
    @ iimplsLs
    @ ctorLs
    @ instanceValLs
    @ methLs
    @ ilFieldsL
    @ propLs
    @ eventLs
    @ staticValLs
    @ nestedTypeLs
"""

[<Test>]
let ``multiple cons operators`` () =
    formatSourceString
        """
let allDecls = inheritsL :: iimplsLs :: ctorLs :: foo ::blah
"""
        { config with
            MaxInfixOperatorExpression = 0 }
    |> prepend newline
    |> should
        equal
        """
let allDecls =
    inheritsL
    :: iimplsLs
    :: ctorLs
    :: foo
    :: blah
"""

[<Test>]
let ``right placed operators`` () =
    formatSourceString
        """
  // right: @, ::, **, ^, := or starts with combinations
  let allDecls = inheritsL @ iimplsLs @ ctorLs 
  let allDecls = inheritsL :: iimplsLs :: ctorLs
  let allDecls = inheritsL ** iimplsLs ** ctorLs
  let allDecls = inheritsL ^ iimplsLs ^ ctorLs
  let allDecls = inheritsL ^^ iimplsLs ^^ ctorLs
  let allDecls = inheritsL := iimplsLs := ctorLs
  let allDecls = inheritsL @- iimplsLs @- ctorLs 
  let allDecls = inheritsL @+ iimplsLs @+ ctorLs 
"""
        { config with
            MaxInfixOperatorExpression = 0 }
    |> prepend newline
    |> should
        equal
        """
// right: @, ::, **, ^, := or starts with combinations
let allDecls =
    inheritsL
    @ iimplsLs
    @ ctorLs

let allDecls =
    inheritsL
    :: iimplsLs
    :: ctorLs

let allDecls =
    inheritsL
    ** iimplsLs
    ** ctorLs

let allDecls =
    inheritsL
    ^ iimplsLs
    ^ ctorLs

let allDecls =
    inheritsL
    ^^ iimplsLs
    ^^ ctorLs

let allDecls =
    inheritsL
    := iimplsLs
    := ctorLs

let allDecls =
    inheritsL
    @- iimplsLs
    @- ctorLs

let allDecls =
    inheritsL
    @+ iimplsLs
    @+ ctorLs
"""
