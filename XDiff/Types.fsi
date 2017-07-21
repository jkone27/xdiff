module XDiff.Types
    
    
    type internal XmlNode = 
        | Node of (string * Map<string,string> * XmlNode list) 
        | Leaf of string * Map<string,string> * string 
        | Empty

    type internal Diff = { Name : string; A : string; B : string; }

    type internal AttributeDiff = 
        | Value of Diff
        | Missing of string

    type internal NodeDiff = 
        | Name of Diff 
        | Attribute of string * (AttributeDiff list)
        | Value of Diff 
        | Missing of string

    type internal FileDiff = { A : string; B : string ; Diffs : NodeDiff list }
