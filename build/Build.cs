using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using NuGet.Packaging;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.NUnit;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.Globbing;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NUnit.NUnitTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// ReSharper disable InconsistentNaming

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
    readonly int BuildNumberParam;

    [Parameter("Specify R# installation host to where Dev artifacts should be put")]
    readonly string DevHostId = "";

    [Parameter("API Key used to publish package to R# gallery", Name = "resharper-gallery-key")]
    readonly string ReSharperGalleryKey;

    [Parameter(Name = "myget-key")]
    readonly string MyGetKey;

    static readonly string TestProjectName = "AlexPovar.ReSharperHelpers.Tests";
    static readonly AbsolutePath ChangelogFile = RootDirectory / "CHANGELOG.md";

    static readonly AbsolutePath ArtifactsDir = RootDirectory / "artifacts";
    static readonly AbsolutePath TestResultFile = ArtifactsDir / "testResult.xml";
    static readonly AbsolutePath NuGetPackageOutDir = ArtifactsDir / "nugetPackages";
    static readonly AbsolutePath RiderPackageOutDir = ArtifactsDir / "riderPackages";
    static readonly AbsolutePath RiderPackageTmpDir = TemporaryDirectory / "Rider";

    BuildVersionInfo CurrentBuildVersion;

    Target CalculateVersion => _ => _
        .Executes(() =>
        {
            Log.Information($"Build version: {BuildVersionParam}");

            CurrentBuildVersion = BuildVersionParam switch
            {
                "git" => GitVersioning.CalculateVersionFromGit(BuildNumberParam),
                "dev" => CalculateDevVersion(),
                var ver => BuildVersionInfo.Create(ver)
            };

            Log.Information($"Calculated version: {CurrentBuildVersion}");

            BuildVersionInfo CalculateDevVersion()
            {
                if (!OperatingSystem.IsWindows())
                {
                    return BuildVersionInfo.Create("1.0.0");
                }

                const string registryPath = @"Software\Zvirja\ReSharperHelpersBuild";
                var registryKey = Registry.CurrentUser.OpenSubKey(registryPath, writable: true) ?? Registry.CurrentUser.CreateSubKey(registryPath, writable: true);

                const string seedValueName = "LastDevBuildSeed";
                var currentSeed = (int)registryKey.GetValue(seedValueName, 100)! + 1;

                // Store increased seed for the next build
                registryKey.SetValue(seedValueName, currentSeed, RegistryValueKind.DWord);

                var devVersion = $"1.0.0.{currentSeed}";
                return BuildVersionInfo.Create(baseVersion: "1.0.0", infoVersion: devVersion, nuGetVersion: devVersion);
            }
        });

    Target Clean => _ => _
        .Executes(() =>
        {
          ArtifactsDir.CreateOrCleanDirectory();
          RiderPackageTmpDir.CreateOrCleanDirectory();
        });

    Target Prepare => _ => _
        .DependsOn(CalculateVersion, Clean)
        .Executes(() => { });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(c => c
                .SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.minimal)
            );
        });

    Target Compile => _ => _
        .DependsOn(Prepare, Restore)
        .Executes(() =>
        {
            // Cannot use dotnet, as build relies on 'Microsoft.Build.Utilities.v4.0' which is available for MS Build only.
            MSBuild(c => c
                .SetConfiguration(Configuration)
                .SetTargets("Build")
                .SetSolutionFile(Solution)
                .SetVerbosity(MSBuildVerbosity.Minimal)
                //
                .AddProperty("DevHostId", DevHostId)
                .SetPackageVersion(CurrentBuildVersion.NuGetVersion)
                .SetAssemblyVersion(CurrentBuildVersion.AssemblyVersion)
                .SetFileVersion(CurrentBuildVersion.FileVersion)
            );
        });

    Target Test => _ => _
        .DependsOn(Prepare, Restore, Compile)
        .Executes(() =>
        {
            var testProject = Solution.GetAllProjects(TestProjectName).Single();

            var testAssemblyPath = testProject.Directory / "bin" / TestProjectName / Configuration / $"{TestProjectName}.dll";

            NUnit3(c => c
                .AddInputFiles(testAssemblyPath)
                .SetResults(TestResultFile)
                .SetFramework("net-4.5")
                .SetTimeout((int)TimeSpan.FromMinutes(10).TotalMilliseconds)
                .AddProcessEnvironmentVariable("JetProductHomeDir", testProject.Directory)
            );
        });

    Target Pack => _ => _
        .DependsOn(Compile, Test)
        .Executes(() =>
        {
          // Magic regex from template: https://github.com/JetBrains/resharper-rider-plugin/blob/53f8339a19363c7ab639e0cd6867c21db29a5cc2/template/content/publishPlugin.ps1#L16
          var changelogEntries = Regex.Matches(File.ReadAllText(ChangelogFile), "(##.+?.+?)(?=##|$)", RegexOptions.Singleline)
            .Take(3)
            .Select(x => x.ToString());

          var changelog = string.Join("", changelogEntries);

          // Cannot use dotnet, as build relies on 'Microsoft.Build.Utilities.v4.0' which is available for MS Build only.
          MSBuild(c => c
            .SetConfiguration(Configuration)
            .SetTargets("Pack")
            .SetSolutionFile(Solution)
            .SetVerbosity(MSBuildVerbosity.Minimal)
            .SetPackageOutputPath(NuGetPackageOutDir)
            //
            .SetPackageReleaseNotes(changelog)
            .SetPackageVersion(CurrentBuildVersion.NuGetVersion)
            .SetAssemblyVersion(CurrentBuildVersion.AssemblyVersion)
            .SetFileVersion(CurrentBuildVersion.FileVersion)
            .SetInformationalVersion(CurrentBuildVersion.InfoVersion)
          );

          // Build Rider extension
          var riderNuGetDir = RiderPackageTmpDir / "NuGet";
          AbsolutePath riderNugetPackage = GlobFiles(NuGetPackageOutDir, "*Rider*.nupkg").Single();
          riderNugetPackage.UnZipTo(riderNuGetDir);

          var riderPkgRootDir = RiderPackageTmpDir / "Package";
          riderPkgRootDir.CreateOrCleanDirectory();

          var riderPkgContentDir = riderPkgRootDir / "ResharperHelpers";
          riderPkgContentDir.CreateOrCleanDirectory();

          var nuspecReader = new NuspecReader(riderNuGetDir / "AlexPovar.ReSharperHelpers.Rider.nuspec");
          var waveVersion = nuspecReader.GetDependencyGroups().SelectMany(x => x.Packages).Single(x => x.Id == "Wave").VersionRange.MinVersion.Version.Major.ToString();
          var riderPackageId = $"{nuspecReader.GetMetadataValue("id")}.{nuspecReader.GetMetadataValue("version")}";

          var pluginXmlFile = riderPkgContentDir / "META-INF" / "plugin.xml";
          pluginXmlFile.WriteAllText(
            $"""
             <idea-plugin require-restart="true">
               <depends>com.intellij.modules.rider</depends>
               <idea-version since-build="{waveVersion}.0.0" until-build="{waveVersion}.*"/>

               <id>{nuspecReader.GetMetadataValue("id")}</id>
               <version>{nuspecReader.GetMetadataValue("version")}</version>
               <name>{nuspecReader.GetMetadataValue("title")}</name>
               <vendor url="{nuspecReader.GetMetadataValue("projectUrl")}">{nuspecReader.GetMetadataValue("authors")}</vendor>
               <description>{nuspecReader.GetMetadataValue("description")}</description>
               <change-notes>{changelog.ReplaceLineEndings($"&lt;br /&gt;{Environment.NewLine}")}</change-notes>
             </idea-plugin>
             """
          );

          CopyDirectoryRecursively(riderNuGetDir / "dotFiles", riderPkgContentDir / "dotnet");

          // Build jar file. It's an empty file with metadata only. Is needed otherwise I cannot upload to marketplace
          {
            var jarContentDir = RiderPackageTmpDir / "jar";
            jarContentDir.CreateOrCleanDirectory();

            CopyFileToDirectory(pluginXmlFile, jarContentDir / "META-INF");

            var jarPkgFilePath = riderPkgContentDir / "lib" / $"{riderPackageId}.jar";
            jarPkgFilePath.Parent.CreateDirectory();
            new FastZip().CreateZip(zipFileName: jarPkgFilePath, sourceDirectory: jarContentDir, recurse: true, fileFilter: null);
          }

          var riderPackageFilePath = RiderPackageOutDir / $"{riderPackageId}.zip";
          riderPackageFilePath.Parent.CreateDirectory();
          new FastZip().CreateZip(zipFileName: riderPackageFilePath, sourceDirectory: riderPkgRootDir, recurse: true, fileFilter: null);

          Log.Information("Created Rider package: {path}", riderPackageFilePath);

          RiderPackageTmpDir.DeleteDirectory();
        });

    Target CompleteBuild => _ => _
        .DependsOn(Pack);

    Target PublishMyGet => _ => _
        .Requires(() => MyGetKey)
        .DependsOn(Pack)
        .Executes(() =>
        {
            var nugetPackage = GlobFiles(NuGetPackageOutDir, "*.nupkg").Single(x => !x.Contains("Rider"));
            NuGetPush(c => c
                .SetTargetPath(nugetPackage)
                .SetApiKey(MyGetKey)
                .SetSource("https://www.myget.org/F/alexpovar-resharperhelpers-prerelease/api/v2/package")
            );
        });

    Target PublishGallery => _ => _
        .Requires(() => ReSharperGalleryKey)
        .DependsOn(Pack)
        .Executes(async () =>
        {
            var nugetPackage = GlobFiles(NuGetPackageOutDir, "*.nupkg").Single(x => !x.Contains("Rider"));
            NuGetPush(c => c
                .SetTargetPath(nugetPackage)
                .SetApiKey(ReSharperGalleryKey)
                .SetSource("https://plugins.jetbrains.com/")
            );

            var riderPackage = GlobFiles(RiderPackageOutDir, "*.zip").Single();
            var packageId = Regex.Match(Path.GetFileName(riderPackage), pattern: @"^(.*?)[\.\d]*\.zip$").Groups[1].Value;

            var uploadPackageContent = new MultipartFormDataContent();
            uploadPackageContent.Add(new StringContent(packageId), name: "xmlId");
            uploadPackageContent.Add(new StreamContent(File.OpenRead(riderPackage)), name: "file", fileName: Path.GetFileName(riderPackage));

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ReSharperGalleryKey);

            var result = await httpClient.PostAsync(new Uri("https://plugins.jetbrains.com/plugin/uploadPlugin"), uploadPackageContent);
            if (result.IsSuccessStatusCode)
            {
              Log.Information("Published Rider Plugin: {fileName}", Path.GetFileName(riderPackage));
            }
            else
            {
              throw new InvalidOperationException($"Cannot upload Rider package: {result}");
            }
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
            Log.Information($"Is tag: {env.RepositoryTag}, tag name: '{env.RepositoryTagName}', PR number: {env.PullRequestNumber}, branch name: '{env.RepositoryBranch}', trigger: {trigger}");
        });

    Target AppVeyor_UploadTestResults => _ => _
        .DependsOn(Test)
        .Executes(async () =>
        {
            Log.Information($"Uploading tests result file: {TestResultFile}");

            var testResultBytes = await File.ReadAllBytesAsync(TestResultFile);

            using var multipartContent = new MultipartFormDataContent($"------------{DateTime.Now.Ticks:X}")
            {
                {new ByteArrayContent(testResultBytes), "file", Path.GetFileName(TestResultFile)!}
            };

            using var httpClient = new HttpClient();
            var result = await httpClient.PostAsync($"https://ci.appveyor.com/api/testresults/nunit3/{AppVeyorEnv.JobId}", multipartContent);
            result.EnsureSuccessStatusCode();
            Log.Information($"Successfully uploaded the file");
        });

    Target AppVeyor_Pipeline => _ => _
        .DependsOn(ResolveAppVeyorTarget(this), AppVeyor_DescribeState, AppVeyor_UploadTestResults)
        .Executes(() =>
        {
            AppVeyorTrigger trigger = ResolveAppVeyorTrigger();
            if (trigger != AppVeyorTrigger.PR)
            {
                AppVeyorEnv.UpdateBuildVersion(CurrentBuildVersion.FileVersion);
                Log.Information($"Updated build version to: '{CurrentBuildVersion.FileVersion}'");
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
            AppVeyorTrigger.MainBranch       => build.CompleteBuild,
            _                                => build.Compile
        };
    }

    enum AppVeyorTrigger
    {
        Invalid,
        SemVerTag,
        PR,
        DevelopBranch,
        MainBranch,
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
            ({ } t, _, _) when Regex.IsMatch(t, "^v\\d.*") => AppVeyorTrigger.SemVerTag,
            (_, true, _)                                   => AppVeyorTrigger.PR,
            (_, _, "main")                                 => AppVeyorTrigger.MainBranch,
            (_, _, "master")                               => AppVeyorTrigger.MainBranch,
            (_, _, "develop")                              => AppVeyorTrigger.DevelopBranch,
            (_, _, "feature/consume-eap")                  => AppVeyorTrigger.ConsumeEapBranch,
            _                                              => AppVeyorTrigger.UnknownBranchOrTag
        };
    }
}
