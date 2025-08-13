using NoteProject.Macro;
using NoteProject.Articles;
using NoteProject.Tools;
using NoteProject.Shared;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return 1;
        }

        // ディレクトリ確保（共通）
        Directory.CreateDirectory(PathUtil.InputMacroDir);
        Directory.CreateDirectory(PathUtil.OutputMacroDir);
        Directory.CreateDirectory(PathUtil.LogsDir);

        var cmd = args[0];
        var rest = args.Skip(1).ToArray();

        try
        {
            return cmd switch
            {
                // ===== マクロ系 =====
                "macro.collect"  => MacroCollector.Run(rest),
                "macro.analyze"  => MacroAnalyzer.Run(rest),
                "macro.snapshot" => MacroSnapshot.Run(rest),

                // ===== 記事系 =====
                "btc_free.analyze"  => BtcFreeAnalyzer.Run(rest),
                "btc_paid.analyze"  => BtcPaidAnalyzer.Run(rest),
                "eth_paid.analyze"  => EthPaidAnalyzer.Run(rest),
                "alts_paid.analyze" => AltsPaidAnalyzer.Run(rest),

                // ===== 個人用ツール系（任意）=====
                "flow.scan" => FlowScanner.Run(rest),
                "zone.scan" => ZoneScanner.Run(rest),

                // ===== 実行セット（任意）=====
                "runset.daily" => Runset.Daily(rest),

                _ => Unknown(cmd)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] {ex.Message}\n{ex}");
            return -1;
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine(@"
Usage: dotnet run -- [command] [options]

Commands:
  --- Macro ---
  macro.collect  --interval [6h|12h|1d]
  macro.analyze  --lookback [30d] --base [6h|12h|1d]
  macro.snapshot

  --- Articles ---
  btc_free.analyze
  btc_paid.analyze
  eth_paid.analyze
  alts_paid.analyze

  --- Tools ---
  flow.scan
  zone.scan

  --- Runsets ---
  runset.daily
");
    }

    static int Unknown(string cmd)
    {
        Console.Error.WriteLine($"Unknown command: {cmd}");
        ShowUsage();
        return 2;
    }
}