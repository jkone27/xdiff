module XDiff.Types
    
    type Diff = { Name : string; A : string; B : string; }

    type AttributeDiff = 
        | Value of Diff
        | Missing of string

    type internal XmlNode = 
        | Node of (string * Map<string,string> * XmlNode list) 
        | Leaf of string * Map<string,string> * string 
        | Empty

    type internal AttributeDifference = { Value : Diff; Missing : System.String; }

    type internal NodeDiff = 
        | Name of Diff 
        | Attribute of string * (AttributeDiff list)
        | Value of Diff 
        | Missing of string

    type internal FileDiff = { A : string; B : string ; Diffs : NodeDiff list }