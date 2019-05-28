module XmlDifference.Difference

    open System
    open System.Xml.Linq
    open Types
    open Attribute
    open XmlNode

    let internal childNodeDifference nameA childsA nameB childsB =
        let extraA, extraB = findDifferentChilds childsA childsB
        let missingsA = 
            extraA 
            |> Set.map (fun a -> NodeDiff.Missing({ Name = nameB; A = Some(a); B = None })) 
            |> Set.toList
        let missingsB = 
            extraB 
            |> Set.map (fun b -> NodeDiff.Missing({ Name = nameA; A = None; B = Some(b) })) 
            |> Set.toList
        let childsANew = 
            childsA 
            |> Seq.filter(fun c -> not (extraA |> Set.contains (c.Name())) ) 
            |> Seq.toList
        let childsBNew = 
            childsB 
            |> Seq.filter(fun c -> not (extraB |> Set.contains (c.Name())) ) 
            |> Seq.toList
        (missingsA, childsANew), (missingsB, childsBNew)

    //this looks crooked..
    let rec private makeDiff (ab : XmlNode * XmlNode) (results: NodeDiff list) : NodeDiff list =
        let a, b = ab in 
        match ab with
        |(Node(nameA,attributesA,childsA), Node(nameB,attributesB,childsB)) ->
            let attrDiff = attributeDiffs nameA attributesA attributesB
            let  (missingsA, childsANew), (missingsB, childsBNew) = 
                childNodeDifference nameA childsA nameB childsB
            let newRes = attrDiff @ results @ missingsA @ missingsB
            (childsANew,childsBNew) 
            ||> Seq.zip 
            |> Seq.fold(fun a ab -> makeDiff ab a) newRes
        |(Leaf(nameA,attributesA,valueA),Leaf(_,attributesB,valueB)) ->
            let attrDiff = attributeDiffs nameA attributesA attributesB
            let leafDiff = 
                match valueA <> valueB with
                |true -> [NodeDiff.Value({ Name = nameA; A = Some(valueA); B = Some(valueB)})]
                |false -> []   
            attrDiff @ leafDiff @ results
        |(Empty,Empty) -> results
        |_ -> 
            match (a,b) with
            |Node(name,_,_),Empty 
            |Leaf(name,_,_),Empty -> 
                Missing({ Name = name; A = Some(name); B = None }) :: results
            |Empty,Node(name,_,_) 
            |Empty,Leaf(name,_,_) -> 
                Missing({ Name = name; A = None; B = Some(name) }) :: results
            |_ -> failwith "empty node"

    let private nodeDiffFilter x excludedNodes =
        match x with
        |Missing(diff) -> not (excludedNodes |> Seq.contains diff.Name)
        |Value(z) -> not (excludedNodes |> Seq.contains z.Name)
        |_ -> true

    let private filterDiffs diffs excludedNodes =
        let d = 
            { diffs with Diffs = 
                diffs.Diffs |> List.filter (fun x -> nodeDiffFilter x excludedNodes) 
            } in
        match d.Diffs with
        |[] -> None
        |_ -> Some(d)

    let internal Diff (diffRequest: DiffRequest, excludedNodes : string seq) : string seq =
        let rootA = xelToDomain diffRequest.A.Content XmlNode.Empty
        let rootB = xelToDomain diffRequest.B.Content XmlNode.Empty
        let diffs = { 
            A = diffRequest.A.Name; 
            B = diffRequest.B.Name; 
            Diffs = makeDiff (rootA, rootB) [] 
        }
        let filtered = filterDiffs diffs excludedNodes
        match filtered with
        |Some(d) ->
            seq { 
                for r in d.Diffs do
                    let res = 
                        match r with
                        |Attribute(a, al) -> sprintf "attribute: %A" (a,al)
                        |Value(v) -> sprintf "value: %A" v
                        |Missing(diff) -> sprintf "missing: %A" diff
                    yield res
            } 
        |None -> [] |> Seq.ofList

    let internal FilesDiff (a : string, b : string, excludedNodes : string seq, documentLoader : string -> XElement) : string seq =
        let diffRequest = {
            A = { Name = a; Content = documentLoader a };
            B = { Name = b; Content = documentLoader b }
        }
        Diff (diffRequest,excludedNodes)


