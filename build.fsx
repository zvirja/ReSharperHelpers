#r @"build/tools/FAKE.Core/tools/FakeLib.dll"

open Fake
open Fake.AppVeyor
open System
open System.Text.RegularExpressions

let solutionPath =  @"code\AlexPovar.ReSharperHelpers.sln" |> FullName
let testsProjectDir = @"code\AlexPovar.ReSharperHelpers.Tests" |> FullName
let testsAssemblyName = "AlexPovar.ReSharperHelpers.Tests.dll"
let nuspecFilePath = @"code\AlexPovar.ReSharperHelpers.nuspec" |> FullName
let tmpBuildDir = "build"
let nugetRestoreDir = tmpBuildDir </> "packages" |> FullName
let nugetOutputDir = tmpBuildDir </> "NuGetPackages" |> FullName
let buildOutDir = tmpBuildDir </> "Artifacts" |> FullName
let testResultFile = tmpBuildDir </> "TestResult.xml"


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
    let nugetVersion = match commitsNum with
                       | 0 -> sprintf "%d.%d.%d%s" major minor revision optPreReleaseSuffix
                       | _ -> sprintf "%d.%d.%d%s.%d" major minor revision optPreReleaseSuffix commitsNum

    let infoVersion = match commitsNum with
                      | 0 -> nugetVersion
                      | _ -> sprintf "%s-%s" nugetVersion sha

    { assemblyVersion=assemblyVersion; fileVersion=fileVersion; infoVersion=infoVersion; nugetVersion=nugetVersion }
    
let currentBuildVersion = match getBuildParamOrDefault "BuildVersion" "git" with
                          | "git" -> calculateVersionFromGit (getBuildParamOrDefault "BuildNumber" "0" |> int)
                          | "dev" -> 
                              let registryPath = @"Software\Zvirja\ResharperHelpersBuild";
                              let getStorageKey() = getRegistryKey HKEYCurrentUser registryPath true
                              
                              let registryKey = match getStorageKey() with
                                                | null -> createRegistrySubKey HKEYCurrentUser registryPath
                                                          getStorageKey()
                                                | v -> v
                              
                              let seedValueName = "LastDevBuildSeed"
                              let currentSeed = registryKey.GetValue(seedValueName, "100")
                                                |> Convert.ToInt32
                                                |> (+) 1
                                                
                              // Store increased seed for the next build
                              registryKey.SetValue(seedValueName, currentSeed.ToString())
                              
                              let randomVersion = sprintf "1.0.0.%d" currentSeed
                              
                              { assemblyVersion = "1.0.0";
                                fileVersion = "1.0.0";
                                infoVersion = randomVersion;
                                nugetVersion = randomVersion; }
                                
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
    let hostIdentifier = getBuildParamOrDefault "DevHostId" ""

    let properties = 
        [
            "Configuration",        buildConfig
            "OutputPath",           buildOutDir
            "AssemblyVersion",      currentBuildVersion.assemblyVersion
            "FileVersion",          currentBuildVersion.fileVersion
            "InformationalVersion", currentBuildVersion.infoVersion
            "DevHostId",            hostIdentifier
        ]
    
    !! solutionPath
    |> MSBuild "" "Build" properties
    |> ignore
)

Target "Tests" (fun _ ->
    setEnvironVar "JetProductHomeDir" testsProjectDir

    !! (buildOutDir </> testsAssemblyName)
    |> Fake.NUnitSequential.NUnit (fun p -> {p with OutputFile = testResultFile
                                                    TimeOut = TimeSpan.FromMinutes 30.0 })
)

Target "NuGetPack" (fun _ ->
    nuspecFilePath 
    |> NuGetPack (fun p -> {p with Version      = currentBuildVersion.nugetVersion
                                   WorkingDir   = buildOutDir
                                   OutputPath   = nugetOutputDir })
)

Target "CompleteBuild" DoNothing

let publishNuget feed key =
    !! (nugetOutputDir </> "*.nupkg")
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
                                         (getBuildParam "NuGetPublicKey") )
                                         
Target "PublishNuGetPrivate" (fun _ -> publishNuget 
                                         "https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2/package" 
                                         (getBuildParam "NuGetPrivateKey") )
// ==============================================
// ================== AppVeyor ==================
// ==============================================

// Add helper to identify whether current trigger is PR
type AppVeyorEnvironment with
    static member IsPullRequest = isNotNullOrEmpty AppVeyorEnvironment.PullRequestNumber

type AppVeyorTrigger = Invalid | SemVerTag | PR | DevelopBranch | ConsumeEapBranch | UnknownBranchOrTag
let appVeyorTrigger =
    if buildServer <> BuildServer.AppVeyor then
        Invalid
    else
        let tag = if AppVeyorEnvironment.RepoTag then Some AppVeyorEnvironment.RepoTagName else None
        let isPR = AppVeyorEnvironment.IsPullRequest
        let branchName = AppVeyorEnvironment.RepoBranch

        match tag, isPR, branchName with
        | Some t, _, _ when "^v\d.*" >** t  -> SemVerTag
        | _, true, _                        -> PR
        | _, _, "develop"                   -> DevelopBranch
        | _, _, "feature/consume-eap"       -> ConsumeEapBranch
        | _                                 -> UnknownBranchOrTag

Target "AppVeyor_DescribeState" (fun _ ->
   logfn "[AppVeyor state] Is AppVeyor: %b, is tag: %b, tag name: '%s', is PR: %b, branch name: '%s', trigger: %A"
         (buildServer = BuildServer.AppVeyor)
         AppVeyorEnvironment.RepoTag
         AppVeyorEnvironment.RepoTagName
         AppVeyorEnvironment.IsPullRequest
         AppVeyorEnvironment.RepoBranch
         appVeyorTrigger
)

Target "AppVeyor_InvalidTrigger" (fun _ -> traceError "Unable to resolve AppVeyor trigger")

Target "AppVeyor_PublishTestResults" (fun _ ->
    if TestFile testResultFile then UploadTestResultsFile NUnit testResultFile
)

Target "AppVeyor" (fun _ ->
    if not AppVeyorEnvironment.IsPullRequest then UpdateBuildVersion currentBuildVersion.fileVersion
)

"Clean"
    ==> "NuGetRestore"
    ==> "Build"
    =?> ("Tests", (getBuildParamOrDefault "RunTests" "true") <> "false" )
    ==> "NuGetPack"
    ==> "CompleteBuild"

"CompleteBuild" ==> "PublishNuGetPublic"
"CompleteBuild" ==> "PublishNuGetPrivate"

// AppVeyor CI
dependency "AppVeyor" <| match appVeyorTrigger with
                         | SemVerTag             -> "PublishNuGetPublic"
                         | DevelopBranch         -> "PublishNuGetPrivate"
                         | ConsumeEapBranch | PR -> "CompleteBuild"
                         | UnknownBranchOrTag    -> "Build"
                         | _                     -> "AppVeyor_InvalidTrigger"

"Tests"
    ?=> "AppVeyor_PublishTestResults"
    ==> "AppVeyor"

"AppVeyor_DescribeState" ?=> "Clean"
"AppVeyor_DescribeState" ?=> "AppVeyor_InvalidTrigger"
"AppVeyor_DescribeState" ==> "AppVeyor"

// ========= ENTRY POINT =========
RunTargetOrDefault "CompleteBuild"