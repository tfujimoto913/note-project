using NoteProject.Macro;
namespace NoteProject.Tools;
public static class Runset
{
    public static int Daily(string[] args)
    {
        // まとめ実行の雛形（今はスタブ）
        MacroCollector.Run(new[]{ "--interval","6h" });
        MacroAnalyzer.Run(new[]{ "--lookback","30d","--base","6h" });
        MacroSnapshot.Run(System.Array.Empty<string>());
        System.Console.WriteLine("[stub] runset.daily");
        return 0;
    }
}
