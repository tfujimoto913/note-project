namespace NoteProject.Shared;

public static class PathUtil
{
    // bin/Debug/net8.0/… から見て src/ に戻る
    public static readonly string Root     = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
    // リポジトリ直下（src の一つ上）
    public static readonly string RepoRoot = Path.GetFullPath(Path.Combine(Root, ".."));

    public static readonly string DataDir        = Path.Combine(RepoRoot, "data");
    public static readonly string InputDir       = Path.Combine(DataDir, "input");
    public static readonly string OutputDir      = Path.Combine(DataDir, "output");
    public static readonly string InputMacroDir  = Path.Combine(InputDir, "macro");
    public static readonly string OutputMacroDir = Path.Combine(OutputDir, "macro");

    public static readonly string ScriptsDir     = Path.Combine(RepoRoot, "scripts");
    public static readonly string LogsDir        = Path.Combine(RepoRoot, "logs");
}
