module XDiff.XmlDiffUtils

    open System
    open System.Xml.Linq
    open Types

    let private getNodeName node = 
        match node with
        |Node(name,_,_) -> name
        |Leaf(name,_,_) -> name
        |Empty -> failwith "empty node, no name"

    let private addNode parent child =
        match parent with
        |Node(name,attributes,childs) ->
            Node(name,attributes, child::childs)
        |Leaf(_) ->
            failwith "parent is a leaf, cannot add child to a leaf"
        |Empty -> child

    let private GetAttributes (xel : XElement ) =
        xel.Attributes() 
        |> Seq.map (fun a -> a.Name.LocalName, a.Value)
        |> Map.ofSeq

    let rec private xelToDomain (x : XElement) (accumulator : XmlNode) : XmlNode  =
        let name = x.Name.LocalName
        match x.HasElements with
        |true -> 
            let childs = x.Elements() |> Seq.map (fun e -> xelToDomain e Empty) |> Seq.toList
            addNode accumulator (Node(name, GetAttributes x, childs))
        |false -> 
            addNode accumulator (Leaf(name, GetAttributes x, x.Value))


    let private attributesDiff (attrA : Map<string,string>) (attrB: Map<string,string>) : AttributeDiff list =
        let diffAB = ( Map.fold(fun a k v ->
            let corresponding = Map.tryFind k attrB
            match corresponding with
            |Some(v2) -> 
                if v2 <> v then 
                    AttributeDiff.Value({ Name = k; A = Some(v2); B = Some(v)}) :: a
                else
                    a
            |None -> 
                AttributeDiff.Missing({ Name = k; A = Some(v); B = None}) :: a
            ) [] attrA ) |> Set.ofList
        in
        let diffBA = ( Map.fold(fun a k v ->
            let corresponding = Map.tryFind k attrA
            match corresponding with
            |Some(v2) -> 
                if v2 <> v then 
                    AttributeDiff.Value({ Name = k; A = Some(v2); B = Some(v)}) :: a
                else
                    a
            |None -> 
                AttributeDiff.Missing({ Name = k; A = Some(v); B = None}) :: a
            ) [] attrB ) |> Set.ofList
        let resulting = diffAB |> Set.union diffBA |> Set.toList
        resulting

    let findChildNames childs =
        childs |> Seq.map getNodeName |> Set.ofSeq

    let findDifferentChilds childsA childsB =
        let childNamesA = findChildNames childsA
        let childNamesB = findChildNames childsB
        let exA = ((childNamesA, childNamesB) ||> Set.difference) 
        let exB = ((childNamesB, childNamesA) ||> Set.difference)
        in (exA, exB) 

    let attributeDiffs nameA attributesA attributesB =
        let ads = attributesDiff attributesA attributesB
        match ads.IsEmpty with
        |false -> [Attribute(nameA, ads)]
        |true -> []

    let childNodeDifference nameA childsA nameB childsB =
        let extraA, extraB = findDifferentChilds childsA childsB
        let missingsA = extraA |> Set.map (fun a -> NodeDiff.Missing({ Name = nameB; A = Some(a); B = None })) |> Set.toList
        let missingsB = extraB |> Set.map (fun b -> NodeDiff.Missing({ Name = nameA; A = None; B = Some(b) })) |> Set.toList
        let childsANew = childsA |> Seq.filter(fun c -> not (extraA |> Set.contains (getNodeName c)) ) |> Seq.toList
        let childsBNew = childsB |> Seq.filter(fun c -> not (extraB |> Set.contains (getNodeName c)) ) |> Seq.toList
        (missingsA, childsANew), (missingsB, childsBNew)

    let rec private MakeDiff (ab : XmlNode * XmlNode) (results: NodeDiff list) : NodeDiff list =
        let a, b = ab in 
        match ab with
        |(Node(nameA,attributesA,childsA), Node(nameB,attributesB,childsB)) ->
            let attrDiff = attributeDiffs nameA attributesA attributesB
            let  (missingsA, childsANew), (missingsB, childsBNew) = childNodeDifference nameA childsA nameB childsB
            let newRes = attrDiff @ results @ missingsA @ missingsB
            (childsANew,childsBNew) ||> Seq.zip |> Seq.fold(fun a ab -> MakeDiff ab a) newRes
        |(Leaf(nameA,attributesA,valueA),Leaf(nameB,attributesB,valueB)) ->
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
            |Leaf(name,_,_),Empty -> Missing({ Name = name; A = Some(name); B = None }) :: results
            |Empty,Node(name,_,_) 
            |Empty,Leaf(name,_,_) -> Missing({ Name = name; A = None; B = Some(name) }) :: results
            |_ -> failwith "empty node"

    let private filterDiffs diffs excludedNodes =
        let d = { 
            diffs with Diffs = diffs.Diffs 
                        |> List.filter (fun x -> 
                            match x with
                            |Missing(diff) -> not (excludedNodes |> Seq.contains diff.Name)
                            |Value(z) -> not (excludedNodes |> Seq.contains z.Name)
                            |_ -> true
                        ) 
            } in
        match d.Diffs with
        |[] -> None
        |_ -> Some(d)

    let private fromFileNameToXmlNode fileName =
        xelToDomain (XDocument.Parse(IO.File.ReadAllText(fileName)).Root) XmlNode.Empty

    let ComputeDiff (a : string, b : string, excludedNodes : string seq) : string seq =
        let rootA = fromFileNameToXmlNode a
        let rootB = fromFileNameToXmlNode b 
        let diffs = { A = a; B = b; Diffs = MakeDiff (rootA, rootB) [] }
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
