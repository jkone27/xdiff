module XmlDifference.DiffExtensions

    open Difference
    open Types
    open System.Xml.Linq
    open System.IO
    open System.Runtime.CompilerServices
    open System.Collections.Generic

    let private defaultLoad fileName =
        XDocument.Parse(File.ReadAllText(fileName)).Root

    //user defined extension
    type DiffRequest with 
        member this.Difference (?excludedNodes: string seq) =
            let excludedNodes = defaultArg excludedNodes Seq.empty
            Diff(this,excludedNodes)

    //string extension
    [<Extension>]
    type DifferenceExtension =
      
        [<Extension>]
        static member Difference (first : System.String, second: System.String) =
            FilesDiff(first,second,Seq.empty,defaultLoad)

        [<Extension>]
        static member Difference (first : System.String, second: System.String, 
                excludedNodes: IEnumerable<System.String>) =
            FilesDiff(first,second,excludedNodes,defaultLoad)

        [<Extension>]
        static member Difference (first : System.String, second: System.String, 
                fileLoader : System.String -> XElement) =
            FilesDiff(first,second,Seq.empty,fileLoader)

        [<Extension>]
        static member Difference (first :  System.String, second:  System.String, 
                excludedNodes: IEnumerable<System.String>, 
                fileLoader :  System.String -> XElement) =
            FilesDiff(first,second,excludedNodes,fileLoader)
