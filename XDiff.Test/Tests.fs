namespace XDiff.Test

open System
open System.Xml.Linq
open XDiff
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Data


type XmlType = XmlProvider<"sample.xml">

[<TestClass>]
type XDiffTests () =

    [<TestMethod>]
    member this.``With two identical xml files diff sequence contains zero entries`` () =

        let first = XmlType.GetSample()
        let second = XmlType.GetSample()
        let x = XmlDiffUtils.Diff(("first",first.XElement),("second",second.XElement),[])
        Assert.IsTrue(x |> Seq.isEmpty);

    [<TestMethod>]
    member this.``The second file has one line difference, in one node element.`` () =

        let first = XmlType.GetSample()
        let second = XmlType.GetSample()
        let firstBook = second.Books.[0].XElement
        firstBook.SetElementValue(XName.Get "author", "test")
        let x = XmlDiffUtils.Diff(("first",first.XElement),("second",second.XElement),[])
        Assert.IsFalse(x |> Seq.isEmpty);
