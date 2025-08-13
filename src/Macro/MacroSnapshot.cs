using NoteProject.Shared;
using System.Text.Json.Nodes;

namespace NoteProject.Macro;

public static class MacroSnapshot
{
    public static int Run(string[] args)
    {
        var files = Directory.Exists(PathUtil.OutputMacroDir)
            ? Directory.GetFiles(PathUtil.OutputMacroDir, "macro_analysis_*.json", SearchOption.TopDirectoryOnly).OrderBy(f => f).ToList()
            : new List<string>();
        if (files.Count == 0) { Console.Error.WriteLine("No macro_analysis_*.json found. Run macro.analyze first."); return 2; }
        var obj = JsonNode.Parse(File.ReadAllText(files[^1]))!.AsObject();

        string Headline(JsonObject o)
        {
            var b = o["ai_flow_interpretation"]!["total3_projection_bias"]!.GetValue<string>();
            return b switch { "up" => "ALT選別フロー継続、TOTAL3上向きバイアス", "down" => "BTC優位回帰、ALT逆風バイアス", _ => "フラット基調、様子見バイアス" };
        }

        var snapshot = new JsonObject
        {
            ["ts"] = obj["meta"]!["ts"]!.GetValue<string>(),
            ["headline"] = Headline(obj),
            ["free_view"] = new JsonObject { ["granularity"] = "1d", ["bullets"] = new JsonArray { "BTC.Dは直近での低下/上昇を確認", "TOTAL3の方向を日次で要確認" } },
            ["paid_view"] = new JsonObject
            {
                ["granularity"] = obj["meta"]!["interval_base"]!.GetValue<string>(),
                ["bullets"] = new JsonArray { "6hベースでTOTAL3・OTHERS.Dの変化率を監視", "セクター選別/回帰を LTF で検証" },
                ["scores"] = new JsonObject { ["ai_flow_certainty"] = obj["ai_flow_interpretation"]!["certainty_score"]!.GetValue<double>() }
            }
        };

        var outPath = Path.Combine(PathUtil.OutputMacroDir, "latest_snapshot.json");
        JsonUtil.Write(outPath, snapshot);
        Console.WriteLine($"macro.snapshot saved → {outPath}");
        return 0;
    }
}
