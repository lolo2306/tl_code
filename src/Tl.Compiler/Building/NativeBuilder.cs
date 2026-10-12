using System.Diagnostics;
using System.Runtime.InteropServices;
using Tl.Compiler.Diagnostics;

namespace Tl.Compiler.Building;

public static class NativeBuilder
{
    private const string ProjectFileContent = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net10.0</TargetFramework>
            <AssemblyName>GeneratedProgram</AssemblyName>
            <InvariantGlobalization>true</InvariantGlobalization>
            <Nullable>enable</Nullable>
          </PropertyGroup>
        </Project>
        """;

    public static void Build(string csharpSource, string outputPath)
    {
        var tempDir = Directory.CreateTempSubdirectory("tlc-").FullName;

        try
        {
            File.WriteAllText(Path.Combine(tempDir, "Program.cs"), csharpSource);
            File.WriteAllText(Path.Combine(tempDir, "GeneratedProgram.csproj"), ProjectFileContent);

            var publishDir = Path.Combine(tempDir, "publish");
            var rid = ResolveRid();

            RunDotnet(
                tempDir,
                "publish",
                "-c", "Release",
                "-r", rid,
                "--self-contained", "true",
                "-p:PublishSingleFile=true",
                "-p:IncludeNativeLibrariesForSelfExtract=true",
                "-o", publishDir);

            var exeName = "GeneratedProgram" + (OperatingSystem.IsWindows() ? ".exe" : string.Empty);
            var builtExecutable = Path.Combine(publishDir, exeName);

            if (!File.Exists(builtExecutable))
            {
                throw new TlCompilerException("La publication .NET n'a pas produit d'exécutable");
            }

            var finalOutputPath = outputPath + (OperatingSystem.IsWindows() ? ".exe" : string.Empty);
            File.Copy(builtExecutable, finalOutputPath, overwrite: true);

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(
                    finalOutputPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            }
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static string ResolveRid()
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new TlCompilerException("Seul Linux est supporté pour l'instant (phase 1 de la roadmap)");
        }

        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "linux-x64",
            Architecture.Arm64 => "linux-arm64",
            var arch => throw new TlCompilerException($"Architecture non supportée : {arch}"),
        };
    }

    private static void RunDotnet(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new TlCompilerException("Impossible de démarrer le processus 'dotnet'");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new TlCompilerException($"Échec de 'dotnet publish' :\n{stdout}\n{stderr}");
        }
    }
}
