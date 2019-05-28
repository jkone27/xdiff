module XmlDifference.XmlNode

    open Types
    open System.Xml.Linq

    //type extension
    type XmlNode with
        member internal this.Name() =
            match this with
            |Node(name,_,_) -> name
            |Leaf(name,_,_) -> name
            |Empty -> failwith "empty node, no name"

    // requires - open System.Runtime.CompilerServices
    //[<Extension>]
    //type internal XlmNodeExtensions =
    //    [<Extension>]
    //    static member Name (node : XmlNode) = 
    //        match node with
    //        |Node(name,_,_) -> name
    //        |Leaf(name,_,_) -> name
    //        |Empty -> failwith "empty node, no name"

    let internal addNode parent child =
        match parent with
        |Node(name,attributes,childs) ->
            Node(name,attributes, child::childs)
        |Leaf(_) ->
            failwith "parent is a leaf, cannot add child to a leaf"
        |Empty -> child

    let internal GetAttributes (xel : XElement ) =
        xel.Attributes() 
        |> Seq.map (fun a -> a.Name.LocalName, a.Value)
        |> Map.ofSeq

    let rec internal xelToDomain (x : XElement) (accumulator : XmlNode) : XmlNode  =
        let name = x.Name.LocalName
        match x.HasElements with
        |true -> 
            let childs = 
                x.Elements() 
                |> Seq.map (fun e -> xelToDomain e Empty) 
                |> Seq.toList
            addNode accumulator (Node(name, GetAttributes x, childs))
        |false -> 
            addNode accumulator (Leaf(name, GetAttributes x, x.Value))

    
    let private findDistinctChildNames (childs : XmlNode seq) =
        childs |> Seq.map (fun n -> n.Name()) |> Set.ofSeq

    let internal findDifferentChilds childsA childsB =
        let childNamesA = findDistinctChildNames childsA
        let childNamesB = findDistinctChildNames childsB
        let exA = ((childNamesA, childNamesB) ||> Set.difference) 
        let exB = ((childNamesB, childNamesA) ||> Set.difference)
        in (exA, exB) 

