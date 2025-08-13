using System.Diagnostics;
using NoteProject.Shared;

namespace NoteProject.Macro;

public static class MacroCollector
{
    public static int Run(string[] args)
    {
        string interval = GetArg(args, "--interval", "6h");
        var ts = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9)); // JST
        string outPath = Path.Combine(PathUtil.InputMacroDir, ts.ToString("yyyy-MM"), ts.ToString("yyyy-MM-dd_HH-mm")) + ".json";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var psi = new ProcessStartInfo
        {
            FileName = DetectPythonExecutable(),
            ArgumentList = { Path.Combine(PathUtil.ScriptsDir, "macro_fetch.py"), "--interval", interval, "--out", outPath },
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
        };
        var p = Process.Start(psi)!;
        p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        p.ErrorDataReceived  += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
        p.BeginOutputReadLine(); p.BeginErrorReadLine(); p.WaitForExit();

        if (p.ExitCode != 0) { Console.Error.WriteLine($"macro_fetch.py failed with code {p.ExitCode}"); return p.ExitCode; }

        Log($"macro.collect saved → {outPath} (interval={interval})");
        return 0;
    }

    static string GetArg(string[] args, string key, string def){ var i = Array.IndexOf(args, key); return (i>=0 && i+1<args.Length)? args[i+1] : def; }

    static string DetectPythonExecutable()
    {
        foreach (var c in new[] { "python3", "python", "/usr/bin/python3", "/opt/homebrew/bin/python3" })
        {
            try { var p = Process.Start(new ProcessStartInfo{ FileName = c, ArgumentList = { "--version" }, UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardError=true }); p?.WaitForExit(3000); if (p?.ExitCode==0) return c; } catch {}
        }
        throw new Exception("Python 3 が見つかりません。PATH を確認してください。");
    }

    static void Log(string msg){ Directory.CreateDirectory(PathUtil.LogsDir); File.AppendAllText(Path.Combine(PathUtil.LogsDir,"analysis_log.json"), $"{DateTimeOffset.Now:O}\tmacro.collect\t{msg}\n"); }
}
