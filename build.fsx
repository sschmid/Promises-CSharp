// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO
#if MONO
#else
#load "packages/SourceLink.Fake/Tools/Fake.fsx"
open SourceLink
#endif

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Wooga.Promises.Unity3D"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "Wooga fork of Simons Promises"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Wooga fork of Simons Promises"

// List of author names (for NuGet package)
let authors = [ "Wooga" ]

// Tags for your project (for NuGet package)
let tags = ""

// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "wooga"
let gitHome = "https://github.com/" + gitOwner

// The name of the project on GitHub
let gitName = "Promises-CSharp"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.github.com/wooga"

let release = LoadReleaseNotes "RELEASE_NOTES.md"

// --------------------------------------------------------------------------------------
// Build a NuGet package

Target "NuGetAsDll" (fun _ ->
//  DLL NuGet package is disabled for now
    ()

//    NuGet (fun p ->
//        { p with
//            Authors = authors
//            Project = project
//            Summary = summary
//            Description = description
//            Version = release.NugetVersion
//            ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
//            Tags = tags
//            OutputPath = "bin"
//            WorkingDir = "."
//            AccessKey = getBuildParamOrDefault "nugetkey" ""
//            Publish = hasBuildParam "nugetkey"
//            Dependencies = [] })
//        ("nuget/" + project + ".nuspec")
)

Target "NuGetAsSource" (fun _ ->
    let sourceProject = project + ".Source"
    let nugetParams = {
      NuGetHelper.NuGetDefaults() with
        Authors = authors
        Project = sourceProject
        Summary = summary
        Description = description
        Version = release.NugetVersion
        ReleaseNotes = String.Join(Environment.NewLine, release.Notes)
        Tags = tags
        OutputPath = "./bin"
        WorkingDir = "."
        PublishUrl = "http://wooga.artifactoryonline.com/wooga/api/nuget/nuget-private"
        AccessKey = getBuildParamOrDefault "nugetkey" ""
#if MONO
        Publish = false
#else
        Publish = hasBuildParam "nugetkey"
#endif
        Dependencies = [] }

    NuGet (fun p -> nugetParams) ("nuget/" + sourceProject + ".nuspec")

#if MONO
    if hasBuildParam "nugetkey" then
      let source = sprintf "-s %s" nugetParams.PublishUrl
      let args = sprintf "push \"%s\" %s %s" (nugetParams.OutputPath @@ sprintf "%s.%s.nupkg" nugetParams.Project nugetParams.Version) nugetParams.AccessKey source
      let result =
              ExecProcess (fun info ->
                  info.FileName <- nugetParams.ToolPath
                  info.WorkingDirectory <- FullName nugetParams.WorkingDir
                  info.Arguments <- args) nugetParams.TimeOut
      //enableProcessTracing <- tracing
      if result <> 0 then failwithf "Error during NuGet push. %s %s" nugetParams.ToolPath args
#endif
)

Target "NuGet" DoNothing
Target "BuildPackage" DoNothing
Target "All" DoNothing

#load "paket-files/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit

Target "Release" (fun _ ->
  StageAll ""
  Git.Commit.Commit "" (sprintf "Bump version to %s" release.NugetVersion)
  Branches.push ""

  Branches.tag "" release.NugetVersion
  Branches.pushTag "" "origin" release.NugetVersion

  let nugetPkg = sprintf "bin/%s.%s.nupkg"
                 <| project + ".Source"
                 <| release.NugetVersion

  // release on github
  createClient (getBuildParamOrDefault "github-user" "") (getBuildParamOrDefault "github-pw" "")
  |> createDraft gitOwner gitName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
  |> uploadFile nugetPkg
  |> releaseDraft
  |> Async.RunSynchronously
)

"All"
  ==> "NuGet"
  ==> "BuildPackage"
  ==> "Release"

"NuGetAsDLL"
  ==> "NuGet"

"NuGetAsSource"
  ==> "NuGet"

RunTargetOrDefault "All"
