#r @"Fake/FakeLib.dll"
#load "Core.fsx"
#load "Utils.fsx"

open Fake
open Fake.Git
open Core
open System.IO

let pathToSolution = @"..\src\YC.Core.sln"
let pathToNuspec = @"..\src\YaccConstructor\YC.Core.nuspec"
let pathToNuspecFromRoot = @"src\YaccConstructor\YC.Core.nuspec"
let pathToAssembleyInfo = @"..\src\YaccConstructor\AssemblyInfo.fs"
let pathToAssembleyInfoFromRoot = @"src\YaccConstructor\AssemblyInfo.fs"

let specConfig = new SpecificConfig(pathToSolution, pathToNuspec, pathToNuspecFromRoot, pathToAssembleyInfo, pathToAssembleyInfoFromRoot)
commonConfig specConfig

"Packaging:Restore"
    ==> "Solution:Clean"
    ==> "Solution:Build"
//    ==> "Test:Run"
    ==> "Versioning:Update"
    ==> "Packaging:Package"
    =?> ("Packaging:Push", not isLocalBuild)
    =?> ("Git:Commit", not isLocalBuild)
    =?> ("Git:Push", not isLocalBuild)
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"