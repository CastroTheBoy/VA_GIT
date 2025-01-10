public sealed class UIManager
{
    private Dictionary<string, Dictionary<string, int>> _elementStringReference;
    private Dictionary<string, List<UIElement>> _elementListReference;
    private static UIManager? _instance = null;

    private UIManager()
    {
        _elementStringReference = new Dictionary<string, Dictionary<string, int>> ();
        _elementListReference = new Dictionary<string, List<UIElement>> ();
    }

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIManager();
            }
            return _instance;
        }
    }

    public void CreateGroup(string name)
    {
        _elementListReference.Add(name, new List<UIElement>());
        _elementStringReference.Add(name, new Dictionary<string, int>());
    }

    public UIElement GetElement(string groupName, string elementName)
    {
        return _elementListReference[groupName][_elementStringReference[groupName][elementName]];
    }

    public void AddElement(string groupName, string elementName, UIElement element)
    {
        _elementListReference[groupName].Add(element);
        _elementStringReference[groupName].Add(
            elementName,
            _elementListReference[groupName].IndexOf(element));
    }

    public void RemoveElement(string groupName, string elementName)
    {
        _elementListReference[groupName].RemoveAt(_elementStringReference[groupName][elementName]);
        _elementStringReference[groupName].Remove(elementName);
    }

    public void DrawGroup(string groupName)
    {
        foreach (UIElement element in _elementListReference[groupName])
            element.Draw();
    }
}

public abstract class UIElement
{
    public static int ConsoleHeight { get => Console.WindowHeight; }
    public static int ConsoleWidth { get => Console.WindowWidth; }

    public short Left { get; }
    public short Right { get; }
    public short Top { get; }
    public short Bottom { get; }
    private protected ConsoleBuffer _buffer;

    public UIElement(short left, short top, short right, short bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
        _buffer = new ConsoleBuffer(left, top, right, bottom);
    }

    public void Draw() => _buffer.Write();

    public void Clear(bool draw = false)
    {
        for (int i = 0; i < _buffer.Buffer.Length; i++)
        {
            _buffer.Add(i, ConsoleBuffer.CombineAttribute(), (char)0);
        }

        if (draw)
            Draw();
    }
}

public sealed class BorderElement : UIElement
{
    public BorderElement(short left, short top, short right, short bottom) : base(left, top, right, bottom) { }

    public void BuildBorder(Border border)
    {
        int bufferIndex = -1;
        for (int height = Top; height < Bottom; height++)
            for (int width = Left; width < Right; width++)
            {
                bufferIndex++;
                if (width == Left && border.HasFlag(Border.Left))
                {
                    _buffer.Add(bufferIndex, ConsoleBuffer.CombineAttribute(), '|');
                    continue;
                }
                if (width == Right - 1 && border.HasFlag(Border.Right))
                {
                    _buffer.Add(bufferIndex, ConsoleBuffer.CombineAttribute(), '|');
                    continue;
                }
                if (height == Top && border.HasFlag(Border.Top))
                {
                    _buffer.Add(bufferIndex, ConsoleBuffer.CombineAttribute(), '-');
                    continue;
                }
                if (height == Bottom - 1 && border.HasFlag(Border.Bottom))
                {
                    _buffer.Add(bufferIndex, ConsoleBuffer.CombineAttribute(), '-');
                    continue;
                }
            }
    }
}

[Flags]
public enum Border : byte
{
    Left = 0b0001,
    Right = 0b0010,
    Top = 0b0100,
    Bottom = 0b1000
}

public sealed class TextEntry
{
    public string Text { get; }
    public ushort Color
    {
        get
        {
            if (IsHighlighted)
                return _highlightedColor;
            return _defaultColor;
        }
    }
    public bool IsHighlighted { get; set; }
    private ushort _defaultColor;
    private ushort _highlightedColor;

    public TextEntry(string text) : this(text, false) { }

    public TextEntry(string text, bool isHighlighted) : this(text, ConsoleBuffer.CombineAttribute(), ConsoleBuffer.CombineAttribute(background: BufferColor.Yellow), isHighlighted) { }

    public TextEntry(string text, ushort defaultColor, ushort highlighColor, bool isHighlighted = false)
    {
        Text = text;
        _defaultColor = defaultColor;
        _highlightedColor = highlighColor;
        IsHighlighted = isHighlighted;
    }
}

public abstract class WriterElement : UIElement
{
    public WriterElement(short left, short top, short right, short bottom) : base(left, top, right, bottom) { }

    public void Write(bool draw = false, params TextEntry[] items)
    {
        Clear();
        int bufferIndex = 0;
        foreach (TextEntry item in items)
        {
            for (int i = 0; i < item.Text.Length; i++)
            {
                _buffer.Add(bufferIndex, item.Color, item.Text[i]);
                bufferIndex++;
            }

            // fill line
            int curretLineIndex = bufferIndex % (Right - Left);
            if (curretLineIndex == 0)
                curretLineIndex = (Right - Left);
            int currentLineRemainingChars = (Right - Left) - curretLineIndex;
            int bufferCurrentLineEndIndex = bufferIndex + currentLineRemainingChars;

            for (; bufferIndex < bufferCurrentLineEndIndex; bufferIndex++)
            {
                _buffer.Add(bufferIndex, ConsoleBuffer.CombineAttribute(), (char)0);
            }
        }
        if (draw)
            this.Draw();
    }
}

public interface IListWriter
{
    public int Count { get; }
    public void WriteList(bool draw = false);
    public TextEntry GetElement(int index);
}

public class NamedListWriterElement : WriterElement, IListWriter
{
    private Dictionary<string, int> _messageDict;
    private List<TextEntry> _messageList;
    public int Count { get => _messageList.Count; }

    public NamedListWriterElement(short left, short top, short right, short bottom) : base(left, top, right, bottom) 
    {
        _messageDict = new Dictionary<string, int>();
        _messageList = new List<TextEntry>();
    }

    public void WriteList(bool draw = false)
    {
        Write(draw, _messageList.ToArray());
    }

    public TextEntry GetElement(string name)
    {
        return _messageList[_messageDict[name]];
    }
    public TextEntry GetElement(int index)
    {
        return _messageList[index];
    }

    public void AddEntry(string name, TextEntry entry)
    {
        _messageList.Add(entry);
        _messageDict.Add(name, _messageList.IndexOf(entry));
    }

    public void RemoveEntry(string name)
    {
        _messageList.RemoveAt(_messageDict[name]);
        _messageDict.Remove(name);
    }
}

public class ListWriterElement : WriterElement, IListWriter
{
    private List<TextEntry> _messageList;
    public int Count { get => _messageList.Count; }

    public ListWriterElement(short left, short top, short right, short bottom) : base(left, top, right, bottom) 
    {
        _messageList = new List<TextEntry>();
    }

    public void WriteList(bool draw = false)
    {
        Write(draw, _messageList.ToArray());
    }

    public TextEntry GetElement(int index)
    {
        return _messageList[index];
    }

    public void AddEntry(TextEntry entry)
    {
        _messageList.Add(entry);
    }

    public void SetList(List<TextEntry> entries, bool write = false)
    {
        _messageList = entries;
        if (write)
            WriteList();
    }

    public void RemoveEntry(int index)
    {
        _messageList.RemoveAt(index);
    }
}

public sealed class LogWriterElement : WriterElement
{
    public List<TextEntry> _messages = new List<TextEntry>();

    public LogWriterElement(short left, short top, short right, short bottom) : base(left, top, right, bottom) { }

    public void WriteLog(bool draw = false)
    {
        Write(draw, _messages.ToArray()[GetMinMessageIndex()..]);
    }

    public void AddMessage(string text, bool draw = false)
    {
        _messages.Add(new TextEntry(text));
        WriteLog(draw);
    }

    public void AddMessage(TextEntry entry, bool draw = false)
    {
        _messages.Add(entry);
        WriteLog(draw);
    }

    public int GetMinMessageIndex()
    {
        int lineCharCount = (Right - Left);
        int runningTotal = 0;
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            runningTotal += (int)Math.Ceiling(((double)_messages[i].Text.Length / (double)lineCharCount)) * lineCharCount;
            if (runningTotal > _buffer.Buffer.Length)
                return i + 1;
        }
        return 0;
    }
}