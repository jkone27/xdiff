namespace XmlDifference.Test

open System
open System.Xml.Linq
open XmlDifference.DiffExtensions   
open XmlDifference.Types
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Data
open System.Text.RegularExpressions


type XmlType = XmlProvider<"sample.xml">

[<AbstractClass; Sealed>]
type Assert private() =
    
    static member RemoveSpecialChars(str) =
          Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled)
    
    static member StringEqualsIgnoreSpecialChars(expected: string, actual: string) =
        let expectedFormat = Assert.RemoveSpecialChars(expected)
        let actualFormat = Assert.RemoveSpecialChars(actual)
        Assert.AreEqual(expectedFormat, actualFormat)

[<TestClass>]
type XDiffTests () =

    [<TestMethod>]
    member this.``With two identical xml files diff sequence contains zero entries`` () =

        let first = XmlType.GetSample()
        let second = XmlType.GetSample()

        let diffRequest = {
            A =  {
                Name = "a"; Content = first.XElement
            };
            B =  {
                Name = "b"; Content = second.XElement
            }
        }

        let x = diffRequest.Difference()
        Assert.IsTrue(x |> Seq.isEmpty);

    [<TestMethod>]
    member this.``The second file has one line difference, in one node element.`` () =

        let first = XmlType.GetSample()
        let second = XmlType.GetSample()
        let firstBook = second.Books.[0].XElement
        firstBook.SetElementValue(XName.Get "author", "test")
        let diffRequest = {
                   A =  {
                       Name = "a"; Content = first.XElement
                   };
                   B =  {
                       Name = "b"; Content = second.XElement
                   }
               } 
        let x = diffRequest.Difference() |> Seq.toArray
        Assert.IsTrue(x.Length = 1)
        let expected = """value: {Name = "author";
        A = Some "Gambardella, Matthew";
        B = Some "test";}"""
        let diff = x.[0]
        Assert.StringEqualsIgnoreSpecialChars(diff, expected);

    
    [<TestMethod>]
    member this.``The second file has one line difference, in one node element. (string ext)`` () =

        let first = XmlType.GetSample()
        let second = XmlType.GetSample()
        let firstBook = second.Books.[0].XElement
        firstBook.SetElementValue(XName.Get "author", "test")
        
        //mocking the file loader
        let fileLoader fileName =
            match fileName with
            |"a.xml" -> first.XElement
            |"b.xml" -> second.XElement
            |_ -> failwith "error"
        
        let x = "a.xml".Difference("b.xml", fileLoader = fileLoader) |> Seq.toArray
        Assert.IsTrue(x.Length = 1)
        let expected = """value: {Name = "author";
        A = Some "Gambardella, Matthew";
        B = Some "test";}"""
        let diff = x.[0]
        Assert.StringEqualsIgnoreSpecialChars(diff, expected);

  




    

