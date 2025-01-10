public static class UIManager
{
    static UIManager()
    {
        // ----------------------------------------------------------
        // Main game screen

        UIPane gameScreen = new UIPane();
        GameScreens.Add("Game", gameScreen);
        ActivePane = gameScreen;

        gameScreen.Elements.Add("OuterBorder", new UIElement(0, 0, 120, 30));
        gameScreen.Elements.Add("ActionBorder", new UIElement(0, 20, 81, 30));
        gameScreen.Elements.Add("LogBorder", new UIElement(80, 0, 120, 30));

        // Drawing outer, action and log block borders
        foreach (UIElement element in gameScreen.Elements.Values)
            UIMethods.BuildBorder(element);
        
        // Action block elements
        UIElement actionList = new UIElement(3, 22, 41, 29);
        gameScreen.Elements.Add("ActionList", actionList);
        ActionList = new UIListWriter(actionList);
        ActionList.SetList("Attack", "Kill", "Use item", "Equip gear", "Use gear");

        UIElement actionListHeader = new UIElement(3, 21, 41, 22);
        gameScreen.Elements.Add("ActionListHeader", actionListHeader);
        ActionListHeader = new UIListWriter(actionListHeader);
        ActionListHeader.SetList(new ConsoleCharacterString("Actions",ConsoleColor.DarkYellow));

        UIElement itemList = new UIElement(41, 22, 80, 29);
        gameScreen.Elements.Add("ItemList", itemList);
        ItemList = new UIListWriter(itemList);

        UIElement itemListHeader = new UIElement(41, 21, 80, 22);
        gameScreen.Elements.Add("ItemListHeader", itemListHeader);
        ItemListHeader = new UIListWriter(itemListHeader);

        // Party blocks
        UIElement leftParty = new UIElement(5, 3, 40, 19);
        LeftParty = new UIListWriter(leftParty);
        gameScreen.Elements.Add("LeftParty", leftParty);
        
        UIElement leftPartyHeader = new UIElement(5, 2, 40, 3);
        gameScreen.Elements.Add("LeftPartyHeader", leftPartyHeader);
        LeftPartyHeader = new UIListWriter(leftPartyHeader);

        UIElement rightParty = new UIElement(45, 3, 79, 19);
        RightParty = new UIListWriter(rightParty);
        gameScreen.Elements.Add("RightParty", rightParty);

        UIElement rightPartyHeader = new UIElement(45, 2, 79, 3);
        gameScreen.Elements.Add("RIghtPartyHeader", rightPartyHeader);
        RightPartyHeader = new UIListWriter(rightPartyHeader);

        // Adding log
        UIElement battleLog = new UIElement(81,1,119,29);
        GameLog = new UIListWriter(battleLog);
        gameScreen.Elements.Add("BattleLog",battleLog);

        // ----------------------------------------------------------
        // Start screen

        UIPane startScreen = new UIPane();
        GameScreens.Add("Start", startScreen);

        startScreen.Elements.Add("OuterBorder", new UIElement(0, 0, 120, 30));

        // Drawing outer border
        foreach (UIElement element in startScreen.Elements.Values)
            UIMethods.BuildBorder(element, '*', '*');

        UIElement startScreenMessage = new UIElement(30, 10, 92, 29);
        startScreen.Elements.Add("StartMessage", startScreenMessage);

        UIListWriter startScreenMessageList = new(startScreenMessage);
        startScreenMessageList.AddEntry("Welocome, Hero!");
        startScreenMessageList.AddEntry("The realm of C# is being assaulted by the malevolent Uncoded One.");
        startScreenMessageList.AddEntry("Only the power of the progammer can defeat him!");
        startScreenMessageList.AddEntry("You must rise up and defeat the great evil to save it's inhabitants.");

        // ----------------------------------------------------------
        // End game screen

        UIPane endScreen = new UIPane();
        GameScreens.Add("End", endScreen);

        UIElement endScreenOuterBorder = new UIElement(0, 0, 120, 30);
        endScreen.Elements.Add("OuterBorder", endScreenOuterBorder);

        // Drawing outer border
        foreach (UIElement element in endScreen.Elements.Values)
            UIMethods.BuildBorder(element, '*', '*');

        UIElement endScreenVictoryMessage = new UIElement(30, 10, 92
            , 29);
        endScreen.Elements.Add("VictoryMessage", endScreenVictoryMessage);

        UIListWriter endScreenVictoryMessageList = new(endScreenVictoryMessage);
        endScreenVictoryMessageList.SetList(
            "Congratulation, Hero!",
            "You have defeated the great evil plaguing the lands and saved it's residents from life long suffering.",
            "",
            "Press ESC to exit the game or press Enter to battle once more.");

    }

    public static Dictionary<string, UIPane> GameScreens = new Dictionary<string, UIPane>();
    public static UIPane ActivePane;
    public static UIListWriter GameLog;
    public static UIListWriter ActionList;
    public static UIListWriter ActionListHeader;
    public static UIListWriter ItemList;
    public static UIListWriter ItemListHeader;
    public static UIListWriter LeftParty;
    public static UIListWriter LeftPartyHeader;
    public static UIListWriter RightParty;
    public static UIListWriter RightPartyHeader;

    public static bool DrawScreen = true;
    public static bool _drawLoop = true;

    public static ConsoleColor Foreground = ConsoleColor.White;
    public static ConsoleColor Background = ConsoleColor.Black;
    public static ConsoleColor Highlight = ConsoleColor.DarkRed;

    public static void Exit() => _drawLoop = false;

    public static void Draw()
    {
        while (_drawLoop)
        {
            //Thread.Sleep(33); // 30 fps?
            if (DrawScreen)
                ActivePane.Draw();
        }
    }
}

public class UIPane
{
    public Dictionary<string, UIElement> Elements { get; } = new Dictionary<string, UIElement>();

    public void Draw()
    {
        UIElement temp = new UIElement(0, 0, 120, 30);
        foreach (UIElement element in Elements.Values)
        {
            lock (element.Buffer.SyncRoot)
            {
                for (int i = 0; i < element.Buffer.GetLength(0); i++)
                    for (int j = 0; j < element.Buffer.GetLength(1); j++)
                    {
                        temp.Buffer[element.Left + i, element.Top + j] = element.Buffer[i, j].ShallowCopy();
                    }
            }
        }
        ToConsoleBuffer(temp.Buffer).Write();
    }

    private static ConsoleBuffer ToConsoleBuffer(ConsoleCharacter[,] arr)
    {
        ConsoleBuffer buffer = new ConsoleBuffer(0, 0, (short)arr.GetLength(0), (short)arr.GetLength(1));

        int x = 0;
        for (int j = 0; j < arr.GetLength(1); j++)
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                buffer.Add(x, Combiner(arr[i, j]), arr[i, j].Character);
                x++;
            }
        return buffer;
    }

    private static ushort Combiner(ConsoleCharacter cc)
    {
        return (ushort)((ushort)((ushort)cc.Background << 4) | (ushort)cc.Foreground);
    }
}

public class UIElement
{
    public ConsoleCharacter[,] Buffer { get; set; }
    public short Left;
    public short Right;
    public short Top;
    public short Bottom;

    public UIElement(short left, short top, short right, short bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        Buffer = new ConsoleCharacter[right - left, bottom - top];
        for (int i = 0; i < Buffer.GetLength(0); i++)
            for (int j = 0; j < Buffer.GetLength(1); j++)
                Buffer[i, j] = new ConsoleCharacter();
    }

    public void Clear()
    {
        foreach (ConsoleCharacter cc in Buffer)
        {
            cc.Character = ' ';
            cc.Foreground = UIManager.Foreground;
            cc.Background = UIManager.Background;
        }
    }
}

public class ConsoleCharacterString
{
    private ConsoleCharacter[]? _array = null;
    public string Text { get; set; }
    public int Length { get => Text.Length; }
    private ConsoleColor _backgroundStandard;
    private ConsoleColor _backgroundHighlight = UIManager.Highlight;
    public ConsoleColor Background
    {
        get
        {
            return IsHighlighted ?_backgroundHighlight : _backgroundStandard;
        }
    }
    public ConsoleColor Foreground;
    public bool IsHighlighted = false;

    public ConsoleCharacterString(ConsoleCharacter[] array)
    {
        _array = array;
        string s = "";
        foreach (ConsoleCharacter c in _array)
            s += c.Character;
        Text = s;
    }

    public ConsoleCharacterString(string text, ConsoleColor backgroundStandard, ConsoleColor backgroundHighlighted, ConsoleColor foreground)
    {
        _backgroundHighlight = backgroundHighlighted;
        _backgroundStandard = backgroundStandard;
        Foreground = foreground;
        Text = text;
    }

    public ConsoleCharacterString(string text, ConsoleColor background, ConsoleColor foreground) : this(text, background, UIManager.Highlight, foreground) { }
    public ConsoleCharacterString(string text, ConsoleColor foreground) : this(text, UIManager.Background, foreground) { }
    public ConsoleCharacterString(string text) : this(text, UIManager.Foreground) { }

    public ConsoleCharacter[] ToArray()
    {
        if(_array != null)
        {
            foreach (ConsoleCharacter c in _array)
                c.Background = Background;
            return _array;
        }
        ConsoleCharacter[] arr = new ConsoleCharacter[Text.Length];
        for(int i = 0; i < Text.Length; i++)
            arr[i] = new ConsoleCharacter(Text[i],Background, Foreground);
        return arr;
    }
}

public class ConsoleCharacterBuilder
{
    private List<ConsoleCharacterString> _buffer = new();

    public void AddString(string text, ConsoleColor background, ConsoleColor foreground)
    {
        _buffer.Add(new ConsoleCharacterString(text, background, foreground));
    }
    public void AddString(string text, ConsoleColor foreground)
    {
        _buffer.Add(new ConsoleCharacterString(text, UIManager.Background, foreground));
    }
    public void AddString(string text)
    {
        _buffer.Add(new ConsoleCharacterString(text, UIManager.Background, UIManager.Foreground));
    }
    public void AddString(ConsoleCharacterString text)
    {
        _buffer.Add(text);
    }

    public ConsoleCharacterString Build()
    {
        int length = 0;
        foreach (ConsoleCharacterString c in _buffer)
            length += c.Text.Length;
        ConsoleCharacter[] arr = new ConsoleCharacter[length];
        int x = 0;
        foreach (ConsoleCharacterString c in _buffer)
        {
            c.ToArray().CopyTo(arr, x);
            x += c.Text.Length;
        }
        return new ConsoleCharacterString(arr);
    }

    public void Clear() => _buffer.Clear();
}

public class ConsoleCharacter
{
    public char Character { get; set; }
    public ConsoleColor Background {  get; set; }
    public ConsoleColor Foreground { get; set; }

    public ConsoleCharacter(char character, ConsoleColor background, ConsoleColor foreground)
    {
        Character = character;
        Foreground = foreground;
        Background = background;
    }

    public ConsoleCharacter() : this(' ', UIManager.Background, UIManager.Foreground) { }
    
    public ConsoleCharacter ShallowCopy()
    {
        return (ConsoleCharacter) MemberwiseClone();
    }
}

public class UIListWriter
{
    public int Count { get => _text.Count; }
    private List<ConsoleCharacterString> _text = new();
    private UIElement _element;
    private int _maxLineChars;
    private int _maxTotalChars;
    private int _lowerIndex = 0;
    private int _upperIndex = 0;
    private List<int> _highlighted = new();

    public UIListWriter(UIElement element)
    {
        _element = element;
        _maxLineChars = element.Right - element.Left;
        _maxTotalChars = (element.Bottom - element.Top) * (_maxLineChars);
    }

    public void ColorEntry(int index, bool highlight = true)
    {
        if (highlight)
        {
            if(!_highlighted.Contains(index)) _highlighted.Add(index);
            WriteList();
            return;
        }
        _highlighted.Remove(index);
        WriteList();
    }

    public void ClearHighlights()
    {
        _highlighted.Clear();
        WriteList();
    }

    public void AddEntry(string text) => AddEntry(new ConsoleCharacterString(text));
    public void AddEntry(ConsoleCharacterString text)
    {
        _text.Add(text);
        _upperIndex = _text.Count - 1;
        _lowerIndex = GetMinMessageIndex(_upperIndex);
        WriteList();
    }
    public void SetList(params string[] text)
    {
        List<ConsoleCharacterString> list = new();
        foreach (string s in text)
            list.Add(new ConsoleCharacterString(s));
        _text = list;
        _highlighted.Clear();
        _lowerIndex = 0;
        _upperIndex = GetMaxMessageIndex(_lowerIndex);
        WriteList();
    }
    public void SetList(params ConsoleCharacterString[] text)
    {
        List<ConsoleCharacterString> list = [.. text];
        _text = list;
        _highlighted.Clear();
        _lowerIndex = 0;
        _upperIndex = GetMaxMessageIndex(_lowerIndex);
        WriteList();
    }
    public List<string> GetList() => _text.Select(o => o.Text).ToList();
    
    public void ClearList()
    {
        _text.Clear();
        _highlighted.Clear();
        _lowerIndex = 0;
        _upperIndex = 0;
        _element.Clear();
    }
    public void ScrollList(bool up)
    {
        if (_text.Count == 0) return;
        if (up)
        {
            if (_lowerIndex == 0) return;
            _lowerIndex--;
            _upperIndex = GetMaxMessageIndex(_lowerIndex);
            WriteList();
            return;
        }
        if (_upperIndex + 1 == _text.Count) return;
        _upperIndex++;
        _lowerIndex = GetMinMessageIndex(_upperIndex);
        WriteList();
    }

    private void WriteList()
    {
        lock (_element.Buffer.SyncRoot)
        {
            _element.Clear();
            ConsoleCharacterString? s = null;
            ConsoleCharacter[]? arr = null;
            int newLineCounter = 0;
            int currentRowIndex;
            int currentMesage = 0;
            for (int i = _lowerIndex; i <= _upperIndex; i++)
            {
                currentRowIndex = 0;
                s = _text[i];
                s.IsHighlighted = _highlighted.Contains(i);
                arr = s.ToArray();
                for (int c = 0; c < arr.Length; c++)
                {
                    if (currentRowIndex >= _maxLineChars)
                    {
                        newLineCounter++;
                        currentRowIndex = 0;
                    }
                    _element.Buffer[currentRowIndex, newLineCounter] = arr[c].ShallowCopy();
                    currentRowIndex++;
                }
                newLineCounter++;
                currentMesage++;
            }
        }
    }

    private int GetMinMessageIndex(int maxIndex)
    {
        int runningTotal = 0;
        for (int i = maxIndex; i >= 0; i--)
        {
            runningTotal += (int)Math.Ceiling(((double)_text[i].Length / (double)_maxLineChars)) * _maxLineChars;
            if (runningTotal > _maxTotalChars)
                return i + 1;
        }
        return 0;
    }

    private int GetMaxMessageIndex(int minIndex)
    {
        int runningTotal = 0;
        for (int i = minIndex; i < _text.Count; i++)
        {
            runningTotal += (int)Math.Ceiling(((double)_text[i].Length / (double)_maxLineChars)) * _maxLineChars;
            if (runningTotal > _maxTotalChars)
                return i - 1;
        }
        return _text.Count - 1;
    }

    public int SelectFromList(int startingIndex = 0, bool clearHighlight = true)
    {
        int index = startingIndex;
        ColorEntry(index);

        while (true)
        {
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Enter:
                    if (clearHighlight)
                        ColorEntry(index, false);
                    return index;
                case ConsoleKey.DownArrow:
                    ColorEntry(index, false);
                    if (index >= Count - 1)
                        index = 0;
                    else
                        index++;
                    ColorEntry(index);
                    continue;
                case ConsoleKey.UpArrow:
                    ColorEntry(index, false);
                    if (index == 0)
                        index = Count - 1;
                    else
                        index--;
                    ColorEntry(index);
                    continue;
                case ConsoleKey.Escape:
                    if (clearHighlight)
                        ColorEntry(index, false);
                    return -1;
            }
        }
    }
}

public static class UIMethods
{
    public static void BuildBorder(UIElement element, char widthBorders = '|', char heightBorder = '-')
    {
        int width = element.Right - element.Left;
        int height = element.Bottom - element.Top;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i == 0) { element.Buffer[i, j].Character = widthBorders; continue; }
                if (i == width - 1)
                { element.Buffer[i, j].Character = widthBorders; continue; }
                if (j == 0) { element.Buffer[i, j].Character = heightBorder; continue; }
                if (j == height - 1)
                { element.Buffer[i, j].Character = heightBorder; continue; }
            }
        }
    }
}