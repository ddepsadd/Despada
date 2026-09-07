# Third-party notices

Despada bundles or redistributes the following third-party components.
Despada itself is licensed under AGPL-3.0-or-later; the notices below apply
only to the components named.

Full license texts are in `licenses/`.

---

## ImGui.NET

Redistributed as `ImGui.NET.dll` alongside release binaries.
Upstream: https://github.com/ImGuiNET/ImGui.NET
License: MIT (`licenses/MIT.txt`)

    Copyright (c) 2017 Eric Mellino and ImGui.NET contributors

## Dear ImGui

Included in binary form via cimgui.
Upstream: https://github.com/ocornut/imgui
License: MIT (`licenses/MIT.txt`)

    Copyright (c) 2014-2026 Omar Cornut

## cimgui

Embedded into `Despada.dll` as `Native/cimgui.dll`, `Native/libcimgui.so`,
`Native/libcimgui.dylib`.
Upstream: https://github.com/cimgui/cimgui
License: MIT (`licenses/MIT.txt`)

    Copyright (c) 2015 Stephan Dilly

## Harmony (Lib.Harmony)

Referenced at build time; supplied at runtime by the loader.
Upstream: https://github.com/pardeike/Harmony
License: MIT (`licenses/MIT.txt`)

    Copyright (c) 2017 Andreas Pardeike

## JetBrains Mono Nerd Font

Embedded into `Despada.dll` as
`Assets/JetBrainsMonoNerdFontMono-Regular.ttf`.

Base typeface: JetBrains Mono — https://github.com/JetBrains/JetBrainsMono
Patched by Nerd Fonts — https://github.com/ryanoasis/nerd-fonts
License: SIL Open Font License 1.1 (`licenses/OFL-1.1.txt`)

    Copyright 2020 The JetBrains Mono Project Authors
    (https://github.com/JetBrains/JetBrainsMono)

    Copyright (c) 2014, Ryan L McIntyre (https://ryanlmcintyre.com)

SIL OFL 1.1 section 2 requires that both copyright notices above and the full
license text accompany the font in any redistribution, including when the font
is embedded in a binary.

The patched font also carries glyphs drawn from several upstream icon sets
under their own licenses. See the Nerd Fonts license audit:
https://github.com/ryanoasis/nerd-fonts/blob/-/license-audit.md