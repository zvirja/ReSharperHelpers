using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.NUnit;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.CompleteBuild);

    [Solution] readonly Solution Solution;
 
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter(Name = "BuildVersion")]
    readonly string BuildVersionParam = "git";

    [Parameter(Name = "BuildNumber")]
    readonly int BuildNumberParam = 0;

    [Parameter("Specify R# installation host to where Dev artifacts should be put")]
    readonly string DevHostId = "";

    [Parameter("API Key used to publish package to R# gallery", Name = "resharper-gallery-key")]
    readonly string ReSharperGalleryKey = null;

    [Parameter(Name = "myget-key")]
    readonly string MyGetKey = null;

    static readonly string ProjectName = "AlexPovar.ReSharperHelpers";
    static readonly string TestProjectName = "AlexPovar.ReSharperHelpers.Tests";
    static readonly string NuSpecFileName = "AlexPovar.ReSharperHelpers.nuspec";
    static readonly AbsolutePath ArtifactsDir = RootDirectory / "artifacts";
    static readonly AbsolutePath OutputDir = ArtifactsDir / "output";
    static readonly AbsolutePath TestResultFile = ArtifactsDir / "testResult.xml";
    static readonly AbsolutePath NuGetPackageOutDir = ArtifactsDir / "nugetPackages";
    
    BuildVersionInfo CurrentBuildVersion;

    Target CalculateVersion => _ => _
        .Executes(() =>
        {
            Info($"Build version: {BuildVersionParam}");

            CurrentBuildVersion = BuildVersionParam switch
            {
                "git" => GitVersioning.CalculateVersionFromGit(BuildNumberParam),
                "dev" => CalculateDevVersion(),
                var ver => new BuildVersionInfo {AssemblyVersion = ver, FileVersion = ver, InfoVersion = ver, NuGetVersion = ver}
            };
            
            Info($"Calculated version: {CurrentBuildVersion}");

            BuildVersionInfo CalculateDevVersion()
            {
                const string registryPath = @"Software\Zvirja\ReSharperHelpersBuild";
                var registryKey = Registry.CurrentUser.OpenSubKey(registryPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(registryPath, writable: true);

                const string seedValueName = "LastDevBuildSeed";
                var currentSeed = (int)registryKey.GetValue(seedValueName, 100) + 1;
                
                // Store increased seed for the next build
                registryKey.SetValue(seedValueName, currentSeed, RegistryValueKind.DWord);

                var devVersion = $"1.0.0.{currentSeed}";
                return new BuildVersionInfo {AssemblyVersion = "1.0.0", FileVersion = "1.0.0", InfoVersion = devVersion, NuGetVersion = devVersion};
            }
        });

    Target Clean => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDir);
        });

    Target Prepare => _ => _
        .DependsOn(CalculateVersion, Clean)
        .Executes(() => { });

    Target Restore => _ => _
        .Executes(() =>
        {
            MSBuild(c => c
                .SetConfiguration(Configuration)
                .SetTargets("Restore")
                .SetSolutionFile(Solution.Path)
                .SetVerbosity(MSBuildVerbosity.Minimal)
            );
        });
    
    Target Compile => _ => _
        .DependsOn(Prepare, Restore)
        .Executes(() =>
        {
            MSBuild(c => c
                .SetConfiguration(Configuration)
                .SetTargets("Build")
                .SetSolutionFile(Solution.Path)
                .SetVerbosity(MSBuildVerbosity.Minimal)
                .SetOutDir(OutputDir)
                .AddProperty("AssemblyVersion", CurrentBuildVersion.AssemblyVersion)
                .AddProperty("FileVersion", CurrentBuildVersion.FileVersion)
                .AddProperty("InformationalVersion", CurrentBuildVersion.InfoVersion)
                .AddProperty("DevHostId", DevHostId)
            );
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProject = Solution.GetProject(TestProjectName);
            
            var testAssemblyPath = OutputDir / $"{TestProjectName}.dll";

            NUnit3(c => c
                .AddInputFiles(testAssemblyPath)
                .SetResults(TestResultFile)
                .SetFramework("net-4.5")
                .SetTimeout(30_000)
                .AddProcessEnvironmentVariable("JetProductHomeDir", testProject!.Directory)
            );
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var waveVersion = Solution.GetProject(ProjectName)
                .GetMSBuildProject(Configuration)
                .GetItems("PackageReference")
                .Single(r => r.EvaluatedInclude == "Wave")
                .GetMetadataValue("Version");

            NuGetPack(c => c
                .SetTargetPath(Solution.Directory / NuSpecFileName)
                .SetBasePath(OutputDir)
                .SetVersion(CurrentBuildVersion.NuGetVersion)
                .AddProperty("WaveVersion", waveVersion)
                .SetOutputDirectory(NuGetPackageOutDir)
            );
        });

    Target CompleteBuild => _ => _
        .DependsOn(Pack);

    Target PublishMyGet => _ => _
        .Requires(() => MyGetKey)
        .DependsOn(Pack)
        .Executes(() =>
        {
            var nugetPackage = GlobFiles(NuGetPackageOutDir, "*.nupkg").Single();
            NuGetPush(c => c
                .SetTargetPath(nugetPackage)
                .SetApiKey(MyGetKey)
                .SetSource("https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2/package")
            );
        });
    
    Target PublishGallery => _ => _
        .Requires(() => ReSharperGalleryKey)
        .DependsOn(Pack)
        .Executes(() =>
        {
            var nugetPackage = GlobFiles(NuGetPackageOutDir, "*.nupkg").Single();
            NuGetPush(c => c
                .SetTargetPath(nugetPackage)
                .SetApiKey(ReSharperGalleryKey)
                .SetSource("https://plugins.jetbrains.com/")
            );
        });
    
    // ==============================================
    // ================== AppVeyor ==================
    // ==============================================

    static AppVeyor AppVeyorEnv => AppVeyor.Instance ?? throw new InvalidOperationException("Is not AppVeyor CI");

    Target AppVeyor_DescribeState => _ => _
        .Before(Prepare)
        .Executes(() =>
        {
            var env = AppVeyorEnv;
            var trigger = ResolveAppVeyorTrigger();
            Info($"Is tag: {env.RepositoryTag}, tag name: '{env.RepositoryTagName}', PR number: {env.PullRequestNumber}, branch name: '{env.RepositoryBranch}', trigger: {trigger}");
        });

    Target AppVeyor_UploadTestResults => _ => _
        .DependsOn(Test)
        .Executes(async () =>
        {
            Info($"Uploading tests result file: {TestResultFile}");

            var testResultBytes = await File.ReadAllBytesAsync(TestResultFile);

            using var multipartContent = new MultipartFormDataContent($"------------{DateTime.Now.Ticks:X}")
            {
                {new ByteArrayContent(testResultBytes), "file", Path.GetFileName(TestResultFile)}
            };
            
            using var httpClient = new HttpClient();
            var result = await httpClient.PostAsync($"https://ci.appveyor.com/api/testresults/nunit3/{AppVeyorEnv.JobId}", multipartContent);
            result.EnsureSuccessStatusCode();
            Info($"Successfully uploaded the file");
        });

    Target AppVeyor_Pipeline => _ => _
        .DependsOn(ResolveAppVeyorTarget(this), AppVeyor_DescribeState, AppVeyor_UploadTestResults)
        .Executes(() =>
        {
            var trigger = ResolveAppVeyorTrigger();
            if (trigger != AppVeyorTrigger.PR)
            {
                AppVeyorEnv.UpdateBuildVersion(CurrentBuildVersion.FileVersion);
                Info($"Updated build version to: '{CurrentBuildVersion.FileVersion}'");
            }
        });

    static Target ResolveAppVeyorTarget(Build build)
    {
        var trigger = ResolveAppVeyorTrigger();
        return trigger switch
        {
            AppVeyorTrigger.SemVerTag        => build.PublishGallery,
            AppVeyorTrigger.DevelopBranch    => build.PublishMyGet,
            AppVeyorTrigger.ConsumeEapBranch => build.CompleteBuild,
            AppVeyorTrigger.PR               => build.CompleteBuild,
            AppVeyorTrigger.MasterBranch     => build.CompleteBuild,
            _                                => build.Compile
        };
    }

    enum AppVeyorTrigger
    {
        Invalid,
        SemVerTag,
        PR,
        DevelopBranch,
        MasterBranch,
        ConsumeEapBranch,
        UnknownBranchOrTag
    }
    
    static AppVeyorTrigger ResolveAppVeyorTrigger()
    {
        var env = AppVeyor.Instance;
        if (env == null)
        {
            return AppVeyorTrigger.Invalid;
        }

        var tag = env.RepositoryTag ? env.RepositoryTagName : null;
        var isPr = env.PullRequestNumber != null;
        var branchName = env.RepositoryBranch;
        
        return (tag, isPr, branchName) switch
        {
            (string t, _, _) when Regex.IsMatch(t, "^v\\d.*") => AppVeyorTrigger.SemVerTag,
            (_, true, _)                                                    => AppVeyorTrigger.PR,
            (_, _, "master")                                                => AppVeyorTrigger.MasterBranch,
            (_, _, "develop")                                               => AppVeyorTrigger.DevelopBranch,
            (_, _, "feature/consume-eap")                                   => AppVeyorTrigger.ConsumeEapBranch,
            _                                                               => AppVeyorTrigger.UnknownBranchOrTag
        };
    }
}
