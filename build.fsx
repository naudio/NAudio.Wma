#r "packages/FAKE/tools/FakeLib.dll"
open Fake 
open System.IO

let buildDir = "" // using the defaults build output
let appReferences = !! "./*.sln" // building from the solution file

let deployDir = "./BuildArtefacts/"
let testDir = "./NAudio.WindowsMediaFormat.Tests/bin/Debug/"
let testDlls = !! (testDir + "*Tests.dll")

Target "DebugBuild" (fun _ ->
    MSBuildDebug buildDir "Build" appReferences
        |> ignore //Log "Build output: "
)

Target "ReleaseBuild" (fun _ ->
    MSBuildRelease buildDir "Build" appReferences
        |> Log "Build output: "
)

Target "Test" (fun _ ->
    trace "Running unit tests"
    testDlls
    |> NUnit (fun p -> 
        {p with
            ExcludeCategory = "IntegrationTest";
            DisableShadowCopy = true; 
            OutputFile = testDir + "TestResults.xml"})
)

Target "Clean" (fun _ ->
    trace "Cleaning up"
    MSBuildDebug buildDir "Clean" appReferences
        |> Log "Debug clean: "
    MSBuildRelease buildDir "Clean" appReferences
        |> Log "Release clean: "
    CleanDirs [deployDir]
)

Target "NuGet" (fun _ ->
    NuGet (fun p -> 
        {p with
            Version = "1.0.1"
            WorkingDir = "."
            OutputPath = deployDir
            
            Publish = false }) 
            "NAudio.Wma.nuspec"

)

Target "Release" DoNothing

Target "ZipAll" DoNothing

// a bit hacky, but persuading CreateZipOfIncludes to create the directory structure we want
let demoIncludes = 
    !! "**"
    -- "**/*.pdb"
    -- "*.vshost.*"
    -- "*nunit*"
    
Target "ZipSource" (fun _ ->
    let errorCode = Shell.Exec( "git","archive --format zip --output " + deployDir + "NAudio.Wma.Source.zip master", ".")
    ()
)

// Create a zip release library
Target "ZipLib" (fun _ ->
    let zipFiles = [@".\NAudio.WindowsMediaFormat\bin\Release\NAudio.WindowsMediaFormat.dll";
        @".\NAudio.WindowsMediaFormat\bin\Release\NAudio.WindowsMediaFormat.xml.dll";
        "license.txt";
        "readme.md"
        ]
    let flatten = true
    let comment = ""
    let workingDir = "."
    CreateZip workingDir (deployDir + "NAudio.Wma.Release.zip") comment DefaultZipLevel flatten zipFiles
)

"Clean" 
    ==> "DebugBuild"
    ==> "Test"
    ?=> "ReleaseBuild"
    ==> "Release"

"ZipLib" ==> "ZipAll"
"ZipSource" ==> "ZipAll"

"ReleaseBuild" ==> "ZipLib" 

RunTargetOrDefault "Test"