﻿module internal rec Fantomas.Core.CodePrinter2

open Fantomas.Core.Context
open Fantomas.Core.SyntaxOak

let (|UppercaseType|LowercaseType|) (t: Type) : Choice<unit, unit> = UppercaseType()

let genTrivia (trivia: TriviaNode) (ctx: Context) =
    let currentLastLine = ctx.WriterModel.Lines |> List.tryHead

    // Some items like #if or Newline should be printed on a newline
    // It is hard to always get this right in CodePrinter, so we detect it based on the current code.
    let addNewline =
        currentLastLine
        |> Option.map (fun line -> line.Trim().Length > 0)
        |> Option.defaultValue false

    let addSpace =
        currentLastLine
        |> Option.bind (fun line -> Seq.tryLast line |> Option.map (fun lastChar -> lastChar <> ' '))
        |> Option.defaultValue false

    let gen =
        match trivia.Content with
        | LineCommentAfterSourceCode s ->
            let comment = sprintf "%s%s" (if addSpace then " " else String.empty) s
            writerEvent (WriteBeforeNewline comment)
        | CommentOnSingleLine comment -> (ifElse addNewline sepNlnForTrivia sepNone) +> !-comment +> sepNlnForTrivia
        | Newline -> (ifElse addNewline (sepNlnForTrivia +> sepNlnForTrivia) sepNlnForTrivia)

    gen ctx

let genNode<'n when 'n :> Node> (n: 'n) (f: Context -> Context) =
    col sepNone n.ContentBefore genTrivia
    +> f
    +> col sepNone n.ContentAfter genTrivia

let genSingleTextNode (node: SingleTextNode) = !-node.Text |> genNode node

let genIdentListNode (iln: IdentListNode) =
    col sepNone iln.Content (function
        | IdentifierOrDot.Ident ident -> genSingleTextNode ident
        | IdentifierOrDot.KnownDot _
        | IdentifierOrDot.UnknownDot _ -> sepDot)
    |> genNode iln

let genParsedHashDirective (phd: ParsedHashDirectiveNode) =
    !- "#" +> !-phd.Ident +> sepSpace +> col sepSpace phd.Args genSingleTextNode
    |> genNode phd

// genNode will should be called in the caller function.
let genConstant (c: Constant) =
    match c with
    | Constant.FromText n -> genSingleTextNode n
    | Constant.Unit n -> genSingleTextNode n.OpeningParen +> genSingleTextNode n.ClosingParen

let genExpr (e: Expr) =
    match e with
    | Expr.Lazy node ->
        let genInfixExpr (ctx: Context) =
            isShortExpression
                ctx.Config.MaxInfixOperatorExpression
                // if this fits on the rest of line right after the lazy keyword, it should be wrapped in parenthesis.
                (sepOpenT +> genExpr e +> sepCloseT)
                // if it is multiline there is no need for parenthesis, because of the indentation
                (indent +> sepNln +> genExpr e +> unindent)
                ctx

        let genNonInfixExpr = autoIndentAndNlnIfExpressionExceedsPageWidth (genExpr e)

        genSingleTextNode node.LazyWord
        +> sepSpace
        +> ifElse node.ExprIsInfix genInfixExpr genNonInfixExpr
    | Expr.Single node ->
        genSingleTextNode node.Leading
        +> sepSpace
        +> ifElse
            node.SupportsStroustrup
            (autoIndentAndNlnIfExpressionExceedsPageWidthUnlessStroustrup genExpr node.Expr)
            (autoIndentAndNlnIfExpressionExceedsPageWidth (genExpr node.Expr))
    | Expr.Constant node -> genConstant node
    | Expr.Null node -> genSingleTextNode node
    | Expr.Quote node ->
        genSingleTextNode node.OpenToken
        +> sepSpace
        +> expressionFitsOnRestOfLine (genExpr node.Expr) (indent +> sepNln +> genExpr node.Expr +> unindent +> sepNln)
        +> sepSpace
        +> genSingleTextNode node.CloseToken
    | Expr.Typed node ->
        let short =
            genExpr node.Expr
            +> sepSpace
            +> !-node.Operator
            +> sepSpace
            +> genType node.Type

        let long =
            genExpr node.Expr +> sepNln +> !-node.Operator +> sepSpace +> genType node.Type

        match node.Expr with
        | Expr.Lambda _ -> long
        | _ -> expressionFitsOnRestOfLine short long
    | Expr.NewParen node ->
        let sepSpaceBeforeArgs (ctx: Context) =
            match node.Type with
            | UppercaseType -> onlyIf ctx.Config.SpaceBeforeUppercaseInvocation sepSpace ctx
            | LowercaseType -> onlyIf ctx.Config.SpaceBeforeLowercaseInvocation sepSpace ctx

        let short =
            genSingleTextNode node.NewKeyword
            +> sepSpace
            +> genType node.Type
            +> sepSpaceBeforeArgs
            +> genExpr node.Arguments

        let long =
            genSingleTextNode node.NewKeyword
            +> sepSpace
            +> genType node.Type
            +> sepSpaceBeforeArgs
            +> genMultilineFunctionApplicationArguments node.Arguments

        expressionFitsOnRestOfLine short long
    | Expr.New _ -> failwith "Not Implemented"
    | Expr.Tuple _ -> failwith "Not Implemented"
    | Expr.StructTuple _ -> failwith "Not Implemented"
    | Expr.ArrayOrList _ -> failwith "Not Implemented"
    | Expr.Record _ -> failwith "Not Implemented"
    | Expr.AnonRecord _ -> failwith "Not Implemented"
    | Expr.ObjExpr _ -> failwith "Not Implemented"
    | Expr.While _ -> failwith "Not Implemented"
    | Expr.For _ -> failwith "Not Implemented"
    | Expr.ForEach _ -> failwith "Not Implemented"
    | Expr.NamedComputation _ -> failwith "Not Implemented"
    | Expr.Computation _ -> failwith "Not Implemented"
    | Expr.CompExprBody _ -> failwith "Not Implemented"
    | Expr.JoinIn _ -> failwith "Not Implemented"
    | Expr.ParenLambda _ -> failwith "Not Implemented"
    | Expr.Lambda _ -> failwith "Not Implemented"
    | Expr.MatchLambda _ -> failwith "Not Implemented"
    | Expr.Match _ -> failwith "Not Implemented"
    | Expr.TraitCall _ -> failwith "Not Implemented"
    | Expr.ParenILEmbedded _ -> failwith "Not Implemented"
    | Expr.ParenFunctionNameWithStar _ -> failwith "Not Implemented"
    | Expr.Paren node ->
        genSingleTextNode node.OpeningParen
        +> genExpr node.Expr
        +> genSingleTextNode node.ClosingParen
    | Expr.Dynamic _ -> failwith "Not Implemented"
    | Expr.PrefixApp _ -> failwith "Not Implemented"
    | Expr.NewlineInfixAppAlwaysMultiline _ -> failwith "Not Implemented"
    | Expr.NewlineInfixApps _ -> failwith "Not Implemented"
    | Expr.SameInfixApps _ -> failwith "Not Implemented"
    | Expr.InfixApp _ -> failwith "nope"
    | Expr.TernaryApp _ -> failwith "Not Implemented"
    | Expr.IndexWithoutDot _ -> failwith "Not Implemented"
    | Expr.AppDotGetTypeApp _ -> failwith "Not Implemented"
    | Expr.DotGetAppDotGetAppParenLambda _ -> failwith "Not Implemented"
    | Expr.DotGetAppParen _ -> failwith "Not Implemented"
    | Expr.DotGetAppWithParenLambda _ -> failwith "Not Implemented"
    | Expr.DotGetApp _ -> failwith "Not Implemented"
    | Expr.AppLongIdentAndSingleParenArg _ -> failwith "Not Implemented"
    | Expr.AppSingleParenArg _ -> failwith "Not Implemented"
    | Expr.DotGetAppWithLambda _ -> failwith "Not Implemented"
    | Expr.AppWithLambda _ -> failwith "Not Implemented"
    | Expr.NestedIndexWithoutDot _ -> failwith "Not Implemented"
    | Expr.EndsWithDualListApp _ -> failwith "Not Implemented"
    | Expr.EndsWithSingleListApp _ -> failwith "Not Implemented"
    | Expr.App _ -> failwith "Not Implemented"
    | Expr.TypeApp _ -> failwith "Not Implemented"
    | Expr.LetOrUses _ -> failwith "Not Implemented"
    | Expr.TryWithSingleClause _ -> failwith "Not Implemented"
    | Expr.TryWith _ -> failwith "Not Implemented"
    | Expr.TryFinally _ -> failwith "Not Implemented"
    | Expr.Sequentials _ -> failwith "Not Implemented"
    | Expr.IfThen _ -> failwith "Not Implemented"
    | Expr.IfThenElse _ -> failwith "Not Implemented"
    | Expr.IfThenElif _ -> failwith "Not Implemented"
    | Expr.Ident node -> genSingleTextNode node
    | Expr.OptVar _ -> failwith "Not Implemented"
    | Expr.LongIdentSet _ -> failwith "Not Implemented"
    | Expr.DotIndexedGet _ -> failwith "Not Implemented"
    | Expr.DotIndexedSet _ -> failwith "Not Implemented"
    | Expr.NamedIndexedPropertySet _ -> failwith "Not Implemented"
    | Expr.DotNamedIndexedPropertySet _ -> failwith "Not Implemented"
    | Expr.DotGet _ -> failwith "Not Implemented"
    | Expr.DotSet _ -> failwith "Not Implemented"
    | Expr.Set _ -> failwith "Not Implemented"
    | Expr.LibraryOnlyStaticOptimization _ -> failwith "Not Implemented"
    | Expr.InterpolatedStringExpr _ -> failwith "Not Implemented"
    | Expr.IndexRangeWildcard _ -> failwith "Not Implemented"
    | Expr.IndexRange _ -> failwith "Not Implemented"
    | Expr.IndexFromEnd _ -> failwith "Not Implemented"
    | Expr.Typar _ -> failwith "Not Implemented"
    |> genNode (Expr.Node e)

let genMultilineFunctionApplicationArguments (argExpr: Expr) = !- "todo!"
// let argsInsideParenthesis lpr rpr pr f =
//     sepOpenTFor lpr +> indentSepNlnUnindent f +> sepNln +> sepCloseTFor rpr
//     |> genTriviaFor SynExpr_Paren pr
//
// let genExpr e =
//     match e with
//     | InfixApp (equal, operatorSli, e1, e2, range) when (equal = "=") ->
//         genNamedArgumentExpr operatorSli e1 e2 range
//     | _ -> genExpr e
//
// match argExpr with
// | Paren (lpr, Lambda (pats, arrowRange, body, range), rpr, _pr) ->
//     fun ctx ->
//         if ctx.Config.MultiLineLambdaClosingNewline then
//             let genPats =
//                 let shortPats = col sepSpace pats genPat
//                 let longPats = atCurrentColumn (sepNln +> col sepNln pats genPat)
//                 expressionFitsOnRestOfLine shortPats longPats
//
//             (sepOpenTFor lpr
//              +> (!- "fun " +> genPats +> genLambdaArrowWithTrivia genExpr body arrowRange
//                  |> genTriviaFor SynExpr_Lambda range)
//              +> sepNln
//              +> sepCloseTFor rpr)
//                 ctx
//         else
//             genExpr argExpr ctx
// | Paren (lpr, Tuple (args, tupleRange), rpr, pr) ->
//     genTupleMultiline args
//     |> genTriviaFor SynExpr_Tuple tupleRange
//     |> argsInsideParenthesis lpr rpr pr
// | Paren (lpr, singleExpr, rpr, pr) -> genExpr singleExpr |> argsInsideParenthesis lpr rpr pr
// | _ -> genExpr argExpr

let genPat (p: Pattern) =
    match p with
    | Pattern.OptionalVal _ -> failwith "Not Implemented"
    | Pattern.Attrib _ -> failwith "Not Implemented"
    | Pattern.Or _ -> failwith "Not Implemented"
    | Pattern.Ands _ -> failwith "Not Implemented"
    | Pattern.Null node
    | Pattern.Wild node -> genSingleTextNode node
    | Pattern.Typed _ -> failwith "Not Implemented"
    | Pattern.Named node -> genSingleTextNode node.Name
    | Pattern.As _ -> failwith "Not Implemented"
    | Pattern.ListCons _ -> failwith "Not Implemented"
    | Pattern.NamePatPairs _ -> failwith "Not Implemented"
    | Pattern.LongIdentParen _ -> failwith "Not Implemented"
    | Pattern.LongIdent _ -> failwith "Not Implemented"
    | Pattern.Unit _ -> failwith "Not Implemented"
    | Pattern.Paren _ -> failwith "Not Implemented"
    | Pattern.Tuple _ -> failwith "Not Implemented"
    | Pattern.StructTuple _ -> failwith "Not Implemented"
    | Pattern.ArrayOrList _ -> failwith "Not Implemented"
    | Pattern.Record _ -> failwith "Not Implemented"
    | Pattern.Const _ -> failwith "Not Implemented"
    | Pattern.IsInst _ -> failwith "Not Implemented"
    | Pattern.QuoteExpr _ -> failwith "Not Implemented"
    |> genNode (Pattern.Node p)

let genBinding (b: BindingNode) =
    genSingleTextNode b.LeadingKeyword
    +> sepSpace
    +> (match b.FunctionName with
        | Choice1Of2 n -> genSingleTextNode n
        | Choice2Of2 pat -> genPat pat)
    +> sepSpace
    +> col sepSpace b.Parameters genPat
    +> sepSpace
    +> genSingleTextNode b.Equals
    +> sepSpace
    +> genExpr b.Expr
    |> genNode b

let genOpenList (openList: OpenListNode) =
    col sepNln openList.Opens (function
        | Open.ModuleOrNamespace node -> !- "open " +> genIdentListNode node.Name |> genNode node
        | Open.Target node -> !- "open type " +> genType node.Target)

let genType (t: Type) =
    match t with
    | Type.Funs _ -> failwith "Not Implemented"
    | Type.Tuple _ -> failwith "Not Implemented"
    | Type.HashConstraint _ -> failwith "Not Implemented"
    | Type.MeasurePower _ -> failwith "Not Implemented"
    | Type.MeasureDivide _ -> failwith "Not Implemented"
    | Type.StaticConstant _ -> failwith "Not Implemented"
    | Type.StaticConstantExpr _ -> failwith "Not Implemented"
    | Type.StaticConstantNamed _ -> failwith "Not Implemented"
    | Type.Array _ -> failwith "Not Implemented"
    | Type.Anon _ -> failwith "Not Implemented"
    | Type.Var _ -> failwith "Not Implemented"
    | Type.App _ -> failwith "Not Implemented"
    | Type.LongIdentApp _ -> failwith "Not Implemented"
    | Type.StructTuple _ -> failwith "Not Implemented"
    | Type.WithGlobalConstraints _ -> failwith "Not Implemented"
    | Type.LongIdent idn -> genIdentListNode idn
    | Type.AnonRecord _ -> failwith "Not Implemented"
    | Type.Paren _ -> failwith "Not Implemented"
    | Type.SignatureParameter _ -> failwith "Not Implemented"
    | Type.Or _ -> failwith "Not Implemented"

let genTypeDefn (td: TypeDefn) =
    let header =
        let node = (TypeDefn.TypeDefnNode td).TypeName

        genSingleTextNode node.LeadingKeyword
        +> sepSpace
        +> genIdentListNode node.Identifier
        +> sepSpace
        +> optSingle genSingleTextNode node.EqualsToken

    let body =
        match td with
        | TypeDefn.Enum _ -> failwith "Not Implemented"
        | TypeDefn.Union _ -> failwith "Not Implemented"
        | TypeDefn.Record _ -> failwith "Not Implemented"
        | TypeDefn.None _ -> failwith "Not Implemented"
        | TypeDefn.Abbrev node -> genType node.Type
        | TypeDefn.Exception _ -> failwith "Not Implemented"
        | TypeDefn.ExplicitClassOrInterfaceOrStruct _ -> failwith "Not Implemented"
        | TypeDefn.Augmentation _ -> failwith "Not Implemented"
        | TypeDefn.Fun _ -> failwith "Not Implemented"
        | TypeDefn.Delegate _ -> failwith "Not Implemented"
        | TypeDefn.Unspecified _ -> failwith "Not Implemented"
        | TypeDefn.RegularType _ -> failwith "Not Implemented"

    leadingExpressionIsMultiline header (fun isMultiline ->
        ifElse isMultiline (indentSepNlnUnindent body) (sepSpace +> body))
    |> genNode (TypeDefn.Node td)

let genModuleDecl (md: ModuleDecl) =
    match md with
    | ModuleDecl.OpenList ol -> genOpenList ol
    | ModuleDecl.HashDirectiveList _ -> failwith "Not Implemented"
    | ModuleDecl.AttributesList _ -> failwith "Not Implemented"
    | ModuleDecl.DeclExpr e -> genExpr e
    | ModuleDecl.ExternBinding _ -> failwith "Not Implemented"
    | ModuleDecl.TopLevelBinding b -> genBinding b
    | ModuleDecl.ModuleAbbrev _ -> failwith "Not Implemented"
    | ModuleDecl.NestedModule _ -> failwith "Not Implemented"
    | ModuleDecl.TypeDefn td -> genTypeDefn td

let sepNlnUnlessContentBefore (node: Node) =
    if Seq.isEmpty node.ContentBefore then sepNln else sepNone

let colWithNlnWhenMappedNodeIsMultiline<'n> (mapNode: 'n -> Node) (f: 'n -> Context -> Context) (nodes: 'n list) =
    nodes
    |> List.map (fun n -> ColMultilineItem(f n, (mapNode >> sepNlnUnlessContentBefore) n))
    |> colWithNlnWhenItemIsMultiline

let colWithNlnWhenNodeIsMultiline<'n when 'n :> Node> (f: 'n -> Context -> Context) (nodes: 'n list) =
    colWithNlnWhenMappedNodeIsMultiline<'n> (fun n -> n :> Node) f nodes

let genModule (m: ModuleOrNamespaceNode) =
    onlyIf
        m.IsNamed
        (optSingle
            (fun (n: SingleTextNode) -> genSingleTextNode n +> sepSpace +> genIdentListNode m.Name)
            m.LeadingKeyword
         +> onlyIf (not m.Declarations.IsEmpty) (sepNln +> sepNln))
    +> colWithNlnWhenMappedNodeIsMultiline ModuleDecl.Node genModuleDecl m.Declarations
    |> genNode m

let genFile (oak: Oak) =
    col sepNln oak.ParsedHashDirectives genParsedHashDirective
    +> (if oak.ParsedHashDirectives.IsEmpty then sepNone else sepNln)
    +> col sepNln oak.ModulesOrNamespaces genModule
    +> (fun ctx -> onlyIf ctx.Config.InsertFinalNewline sepNln ctx)
    |> genNode oak