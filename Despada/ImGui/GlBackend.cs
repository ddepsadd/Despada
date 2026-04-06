using System.Runtime.InteropServices;

internal static unsafe class GlBackend
{
    private const string LibGL = "libGL.so.1";
    [DllImport(LibGL)] private static extern uint glCreateProgram();
    [DllImport(LibGL)] private static extern uint glCreateShader(uint type);
    [DllImport(LibGL)] private static extern void glShaderSource(uint shader, int count, string[] str, int[]? length);
    [DllImport(LibGL)] private static extern void glCompileShader(uint shader);
    [DllImport(LibGL)] private static extern void glAttachShader(uint program, uint shader);
    [DllImport(LibGL)] private static extern void glLinkProgram(uint program);
    [DllImport(LibGL)] private static extern void glDeleteShader(uint shader);
    [DllImport(LibGL)] private static extern void glUseProgram(uint program);
    [DllImport(LibGL)] private static extern int  glGetUniformLocation(uint program, string name);
    [DllImport(LibGL)] private static extern int  glGetAttribLocation(uint program, string name);
    [DllImport(LibGL)] private static extern void glUniform1i(int location, int v0);
    [DllImport(LibGL)] private static extern void glUniformMatrix4fv(int location, int count, bool transpose, float[] value);
    [DllImport(LibGL)] private static extern void glGenVertexArrays(int n, out uint arrays);
    [DllImport(LibGL)] private static extern void glBindVertexArray(uint array);
    [DllImport(LibGL)] private static extern void glGenBuffers(int n, out uint buffers);
    [DllImport(LibGL)] private static extern void glBindBuffer(uint target, uint buffer);
    [DllImport(LibGL)] private static extern void glBufferData(uint target, nint size, nint data, uint usage);
    [DllImport(LibGL)] private static extern void glEnableVertexAttribArray(uint index);
    [DllImport(LibGL)] private static extern void glVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride, nint pointer);
    [DllImport(LibGL)] private static extern void glGenTextures(int n, out uint textures);
    [DllImport(LibGL)] private static extern void glBindTexture(uint target, uint texture);
    [DllImport(LibGL)] private static extern void glTexParameteri(uint target, uint pname, int param);
    [DllImport(LibGL)] private static extern void glTexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, nint pixels);
    [DllImport(LibGL)] private static extern void glActiveTexture(uint texture);
    [DllImport(LibGL)] private static extern void glScissor(int x, int y, int width, int height);
    [DllImport(LibGL)] private static extern void glDrawElementsBaseVertex(uint mode, int count, uint type, nint indices, int baseVertex);
    public static uint CreateShader(string vertSrc, string fragSrc)
    {
        const uint GL_VERTEX_SHADER   = 0x8B31;
        const uint GL_FRAGMENT_SHADER = 0x8B30;

        var vert = glCreateShader(GL_VERTEX_SHADER);
        glShaderSource(vert, 1, [vertSrc], null);
        glCompileShader(vert);

        var frag = glCreateShader(GL_FRAGMENT_SHADER);
        glShaderSource(frag, 1, [fragSrc], null);
        glCompileShader(frag);

        var prog = glCreateProgram();
        glAttachShader(prog, vert);
        glAttachShader(prog, frag);
        glLinkProgram(prog);
        glDeleteShader(vert);
        glDeleteShader(frag);
        return prog;
    }

    public static int  GetUniformLocation(uint prog, string name) => glGetUniformLocation(prog, name);
    public static int  GetAttribLocation(uint prog, string name)  => glGetAttribLocation(prog, name);
    public static void UseProgram(uint prog)                       => glUseProgram(prog);
    public static void Uniform1i(int loc, int v)                  => glUniform1i(loc, v);
    public static void UniformMatrix4fv(int loc, float[] m)       => glUniformMatrix4fv(loc, 1, false, m);

    public static uint GenVertexArray() { glGenVertexArrays(1, out var v); return v; }
    public static uint GenBuffer()      { glGenBuffers(1, out var v);      return v; }
    public static uint GenTexture()     { glGenTextures(1, out var v);     return v; }

    public static void BindVertexArray(uint v)             => glBindVertexArray(v);
    public static void BindBuffer(uint target, uint buf)   => glBindBuffer(target, buf);
    public static void BindTexture(uint target, uint tex)  => glBindTexture(target, tex);
    public static void ActiveTexture(uint slot)            => glActiveTexture(slot);
    public static void TexParameteri(uint tgt, uint pname, int param) => glTexParameteri(tgt, pname, param);
    public static void EnableVertexAttribArray(uint idx)   => glEnableVertexAttribArray(idx);
    public static void Scissor(int x, int y, int w, int h) => glScissor(x, y, w, h);

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

    public static void DrawElementsBaseVertex(uint mode, int count, uint type, nint indices, int baseVertex)
        => glDrawElementsBaseVertex(mode, count, type, indices, baseVertex);
}