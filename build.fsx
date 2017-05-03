#r @"build/tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open System
open System.Text.RegularExpressions

let solutionPath =  @"code\AlexPovar.ReSharperHelpers.sln" |> FullName
let testsProjectDir = @"code\AlexPovar.ReSharperHelpers.Tests" |> FullName
let testsAssemblyName = "AlexPovar.ReSharperHelpers.Tests.dll"
let nuspecFilePath = @"code\AlexPovar.ReSharperHelpers.nuspec" |> FullName
let tmpBuildDir = "build"
let nugetRestoreDir = tmpBuildDir @@ "packages" |> FullName
let nugetOutputDir = tmpBuildDir @@ "NuGetPackages" |> FullName
let buildOutDir = tmpBuildDir @@ "Artifacts" |> FullName


type BuildVersionInfo = { assemblyVersion:string; fileVersion:string; infoVersion:string; nugetVersion:string }
let calculateVersionFromGit buildNumber =
    // Example of output for a release tag: v3.50.2-288-g64fd5c5b, for a prerelease tag: v3.50.2-alpha1-288-g64fd5c5b
    let desc = Git.CommandHelper.runSimpleGitCommand "" "describe --tags --long --match=v*"

    let result = Regex.Match(desc,
                             @"^v(?<maj>\d+)\.(?<min>\d+)(\.(?<rev>\d+))?(?<pre>-\w+\d*)?-(?<num>\d+)-g(?<sha>[a-z0-9]+)$",
                             RegexOptions.IgnoreCase)
                      .Groups
    let getMatch (name:string) = result.[name].Value

    let major, minor, optRevision, optPreReleaseSuffix, commitsNum, sha =
        getMatch "maj" |> int, getMatch "min" |> int, getMatch "rev", getMatch "pre", getMatch "num" |> int, getMatch "sha"

    let revision = match optRevision with
                             | "" -> 0
                             | v -> v |> int

    let assemblyVersion = sprintf "%d.%d.%d.0" major minor revision
    let fileVersion = sprintf "%d.%d.%d.%d" major minor revision buildNumber
    
    // If number of commits since last tag is greater than zero, we append another identifier with number of commits.
    // The produced version is larger than the last tag version.
    // If we are on a tag, we use version specified modification.
    let nugetVersion = match commitsNum, revision with
                       | 0, 0 -> sprintf "%d.%d%s" major minor optPreReleaseSuffix
                       | 0, _ -> sprintf "%d.%d.%d%s" major minor revision optPreReleaseSuffix
                       | _ -> sprintf "%d.%d.%d%s.%d" major minor revision optPreReleaseSuffix commitsNum

    let infoVersion = match commitsNum with
                      | 0 -> nugetVersion
                      | _ -> sprintf "%s-%s" nugetVersion sha

    { assemblyVersion=assemblyVersion; fileVersion=fileVersion; infoVersion=infoVersion; nugetVersion=nugetVersion }

let currentBuildVersion = match getBuildParamOrDefault "BuildVersion" "git" with
                          | "git" -> calculateVersionFromGit (getBuildParamOrDefault "BuildNumber" "0" |> int)
                          | ver -> { assemblyVersion = ver;
                                     fileVersion     = ver;
                                     infoVersion     = ver;
                                     nugetVersion    = ver; }

Target "Clean" (fun _ ->
    CleanDir nugetOutputDir
    CleanDir buildOutDir
)

Target "NuGetRestore" (fun _ ->
    solutionPath
    |> RestoreMSSolutionPackages (fun p -> {p with OutputPath = nugetRestoreDir })
)

Target "Build" (fun _ ->
    let buildConfig = getBuildParamOrDefault "BuildConfig" "Release"
    let properties = 
        [
            "Configuration",        buildConfig
            "OutputPath",           buildOutDir
            "AssemblyVersion",      currentBuildVersion.assemblyVersion
            "FileVersion",          currentBuildVersion.fileVersion
            "InformationalVersion", currentBuildVersion.infoVersion
        ]
    
    !! solutionPath
    |> MSBuild "" "Build" properties
    |> ignore
)

Target "Tests" (fun _ ->
    setEnvironVar "JetProductHomeDir" testsProjectDir

    !! (buildOutDir @@ testsAssemblyName)
    |> NUnit (fun p -> {p with OutputFile = tmpBuildDir @@ "TestResult.xml"
                               TimeOut = TimeSpan.FromMinutes 30.0 })
)

Target "NuGetPack" (fun _ ->
    nuspecFilePath 
    |> NuGetPack (fun p -> {p with Version      = currentBuildVersion.nugetVersion
                                   WorkingDir   = buildOutDir
                                   OutputPath   = nugetOutputDir })
)

Target "CompleteBuild" ignore

let publishNuget feed key =
    !! (nugetOutputDir @@ "*.nupkg")
    |> Seq.map GetMetaDataFromPackageFile
    |> Seq.iter (fun meta -> NuGetPublish (fun p -> {p with Project = meta.Id
                                                            Version = meta.Version
                                                            OutputPath = nugetOutputDir
                                                            WorkingDir = nugetOutputDir
                                                            PublishUrl = feed
                                                            AccessKey = key })
    )


Target "PublishNuGetPublic" (fun _ -> publishNuget 
                                         "https://resharper-plugins.jetbrains.com" 
                                         (getBuildParam "NuGetKeyPublic") )
                                         
Target "PublishNuGetPrivate" (fun _ -> publishNuget 
                                         "https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2/package" 
                                         (getBuildParam "NuGetKeyPrivate") )


"Clean"
    ==> "NuGetRestore"
    ==> "Build"
    =?> ("Tests", (getBuildParamOrDefault "RunTests" "true") <> "false" )
    ==> "NuGetPack"
    ==> "CompleteBuild"

"CompleteBuild" ==> "PublishNuGetPublic"
"CompleteBuild" ==> "PublishNuGetPrivate"

RunTargetOrDefault "CompleteBuild"