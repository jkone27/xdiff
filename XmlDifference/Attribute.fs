module XmlDifference.Attribute

    open Types

    //todo: refactor
    let internal attributesDiff 
        (attrA : Attributes) (attrB: Attributes) : AttributeDiff list =
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
    

    let internal attributeDiffs attributeName attributesA attributesB =
        let ads = attributesDiff attributesA attributesB
        match ads.IsEmpty with
        |false -> [Attribute(attributeName, ads)]
        |true -> []