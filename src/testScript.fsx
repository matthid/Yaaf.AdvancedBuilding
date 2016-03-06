#I @"../packages/FAKE/tools/"
#r @"FakeLib.dll"

// Script for testing things.
open Fake
open Fake.Git

Target "Test" (fun _ ->
  try
    [ "test.dll" ]
      |> NUnit (fun _ -> failwith "test")
  with
  | exn ->
   ()
)

RunTargetOrDefault "Test"


open System

// def version 1
type TestRecordV1 =
  { Field1 : string
    Feild2 : string } 
  static member Emtpy =
    { Field1 = null
      Feild2 = null }
// usage
let v1var1 = { TestRecordV1.Emtpy with Field1 = "field1" }
let v1var2 = { TestRecordV1.Emtpy with Feild2 = "field2" }
let v1var3 = { TestRecordV1.Field1 = "field1"; Feild2 = "field2" }
let v1access1 = v1var1.Field1
let v1access2 = v1var1.Feild2

// def version 2
type TestRecordV2 =
  { Field1 : string
    [<Obsolete("Use Field2 instead")>]
    Feild2 : string
    Field2 : string } 
  static member Emtpy =
    { Field1 = null
      Feild2 = null
      Field2 = null } // can we somehow hide the warning here
                      // (as it is obvious that we need to address it)
                      // at least limit to the "Feild2 = null" line
// usage
let v2var1 = { TestRecordV2.Emtpy with Field1 = "field1" } // why?
let v2var2 = { TestRecordV2.Emtpy with Feild2 = "field2" }
let v2var3 = { TestRecordV2.Emtpy with Field2 = "field2" } // why?
//let v2var3 = { TestRecordV2.Field1 = "field1"; Feild2 = "field2" } // broken, should never be used anyway (not even in version1)
let v2access1 = v2var1.Field1
let v2access2 = v2var1.Feild2
let v2access3 = v2var1.Field2

// def version 3
type TestRecordV3 =
  { Field1 : string
    Field2 : string } 
  static member Emtpy =
    { Field1 = null
      Field2 = null } // was broken
  [<Obsolete("Use Field2 instead")>]
  member x.Feild2 = x.Field2
// usage
let v3var1 = { TestRecordV3.Emtpy with Field1 = "field1" }
//let v3var2 = { TestRecordV3.Emtpy with Feild2 = "field2" } // broken
let v3var3 = { TestRecordV3.Emtpy with Field2 = "field2" }
let v3access1 = v3var1.Field1
let v3access2 = v3var1.Feild2 // obsolete, OK
let v3access3 = v3var1.Field2