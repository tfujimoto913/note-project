#!/usr/bin/env python3
import argparse, json, os
from datetime import datetime, timezone, timedelta

JST = timezone(timedelta(hours=9))

parser = argparse.ArgumentParser()
parser.add_argument("--interval", default="6h")
parser.add_argument("--out", required=True)
args = parser.parse_args()

out_path = args.out
os.makedirs(os.path.dirname(out_path), exist_ok=True)

# ダミー値生成（直近ファイルがあれば微変化）
macro_dir = os.path.join(os.path.dirname(os.path.dirname(out_path)), "macro")
cands = []
if os.path.isdir(macro_dir):
    for d in os.listdir(macro_dir):
        dp = os.path.join(macro_dir, d)
        if os.path.isdir(dp):
            for f in os.listdir(dp):
                if f.endswith(".json"):
                    cands.append(os.path.join(dp, f))
latest = sorted(cands)[-1] if cands else None

def tweak(v, step): return float(round(v * (1 + step), 6))

if latest:
    prev = json.load(open(latest))
    idx = prev["indices"]
    data = {
        "meta": {"ts": datetime.now(JST).isoformat(), "interval": args.interval, "sources": ["Dummy-API"], "version": "macro.collect v1.0"},
        "indices": {
            "BTC.D":   tweak(idx.get("BTC.D", 48.0), -0.002),
            "USDT.D":  tweak(idx.get("USDT.D", 6.8), -0.001),
            "ETHBTC":  tweak(idx.get("ETHBTC", 0.052), 0.000),
            "TOTAL2":  tweak(idx.get("TOTAL2", 1.12e12), 0.004),
            "TOTAL3":  tweak(idx.get("TOTAL3", 5.35e11), 0.006),
            "OTHERS.D":tweak(idx.get("OTHERS.D", 18.5), 0.004)
        },
        "sectors_cap": {
            "AI": tweak(1.90e11, 0.006), "L2": tweak(1.14e11, 0.003),
            "RWA": tweak(8.6e10, 0.002), "Meme": tweak(7.0e10, 0.008), "DePIN": tweak(4.2e10, 0.004)
        },
        "etf_flows": {"BTC_spot": 120_000_000, "ETH_spot": 35_000_000}, "notes": "OK"
    }
else:
    data = {
        "meta": {"ts": datetime.now(JST).isoformat(), "interval": args.interval, "sources": ["Dummy-API"], "version": "macro.collect v1.0"},
        "indices": {"BTC.D": 48.2, "USDT.D": 6.9, "ETHBTC": 0.0521, "TOTAL2": 1.13e12, "TOTAL3": 5.40e11, "OTHERS.D": 18.6},
        "sectors_cap": {"AI": 1.92e11, "L2": 1.15e11, "RWA": 8.7e10, "Meme": 7.1e10, "DePIN": 4.3e10},
        "etf_flows": {"BTC_spot": 120_000_000, "ETH_spot": 35_000_000}, "notes": "OK"
    }

with open(out_path, "w") as wf:
    json.dump(data, wf, indent=2)
print(f"saved: {out_path}")
