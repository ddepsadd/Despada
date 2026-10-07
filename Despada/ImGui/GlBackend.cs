// SPDX-FileCopyrightText: 2026 ddepsadd <https://github.com/ddepsadd>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.InteropServices;

namespace Despada.ImGui;

internal static unsafe class GlBackend
{
    private delegate uint D_CreateProgram();
    private delegate uint D_CreateShader(uint type);
    private delegate void D_ShaderSource(uint shader, int count, string[] str, int[]? length);
    private delegate void D_CompileShader(uint shader);
    private delegate void D_AttachShader(uint program, uint shader);
    private delegate void D_LinkProgram(uint program);
    private delegate void D_DeleteShader(uint shader);
    private delegate void D_UseProgram(uint program);
    private delegate int  D_GetUniformLocation(uint program, string name);
    private delegate int  D_GetAttribLocation(uint program, string name);
    private delegate void D_Uniform1i(int location, int v0);
    private delegate void D_UniformMatrix4fv(int location, int count, bool transpose, float[] value);
    private delegate void D_GenVertexArrays(int n, out uint arrays);
    private delegate void D_BindVertexArray(uint array);
    private delegate void D_GenBuffers(int n, out uint buffers);
    private delegate void D_BindBuffer(uint target, uint buffer);
    private delegate void D_BufferData(uint target, nint size, nint data, uint usage);
    private delegate void D_EnableVertexAttribArray(uint index);
    private delegate void D_VertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, nint pointer);
    private delegate void D_GenTextures(int n, out uint textures);
    private delegate void D_DeleteTextures(int n, ref uint textures);
    private delegate void D_BindTexture(uint target, uint texture);
    private delegate void D_TexParameteri(uint target, uint pname, int param);
    private delegate void D_TexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, nint pixels);
    private delegate void D_ActiveTexture(uint texture);
    private delegate void D_Scissor(int x, int y, int width, int height);
    private delegate void D_DrawElementsBaseVertex(uint mode, int count, uint type, nint indices, int baseVertex);
    private delegate void D_Enable(uint cap);
    private delegate void D_DrawElements(uint mode, int count, uint type, nint indices);
    private delegate byte* D_GetString(uint name);
    private delegate void D_GetShaderiv(uint shader, uint pname, out int param);
    private delegate void D_GetProgramiv(uint program, uint pname, out int param);
    private delegate void D_GetInfoLog(uint obj, int bufSize, out int length, byte* infoLog);

    private static D_CreateProgram          glCreateProgram          = null!;
    private static D_CreateShader           glCreateShader           = null!;
    private static D_ShaderSource           glShaderSource           = null!;
    private static D_CompileShader          glCompileShader          = null!;
    private static D_AttachShader           glAttachShader           = null!;
    private static D_LinkProgram            glLinkProgram            = null!;
    private static D_DeleteShader           glDeleteShader           = null!;
    private static D_UseProgram             glUseProgram             = null!;
    private static D_GetUniformLocation     glGetUniformLocation     = null!;
    private static D_GetAttribLocation      glGetAttribLocation      = null!;
    private static D_Uniform1i              glUniform1i              = null!;
    private static D_UniformMatrix4fv       glUniformMatrix4fv       = null!;
    private static D_GenVertexArrays        glGenVertexArrays        = null!;
    private static D_BindVertexArray        glBindVertexArray        = null!;
    private static D_GenBuffers             glGenBuffers             = null!;
    private static D_BindBuffer             glBindBuffer             = null!;
    private static D_BufferData             glBufferData             = null!;
    private static D_EnableVertexAttribArray glEnableVertexAttribArray = null!;
    private static D_VertexAttribPointer    glVertexAttribPointer    = null!;
    private static D_GenTextures            glGenTextures            = null!;
    private static D_DeleteTextures         glDeleteTextures         = null!;
    private static D_BindTexture            glBindTexture            = null!;
    private static D_TexParameteri          glTexParameteri          = null!;
    private static D_TexImage2D             glTexImage2D             = null!;
    private static D_ActiveTexture          glActiveTexture          = null!;
    private static D_Scissor                glScissor                = null!;
    private static D_DrawElementsBaseVertex? glDrawElementsBaseVertex;
    private static D_DrawElements           glDrawElements           = null!;
    private static D_GetString              glGetString              = null!;
    private static D_Enable                 glEnable                 = null!;
    private static D_GetShaderiv            glGetShaderiv            = null!;
    private static D_GetProgramiv           glGetProgramiv           = null!;
    private static D_GetInfoLog             glGetShaderInfoLog       = null!;
    private static D_GetInfoLog             glGetProgramInfoLog      = null!;

    private static bool _loaded;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        glCreateProgram           = GlLoader.Load<D_CreateProgram>("glCreateProgram");
        glCreateShader            = GlLoader.Load<D_CreateShader>("glCreateShader");
        glShaderSource            = GlLoader.Load<D_ShaderSource>("glShaderSource");
        glCompileShader           = GlLoader.Load<D_CompileShader>("glCompileShader");
        glAttachShader            = GlLoader.Load<D_AttachShader>("glAttachShader");
        glLinkProgram             = GlLoader.Load<D_LinkProgram>("glLinkProgram");
        glDeleteShader            = GlLoader.Load<D_DeleteShader>("glDeleteShader");
        glUseProgram              = GlLoader.Load<D_UseProgram>("glUseProgram");
        glGetUniformLocation      = GlLoader.Load<D_GetUniformLocation>("glGetUniformLocation");
        glGetAttribLocation       = GlLoader.Load<D_GetAttribLocation>("glGetAttribLocation");
        glUniform1i               = GlLoader.Load<D_Uniform1i>("glUniform1i");
        glUniformMatrix4fv        = GlLoader.Load<D_UniformMatrix4fv>("glUniformMatrix4fv");
        glGenVertexArrays         = GlLoader.Load<D_GenVertexArrays>("glGenVertexArrays");
        glBindVertexArray         = GlLoader.Load<D_BindVertexArray>("glBindVertexArray");
        glGenBuffers              = GlLoader.Load<D_GenBuffers>("glGenBuffers");
        glBindBuffer              = GlLoader.Load<D_BindBuffer>("glBindBuffer");
        glBufferData              = GlLoader.Load<D_BufferData>("glBufferData");
        glEnableVertexAttribArray = GlLoader.Load<D_EnableVertexAttribArray>("glEnableVertexAttribArray");
        glVertexAttribPointer     = GlLoader.Load<D_VertexAttribPointer>("glVertexAttribPointer");
        glGenTextures             = GlLoader.Load<D_GenTextures>("glGenTextures");
        glDeleteTextures          = GlLoader.Load<D_DeleteTextures>("glDeleteTextures");
        glBindTexture             = GlLoader.Load<D_BindTexture>("glBindTexture");
        glTexParameteri           = GlLoader.Load<D_TexParameteri>("glTexParameteri");
        glTexImage2D              = GlLoader.Load<D_TexImage2D>("glTexImage2D");
        glActiveTexture           = GlLoader.Load<D_ActiveTexture>("glActiveTexture");
        glScissor                 = GlLoader.Load<D_Scissor>("glScissor");
        // Core only since GL 3.2 / GLES 3.2; Clyde may fall back to GL 3.1 or GLES 3.0.
        glDrawElementsBaseVertex  = GlLoader.TryLoad<D_DrawElementsBaseVertex>("glDrawElementsBaseVertex");
        glDrawElements            = GlLoader.Load<D_DrawElements>("glDrawElements");
        glGetString               = GlLoader.Load<D_GetString>("glGetString");
        glEnable                  = GlLoader.Load<D_Enable>("glEnable");
        glGetShaderiv             = GlLoader.Load<D_GetShaderiv>("glGetShaderiv");
        glGetProgramiv            = GlLoader.Load<D_GetProgramiv>("glGetProgramiv");
        glGetShaderInfoLog        = GlLoader.Load<D_GetInfoLog>("glGetShaderInfoLog");
        glGetProgramInfoLog       = GlLoader.Load<D_GetInfoLog>("glGetProgramInfoLog");
    }

    public static uint CreateShader(string vertSrc, string fragSrc)
    {
        const uint GL_VERTEX_SHADER   = 0x8B31;
        const uint GL_FRAGMENT_SHADER = 0x8B30;

        const uint GL_COMPILE_STATUS  = 0x8B81;
        const uint GL_LINK_STATUS     = 0x8B82;

        var vert = glCreateShader(GL_VERTEX_SHADER);
        glShaderSource(vert, 1, [vertSrc], null);
        glCompileShader(vert);
        glGetShaderiv(vert, GL_COMPILE_STATUS, out var vertOk);
        if (vertOk == 0)
            throw new InvalidOperationException($"Vertex shader compile failed: {InfoLog(glGetShaderInfoLog, vert)}");

        var frag = glCreateShader(GL_FRAGMENT_SHADER);
        glShaderSource(frag, 1, [fragSrc], null);
        glCompileShader(frag);
        glGetShaderiv(frag, GL_COMPILE_STATUS, out var fragOk);
        if (fragOk == 0)
            throw new InvalidOperationException($"Fragment shader compile failed: {InfoLog(glGetShaderInfoLog, frag)}");

        var prog = glCreateProgram();
        glAttachShader(prog, vert);
        glAttachShader(prog, frag);
        glLinkProgram(prog);
        glDeleteShader(vert);
        glDeleteShader(frag);
        glGetProgramiv(prog, GL_LINK_STATUS, out var linkOk);
        if (linkOk == 0)
            throw new InvalidOperationException($"Shader link failed: {InfoLog(glGetProgramInfoLog, prog)}");
        return prog;
    }

    private static string InfoLog(D_GetInfoLog getLog, uint obj)
    {
        var buf = stackalloc byte[1024];
        getLog(obj, 1024, out var len, buf);
        return System.Text.Encoding.UTF8.GetString(buf, Math.Clamp(len, 0, 1024)).Trim();
    }

    public static int  GetUniformLocation(uint prog, string name) => glGetUniformLocation(prog, name);
    public static int  GetAttribLocation(uint prog, string name)  => glGetAttribLocation(prog, name);
    public static void UseProgram(uint prog)                       => glUseProgram(prog);
    public static void Uniform1i(int loc, int v)                  => glUniform1i(loc, v);
    public static void UniformMatrix4fv(int loc, float[] m)       => glUniformMatrix4fv(loc, 1, false, m);

    public static uint GenVertexArray() { glGenVertexArrays(1, out var v); return v; }
    public static uint GenBuffer()      { glGenBuffers(1, out var v);      return v; }
    public static uint GenTexture()     { glGenTextures(1, out var v);     return v; }
    public static void DeleteTexture(uint tex) => glDeleteTextures(1, ref tex);

    public static void BindVertexArray(uint v)             => glBindVertexArray(v);
    public static void BindBuffer(uint target, uint buf)   => glBindBuffer(target, buf);
    public static void BindTexture(uint target, uint tex)  => glBindTexture(target, tex);
    public static void ActiveTexture(uint slot)            => glActiveTexture(slot);
    public static void TexParameteri(uint tgt, uint pname, int param) => glTexParameteri(tgt, pname, param);
    public static void EnableVertexAttribArray(uint idx)   => glEnableVertexAttribArray(idx);
    public static void Scissor(int x, int y, int w, int h) => glScissor(x, y, w, h);
    public static void Enable(uint cap)                    => glEnable(cap);

    public static void VertexAttribPointer(uint idx, int size, uint type, bool norm, int stride, int offset)
        => glVertexAttribPointer(idx, size, type, norm, stride, (nint)offset);

    public static void BufferData(uint target, int size, nint data, uint usage)
        => glBufferData(target, size, data, usage);

    public static void TexImage2D(int w, int h, nint pixels)
        => glTexImage2D(
            0x0DE1 /*GL_TEXTURE_2D*/, 0,
            0x1908 /*GL_RGBA*/,
            w, h, 0,
            0x1908 /*GL_RGBA*/,
            0x1401 /*GL_UNSIGNED_BYTE*/,
            pixels);

    public static bool HasBaseVertex => glDrawElementsBaseVertex is not null;

    public static void DrawElementsBaseVertex(uint mode, int count, uint type, nint indices, int baseVertex)
        => glDrawElementsBaseVertex!(mode, count, type, indices, baseVertex);

    public static void DrawElements(uint mode, int count, uint type, nint indices)
        => glDrawElements(mode, count, type, indices);

    public static string GetString(uint name)
    {
        var p = glGetString(name);
        return p is null ? "" : Marshal.PtrToStringUTF8((nint)p) ?? "";
    }

    // Header for our shaders, matching the context Clyde actually created:
    // macOS core profiles reject "#version 130", GLES needs the "es" variant and a precision.
    public static string ShaderHeader()
    {
        const uint GL_VERSION                  = 0x1F02;
        const uint GL_SHADING_LANGUAGE_VERSION = 0x8B8C;

        var glVersion = GetString(GL_VERSION);
        if (glVersion.StartsWith("OpenGL ES", StringComparison.Ordinal))
            return "#version 300 es\nprecision mediump float;\n";

        // "4.60 NVIDIA", "3.30 Mesa ...", "1.40" -> major*100 + minor
        var glsl = GetString(GL_SHADING_LANGUAGE_VERSION);
        var num  = glsl.Split(' ', 2)[0].Split('.');
        var ver  = num.Length >= 2 && int.TryParse(num[0], out var maj) && int.TryParse(num[1][..Math.Min(2, num[1].Length)], out var min)
            ? maj * 100 + min : 130;

        return ver >= 330 ? "#version 330 core\n"
             : ver >= 140 ? "#version 140\n"
             :              "#version 130\n";
    }
}
