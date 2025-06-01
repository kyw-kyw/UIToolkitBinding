using System.Text;

namespace UIToolkitBinding.CodeEmitters;

internal abstract class CodeEmitter
{
    static readonly int defaultBufferSize = 1024;
    static readonly int defaultTempBufferSize = 512;
    static StringBuilder? cacheBuffer;
    static StringBuilder? cacheTempBuffer;

    protected static StringBuilder Buffer
    {
        get
        {
            cacheBuffer ??= new StringBuilder(defaultBufferSize);
            return cacheBuffer;
        }
    }
    protected static StringBuilder TempBuffer
    {
        get
        {
            cacheTempBuffer ??= new StringBuilder(defaultTempBufferSize);
            return cacheTempBuffer;
        }
    }

    public static void Clear()
    {
        Buffer.Clear();
        TempBuffer.Clear();

        cacheBuffer = null;
        cacheTempBuffer = null;
    }
}
