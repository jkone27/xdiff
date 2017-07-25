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
        Map.fold(fun a k v ->
            let corresponding = Map.tryFind k attrB
            match corresponding with
            |Some(v2) -> 
                if v2 <> v then 
                    AttributeDiff.Value({ Name = k; A = v2; B = v}) :: a
                else
                    a
            |None -> 
                AttributeDiff.Missing(k) :: a
            ) [] attrA

    let rec private MakeDiff (ab : XmlNode * XmlNode) (results: NodeDiff list) : NodeDiff list =
        let a, b = ab in 
        match (a, b) with
        |(Node(nameA,attributesA,childsA), Node(nameB,attributesB,childsB)) ->
            let nameDiff = 
                match nameA <> nameB with 
                |true -> [Name({ Name = nameA; A = nameA; B = nameB})]
                |false -> []
            let ads = attributesDiff attributesA attributesB
            let attrDiff = 
                match ads.IsEmpty with
                |false -> [Attribute(nameA, ads)]
                |true -> []
            let newRes = nameDiff @ attrDiff @ results
            (childsA, childsB) ||> Seq.zip |> Seq.fold(fun a ab -> MakeDiff ab a) newRes
        |(Leaf(nameA,attributesA,valueA),Leaf(nameB,attributesB,valueB)) ->
            let nameDiff = 
                match nameA <> nameB with 
                |true -> [Name({ Name = nameA; A = nameA; B = nameB})]
                |false -> []
            let ads = attributesDiff attributesA attributesB
            let attrDiff = 
                match ads.IsEmpty with
                |false -> [Attribute(nameA, ads)]
                |true -> []
            let leafDiff = 
                match valueA <> valueB with
                |true -> [NodeDiff.Value({ Name = nameA; A = valueA; B = valueB})]
                |false -> []   
            nameDiff @ attrDiff @ leafDiff @ results
        |(Empty,Empty) -> results
        |_ -> 
            match (a,b) with
            |Node(name,_,_),_ 
            |Leaf(name,_,_),_ 
            |_,Node(name,_,_) 
            |_,Leaf(name,_,_) -> Missing(name) :: results
            |_ -> failwith "empty node"

    let private filterDiffs diffs excludedNodes =
        let d = { 
            diffs with Diffs = diffs.Diffs 
                        |> List.filter (fun x -> 
                            match x with
                            |Name(x) 
                            |Value(x) -> not (excludedNodes |> Seq.contains x.Name)
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
                        |Name(n) -> sprintf "%A" n
                        |Attribute(a, al) -> sprintf "%A" (a,al)
                        |Value(v) -> sprintf "%A" v
                        |Missing(m) -> sprintf "%A" m
                    yield res
            } 
        |None -> [] |> Seq.ofList
