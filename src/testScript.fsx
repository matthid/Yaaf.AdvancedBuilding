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