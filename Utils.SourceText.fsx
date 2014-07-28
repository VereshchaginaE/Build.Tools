#r @"Fake/FakeLib.dll"
#load "Core.fsx"

open Fake
open Fake.Git
open Core
open System.IO

let pathToRepository = @".."
let pathToDll = @"..\Bin\Release\v40"
let pathToSolution = @"..\src\YC.Utils.SourceText.sln"
let pathToTests = @"..\Bin\Release\v40\Utils.SourceText.Tests.dll"
let pathToTools = @"..\tools\Build.Tools"
let pathToPackages = @"..\src\packages"
let pathToNuspec = @"..\src\YC.Utils.SourceText\Utils.SourceText.nuspec"
let pathToNuspecFromRoot = @"src\YC.Utils.SourceText\Utils.SourceText.nuspec"
let pathToAssembleyInfo = @"..\src\YC.Utils.SourceText\AssemblyInfo.fs"
let pathToAssembleyInfoFromRoot = @"src\YC.Utils.SourceText\AssemblyInfo.fs"
let pushURL = @"https://www.myget.org/F/yc/api/v2/package"
let pushApiKey = @"f6ba9139-9d42-4cf1-acaf-344f963ff807"

let commitMessage = @"Change version of package in AssemblyInfo and Nuspec files (beta)"
let gitCommandToCommit = sprintf "commit -m \"%s\" \"%s\" \"%s\"" commitMessage pathToAssembleyInfoFromRoot pathToNuspecFromRoot
let gitUserName = "YcGeneralUser"
let gitPassword = "yc2GeneralUser2014"
let gitEmail = "yc.teamcity@gmail.com"
let gitCommandToPush = sprintf "push --repo https://\"%s\":\"%s\"@github.com/YaccConstructor/YC.Utils.SourceText.git" gitUserName gitPassword
let gitConfigUser = sprintf "config --global user.email \"%s\"" gitEmail
let gitConfigEmail = sprintf "config --global user.name \"%s\"" gitUserName

let configList = "config --list"

config.["build:solution"] <- pathToSolution
config.["core:tools"] <- pathToTools
config.["bin:path"] <-pathToDll
config.["repo:path"] <- pathToRepository
config.["test:path"] <- pathToTests
config.["packaging:nuspecpath"] <- pathToNuspec
config.["packaging:packages"] <- pathToPackages
config.["packaging:assemblyinfopath"] <- pathToAssembleyInfo
config.["packaging:deploypushurl"] <- pushURL
config.["packaging:deployapikey"] <- pushApiKey

Target "Version" (fun x ->
    Versioning.update (mapOfDict config) x
    Versioning.updateDeploy (mapOfDict config) x
)
Target "Commit" (fun _ ->
    gitCommand pathToRepository configList
    gitCommand pathToRepository gitConfigUser
    gitCommand pathToRepository gitConfigEmail
    gitCommand pathToRepository configList

    gitCommand pathToRepository gitCommandToCommit
)
Target "PushChanges" (fun _ ->
    gitCommand pathToRepository gitCommandToPush
)
Target "RestorePackages"    <| Packaging.restore (mapOfDict config)
Target "Clean"              <| Solution.clean (mapOfDict config)
Target "Build"              <| Solution.build (mapOfDict config)
Target "TestRun"            <| Test.run (mapOfDict config)
Target "Package"            <| Packaging.packageDeploy (mapOfDict config)
Target "PushPackage"        <| Packaging.pushDeploy (mapOfDict config)
Target "DefaultTarget"      <| DoNothing

"RestorePackages"
    ==> "Clean"
    ==> "Build"
    ==> "TestRun"
    ==> "Version"
    ==> "Package"
    ==> "PushPackage"
    //==> "Commit"
    //==> "PushChanges"
    ==> "DefaultTarget"

RunParameterTargetOrDefault "target" "DefaultTarget"