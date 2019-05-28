﻿module XDiff.Types  

open System.Xml.Linq

    type internal XmlNode = 
        | Node of (string * Map<string,string> * XmlNode list) 
        | Leaf of string * Map<string,string> * string 
        | Empty

    type Diff = { Name : string; A : string option; B : string option; }

    type AttributeDiff = 
        | Value of Diff
        | Missing of Diff

    type internal AttributeDifference = { Value : Diff; Missing : System.String; }

    type internal NodeDiff = 
        | Attribute of string * (AttributeDiff list)
        | Value of Diff 
        | Missing of Diff


    type LoadedDocument = {
        Name : string;
        Content : XElement
    }

    type DiffRequest = {
        A : LoadedDocument;
        B : LoadedDocument
    }

    type internal FileDiff = { A : string; B : string ; Diffs : NodeDiff list }