#!/usr/bin/env python3
from pathlib import Path

HOLDER = "2026 ddepsadd <https://github.com/ddepsadd>"
LICENSE = "AGPL-3.0-or-later"
BOM = "\ufeff"

HEADER = (
    f"// SPDX-FileCopyrightText: {HOLDER}\n"
    "//\n"
    f"// SPDX-License-Identifier: {LICENSE}\n"
    "\n"
)

root = Path("Despada")
changed = skipped = 0

for path in sorted(root.rglob("*.cs")):
    if any(part in {"bin", "obj"} for part in path.parts):
        continue

    raw = path.read_bytes()
    had_bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig")

    if "SPDX-License-Identifier" in text:
        skipped += 1
        continue

    path.write_text((BOM if had_bom else "") + HEADER + text, encoding="utf-8")
    changed += 1
    print(f"+ {path}")

print(f"\nдобавлено: {changed}, уже размечено: {skipped}")