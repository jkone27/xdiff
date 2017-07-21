module XDiff.Types

    type XmlNode = 
        | Node of (string * Map<string,string> * XmlNode list) 
        | Leaf of string * Map<string,string> * string 
        | Empty

    type Diff = { Name : string; A : string; B : string; }

    type AttributeDiff = 
        | Value of Diff
        | Missing of string

    type NodeDiff = 
        | Name of Diff 
        | Attribute of string * (AttributeDiff list)
        | Value of Diff 
        | Missing of string

    type FileDiff = { A : string; B : string ; Diffs : NodeDiff list }
