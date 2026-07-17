using System.Runtime.InteropServices;

namespace Despada.ImGui.Hook;

internal static class GlStateGuard
{
    // Entry points are resolved via GlLoader (wglGetProcAddress on Windows, GL library exports
    // elsewhere) instead of DllImport-by-name, which only worked on Linux. Signatures/marshalling
    // match the previous per-platform imports 1:1.
    private delegate void D_GetIntegerv(uint pname, out int data);
    private delegate void D_GetIntegervArr(uint pname, int[] data);
    private delegate void D_BindVertexArray(uint array);
    private delegate void D_UseProgram(uint program);
    private delegate void D_Enable(uint cap);
    private delegate void D_Disable(uint cap);
    [return: MarshalAs(UnmanagedType.U1)] private delegate bool D_IsEnabled(uint cap);
    private delegate void D_BlendFuncSeparate(uint sfRGB, uint dfRGB, uint sfA, uint dfA);
    private delegate void D_BlendEquationSeparate(uint modeRGB, uint modeA);
    private delegate void D_ActiveTexture(uint texture);
    private delegate void D_PolygonMode(uint face, uint mode);

    private static D_GetIntegerv            glGetIntegerv        = null!;
    private static D_GetIntegervArr         glGetIntegervArr     = null!;
    private static D_BindVertexArray        glBindVertexArray    = null!;
    private static D_UseProgram             glUseProgram         = null!;
    private static D_Enable                 glEnable             = null!;
    private static D_Disable                glDisable            = null!;
    private static D_IsEnabled              glIsEnabled          = null!;
    private static D_BlendFuncSeparate      glBlendFuncSeparate  = null!;
    private static D_BlendEquationSeparate  glBlendEquationSeparate = null!;
    private static D_ActiveTexture          glActiveTexture      = null!;
    private static D_PolygonMode            glPolygonMode        = null!;

    private static bool _loaded;

    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        glGetIntegerv           = GlLoader.Load<D_GetIntegerv>("glGetIntegerv");
        glGetIntegervArr        = GlLoader.Load<D_GetIntegervArr>("glGetIntegerv");
        glBindVertexArray       = GlLoader.Load<D_BindVertexArray>("glBindVertexArray");
        glUseProgram            = GlLoader.Load<D_UseProgram>("glUseProgram");
        glEnable                = GlLoader.Load<D_Enable>("glEnable");
        glDisable               = GlLoader.Load<D_Disable>("glDisable");
        glIsEnabled             = GlLoader.Load<D_IsEnabled>("glIsEnabled");
        glBlendFuncSeparate     = GlLoader.Load<D_BlendFuncSeparate>("glBlendFuncSeparate");
        glBlendEquationSeparate = GlLoader.Load<D_BlendEquationSeparate>("glBlendEquationSeparate");
        glActiveTexture         = GlLoader.Load<D_ActiveTexture>("glActiveTexture");
        glPolygonMode           = GlLoader.Load<D_PolygonMode>("glPolygonMode");
    }

    private const uint GL_BLEND                        = 0x0BE2;
    private const uint GL_SCISSOR_TEST                 = 0x0C11;
    private const uint GL_DEPTH_TEST                   = 0x0B71;
    private const uint GL_CULL_FACE                    = 0x0B44;
    private const uint GL_STENCIL_TEST                 = 0x0B90;
    private const uint GL_FRAMEBUFFER_SRGB             = 0x8DB9;
    private const uint GL_VERTEX_ARRAY_BINDING         = 0x85B5;
    private const uint GL_CURRENT_PROGRAM              = 0x8B8D;
    private const uint GL_BLEND_SRC_RGB                = 0x80C9;
    private const uint GL_BLEND_DST_RGB                = 0x80CA;
    private const uint GL_BLEND_SRC_ALPHA              = 0x80CB;
    private const uint GL_BLEND_DST_ALPHA              = 0x80CC;
    private const uint GL_BLEND_EQUATION_RGB           = 0x8009;
    private const uint GL_BLEND_EQUATION_ALPHA         = 0x883D;
    private const uint GL_ACTIVE_TEXTURE               = 0x84E0;
    private const uint GL_POLYGON_MODE                 = 0x0B40;
    private const uint GL_FRONT_AND_BACK               = 0x0408;
    private const uint GL_VIEWPORT                     = 0x0BA2;

    public static (int width, int height) GetFramebufferSize()
    {
        EnsureLoaded();
        var vp = new int[4];
        glGetIntegervArr(GL_VIEWPORT, vp);
        return (vp[2], vp[3]);
    }

    private struct GlSnapshot
    {
        public int  LastProgram, LastVAO;
        public int  LastActiveTexture;
        public int  LastBlendSrcRGB, LastBlendDstRGB, LastBlendSrcAlpha, LastBlendDstAlpha;
        public int  LastBlendEqRGB, LastBlendEqAlpha, LastPolygonMode;
        public bool BlendEnabled, ScissorEnabled, DepthEnabled, CullEnabled, StencilEnabled;
        public bool SrgbEnabled;
    }

    [ThreadStatic] private static GlSnapshot _snapshot;

    public static void Save()
    {
        EnsureLoaded();

        glGetIntegerv(GL_CURRENT_PROGRAM,      out _snapshot.LastProgram);
        glGetIntegerv(GL_VERTEX_ARRAY_BINDING, out _snapshot.LastVAO);
        glGetIntegerv(GL_ACTIVE_TEXTURE,       out _snapshot.LastActiveTexture);
        glGetIntegerv(GL_BLEND_SRC_RGB,        out _snapshot.LastBlendSrcRGB);
        glGetIntegerv(GL_BLEND_DST_RGB,        out _snapshot.LastBlendDstRGB);
        glGetIntegerv(GL_BLEND_SRC_ALPHA,      out _snapshot.LastBlendSrcAlpha);
        glGetIntegerv(GL_BLEND_DST_ALPHA,      out _snapshot.LastBlendDstAlpha);
        glGetIntegerv(GL_BLEND_EQUATION_RGB,   out _snapshot.LastBlendEqRGB);
        glGetIntegerv(GL_BLEND_EQUATION_ALPHA, out _snapshot.LastBlendEqAlpha);

        // GL_POLYGON_MODE returns TWO ints (front, back). Read both; front is what we restore.
        var polyMode = new int[2];
        glGetIntegervArr(GL_POLYGON_MODE, polyMode);
        _snapshot.LastPolygonMode = polyMode[0];

        _snapshot.BlendEnabled   = glIsEnabled(GL_BLEND);
        _snapshot.ScissorEnabled = glIsEnabled(GL_SCISSOR_TEST);
        _snapshot.DepthEnabled   = glIsEnabled(GL_DEPTH_TEST);
        _snapshot.CullEnabled    = glIsEnabled(GL_CULL_FACE);
        _snapshot.StencilEnabled = glIsEnabled(GL_STENCIL_TEST);
        _snapshot.SrgbEnabled    = glIsEnabled(GL_FRAMEBUFFER_SRGB);
        if (_snapshot.SrgbEnabled)
            glDisable(GL_FRAMEBUFFER_SRGB);
    }

    public static void Restore()
    {
        EnsureLoaded();

        glUseProgram((uint)_snapshot.LastProgram);
        glBindVertexArray((uint)_snapshot.LastVAO);
        glBlendFuncSeparate(
            (uint)_snapshot.LastBlendSrcRGB, (uint)_snapshot.LastBlendDstRGB,
            (uint)_snapshot.LastBlendSrcAlpha, (uint)_snapshot.LastBlendDstAlpha);
        glBlendEquationSeparate((uint)_snapshot.LastBlendEqRGB, (uint)_snapshot.LastBlendEqAlpha);
        glPolygonMode(GL_FRONT_AND_BACK, (uint)_snapshot.LastPolygonMode);

        RestoreCap(GL_BLEND,            _snapshot.BlendEnabled);
        RestoreCap(GL_SCISSOR_TEST,     _snapshot.ScissorEnabled);
        RestoreCap(GL_DEPTH_TEST,       _snapshot.DepthEnabled);
        RestoreCap(GL_CULL_FACE,        _snapshot.CullEnabled);
        RestoreCap(GL_STENCIL_TEST,     _snapshot.StencilEnabled);
        RestoreCap(GL_FRAMEBUFFER_SRGB, _snapshot.SrgbEnabled);

        glActiveTexture((uint)_snapshot.LastActiveTexture);
    }

    private static void RestoreCap(uint cap, bool wasEnabled)
    {
        if (wasEnabled) glEnable(cap); else glDisable(cap);
    }
}
