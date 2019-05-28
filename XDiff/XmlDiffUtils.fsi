module XDiff.XmlDiffUtils
    open System.Xml.Linq
   
    val Diff : (string * XElement) * (string * XElement) * string seq -> string seq
    val FilesDiff : string * string * string seq -> string seq
    val LoadXmlDocument : string -> XElement
    //note: currently ocaml-style modules are used to implement encapsulation (e.g. only this public signature is exposed to the client)
    //while this can be interesting as a study, its not the best / compact way to do it. XmlDiffUtils should be split into
    //different services, and interface abstraction should be favoured over modules abstraction  (maybe).
