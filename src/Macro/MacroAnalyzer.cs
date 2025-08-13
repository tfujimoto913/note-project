using NoteProject.Shared;
using System.Text.Json.Nodes;

namespace NoteProject.Macro;

public static class MacroAnalyzer
{
    public static int Run(string[] args)
    {
        var files = Directory.Exists(PathUtil.InputMacroDir)
            ? Directory.GetFiles(PathUtil.InputMacroDir, "*.json", SearchOption.AllDirectories).OrderBy(f => f).ToList()
            : new List<string>();
        if (files.Count < 2) { Console.Error.WriteLine("Not enough macro input files. Run macro.collect a few times first."); return 2; }

        var latest = JsonNode.Parse(File.ReadAllText(files[^1]))!.AsObject();
        var prev   = JsonNode.Parse(File.ReadAllText(files[^2]))!.AsObject();

        double Ch(string key){ double v1 = latest["indices"]![key]!.GetValue<double>(); double v0 = prev["indices"]![key]!.GetValue<double>(); return Math.Abs(v0)<1e-9 ? 0 : (v1-v0)/v0; }

        var ts = DateTimeOffset.Parse(latest["meta"]!["ts"]!.GetValue<string>());
        var outObj = new JsonObject
        {
            ["meta"] = new JsonObject { ["ts"] = ts.ToString("O"), ["version"] = "macro.analyze v1.0", ["interval_base"] = "6h" },
            ["momentum"] = new JsonObject
            {
                ["BTC.D"]    = new JsonObject { ["ch_recent"] = Ch("BTC.D"),    ["bias"] = Bias(Ch("BTC.D"), inverse:true) },
                ["TOTAL3"]   = new JsonObject { ["ch_recent"] = Ch("TOTAL3"),   ["bias"] = Bias(Ch("TOTAL3")) },
                ["OTHERS.D"] = new JsonObject { ["ch_recent"] = Ch("OTHERS.D"), ["bias"] = Bias(Ch("OTHERS.D")) }
            },
            ["ai_flow_interpretation"] = new JsonObject
            {
                ["structure"] = Ch("BTC.D") < 0 && Ch("TOTAL3") > 0 ? "sector_selection" : "btc_dominant",
                ["total3_projection_bias"] = Ch("TOTAL3") > 0 ? "up" : (Ch("TOTAL3") < 0 ? "down" : "flat"),
                ["others_d_pressure_score"] = Math.Clamp(0.5 + 3 * Ch("OTHERS.D"), 0, 1),
                ["certainty_score"] = Math.Clamp(0.5 + 2 * (Ch("TOTAL3") - Math.Abs(Ch("BTC.D"))/2), 0, 1)
            }
        };

        var outJson = Path.Combine(PathUtil.OutputMacroDir, $"macro_analysis_{ts:yyyy-MM-dd_HH-mm}.json");
        JsonUtil.Write(outJson, outObj);

        // TSV 追記
        var tsv = Path.Combine(PathUtil.OutputMacroDir, $"macro_analysis_{ts:yyyy-MM}.tsv");
        bool exists = File.Exists(tsv);
        using (var sw = new StreamWriter(tsv, append:true))
        {
            if (!exists) sw.WriteLine("Date\tTime\tInterval\tBTC.D\tUSDT.D\tETHBTC\tTOTAL2\tTOTAL3\tOTHERS.D\tBTC.D_ch\tTOTAL3_ch\tOTHERS.D_ch\ttotal3_projection_bias\tothers_d_pressure_score\tai_certainty");
            sw.WriteLine(string.Join('\t', new[]{
                ts.ToString("yyyy-MM-dd"),
                ts.ToString("HH:mm"),
                latest["meta"]!["interval"]!.GetValue<string>(),
                latest["indices"]!["BTC.D"]!.GetValue<double>().ToString("0.####"),
                latest["indices"]!["USDT.D"]!.GetValue<double>().ToString("0.####"),
                latest["indices"]!["ETHBTC"]!.GetValue<double>().ToString("0.######"),
                latest["indices"]!["TOTAL2"]!.GetValue<double>().ToString("0.####e+0"),
                latest["indices"]!["TOTAL3"]!.GetValue<double>().ToString("0.####e+0"),
                latest["indices"]!["OTHERS.D"]!.GetValue<double>().ToString("0.####"),
                outObj["momentum"]!["BTC.D"]!["ch_recent"]!.GetValue<double>().ToString("0.####"),
                outObj["momentum"]!["TOTAL3"]!["ch_recent"]!.GetValue<double>().ToString("0.####"),
                outObj["momentum"]!["OTHERS.D"]!["ch_recent"]!.GetValue<double>().ToString("0.####"),
                outObj["ai_flow_interpretation"]!["total3_projection_bias"]!.GetValue<string>(),
                outObj["ai_flow_interpretation"]!["others_d_pressure_score"]!.GetValue<double>().ToString("0.##"),
                outObj["ai_flow_interpretation"]!["certainty_score"]!.GetValue<double>().ToString("0.##")
            }));
        }

        Log($"macro.analyze saved → {outJson} & appended → {tsv}");
        return 0;
    }

    static string Bias(double x, bool inverse=false){ if (inverse) x = -x; if (x>0.01) return "up"; if (x<-0.01) return "down"; return "flat"; }
    static void Log(string msg){ Directory.CreateDirectory(PathUtil.LogsDir); File.AppendAllText(Path.Combine(PathUtil.LogsDir,"analysis_log.json"), $"{DateTimeOffset.Now:O}\tmacro.analyze\t{msg}\n"); }
}
