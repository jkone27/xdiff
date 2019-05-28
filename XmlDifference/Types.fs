module XmlDifference.Types  

open System.Xml.Linq

    type Attributes = Map<string,string>

    type internal XmlNode = 
        | Node of (string * Attributes * XmlNode list) 
        | Leaf of string * Attributes * string 
        | Empty

    type Diff = { Name : string; A : string option; B : string option; }

    type LoadedDocument = {
        Name : string;
        Content : XElement
    }

    [<CLIMutable>]
    type DiffRequest = {
        A : LoadedDocument;
        B : LoadedDocument
    }
    
    type AttributeDiff = 
           | Value of Diff
           | Missing of Diff

    type internal AttributeDifference = { Value : Diff; Missing : System.String; }

    type internal NodeDiff = 
        | Attribute of string * (AttributeDiff list)
        | Value of Diff 
        | Missing of Diff

    type internal FileDiff = { A : string; B : string ; Diffs : NodeDiff list }