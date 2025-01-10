using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal static class BufferWriter
{
    private static SafeFileHandle _handle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
    private static Coord _bufferStart = new Coord() { X = 0, Y = 0 };

    public static void WriteBuffer(CharInfo[] buffer, SmallRect rect, Coord buffSize)
    {
        WriteConsoleOutputW(
            _handle, 
            buffer,
            buffSize,
            _bufferStart,
            ref rect);
    }

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteConsoleOutputW(
      SafeFileHandle hConsoleOutput,
      CharInfo[] lpBuffer,
      Coord dwBufferSize,
      Coord dwBufferCoord,
      ref SmallRect lpWriteRegion);
}

internal sealed class ConsoleBuffer
{
    private SmallRect _rect;
    private Coord _buffSize;
    public CharInfo[] Buffer { get; }

    public ConsoleBuffer(short left, short top, short right, short bottom)
    {
        _rect = new SmallRect() { Left = left, Top = top, Right = right, Bottom = bottom };
        _buffSize = new Coord() { X = (short)(right - left), Y = (short)(bottom - top) };
        Buffer = new CharInfo[_buffSize.X * _buffSize.Y];
    }

    public void Write() => BufferWriter.WriteBuffer(Buffer, _rect, _buffSize);

    public void Add(int index, ushort attribute, char character)
    {
        Buffer[index].Attributes = attribute;
        Buffer[index].Char.UnicodeChar = character;
    }

    public static ushort CombineAttribute(BufferColor background = BufferColor.Black, BufferColor foreground = BufferColor.Green)
    {
        return (ushort)((ushort)((ushort)background << 4) | (ushort)foreground);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct Coord
{
    public short X;
    public short Y;

    public Coord(short X, short Y)
    {
        this.X = X;
        this.Y = Y;
    }
};

[StructLayout(LayoutKind.Explicit)]
internal struct CharUnion
{
    [FieldOffset(0)] public ushort UnicodeChar;
    [FieldOffset(0)] public byte AsciiChar;
}

[StructLayout(LayoutKind.Explicit)]
internal struct CharInfo
{
    [FieldOffset(0)] public CharUnion Char;
    [FieldOffset(2)] public ushort Attributes;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SmallRect
{
    public short Left;
    public short Top;
    public short Right;
    public short Bottom;
}

[Flags]
internal enum BufferColor : byte
{
    Black = 0b0000,
    DarkBlue = 0b0001,
    DarkGreen = 0b0010,
    DarkCyan = 0b0011,
    DarkRed = 0b0100,
    DarkMagenta = 0b0101,
    DarkYellow = 0b0110,
    Gray = 0b0111,
    DarkGray = 0b1000,
    Blue = 0b1001,
    Green = 0b1010,
    Cyan = 0b1011,
    Red = 0b1100,
    Magenta = 0b1101,
    Yellow = 0b1110,
    White = 0b1111
}