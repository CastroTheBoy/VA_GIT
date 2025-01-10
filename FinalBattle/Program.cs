// Code
using System.Reflection;

Console.BackgroundColor = ConsoleColor.Black;
Console.ForegroundColor = ConsoleColor.Green;
Console.Title = "The Final Battle";
Console.Clear();
Console.CursorVisible = false;

// Code
MenuManager.Init();
GameManager game = new GameManager(PlayerType.User, PlayerType.Robot, "Castro");
game.Run();

public static class MenuManager
{
    public static void Init() { }

    public static LogWriterElement BattleLog
    {
        get
        {
            return (LogWriterElement)UIManager.Instance.GetElement("Game", "BattleLog");
        }
    }

    public static ListWriterElement PlayerParty
    {
        get
        {
            return (ListWriterElement)UIManager.Instance.GetElement("Game", "PlayerParty");
        }
    }

    public static ListWriterElement EnemyParty
    {
        get
        {
            return (ListWriterElement)UIManager.Instance.GetElement("Game", "EnemyParty");
        }
    }

    public static NamedListWriterElement ActionList
    {
        get
        {
            return (NamedListWriterElement)UIManager.Instance.GetElement("Game", "ActionList");
        }
    }

    static MenuManager()
    {
        //--------------------------------------------------------------------
        // Screen clearer
        UIManager.Instance.CreateGroup("Clearer");
        NamedListWriterElement clearer = new NamedListWriterElement(0, 0, 120, 30);
        clearer.Clear();
        UIManager.Instance.AddElement("Clearer", "Clearer", clearer);

        //--------------------------------------------------------------------
        // Game screen group
        UIManager.Instance.CreateGroup("Game");

        // borders
        BorderElement gameOuterBorders = new BorderElement(0, 0, 120, 30);
        UIManager.Instance.AddElement("Game", "OuterBorder", gameOuterBorders);
        BorderElement gameActionBorders = new BorderElement(0, 20, 81, 30);
        UIManager.Instance.AddElement("Game", "ActionBorder", gameActionBorders);
        BorderElement gameLogBorders = new BorderElement(80, 0, 120, 30);
        UIManager.Instance.AddElement("Game", "LogBorder", gameLogBorders);

        gameOuterBorders.BuildBorder(Border.Top | Border.Right | Border.Left | Border.Bottom);
        gameActionBorders.BuildBorder(Border.Top | Border.Right | Border.Left | Border.Bottom);
        gameLogBorders.BuildBorder(Border.Top | Border.Right | Border.Left | Border.Bottom);

        // Actions
        NamedListWriterElement actionList = new NamedListWriterElement(1, 21, 60, 29);
        foreach (TextEntry entry in GetActionTextEntries())
        {
            actionList.AddEntry(entry.Text, entry);
        }
        UIManager.Instance.AddElement("Game", "ActionList", actionList);

        // Parties
        ListWriterElement playerPartyList = new ListWriterElement(1, 1, 40, 10);
        UIManager.Instance.AddElement("Game", "PlayerParty", playerPartyList);
        ListWriterElement enemyPartyList = new ListWriterElement(50, 1, 79, 10);
        UIManager.Instance.AddElement("Game", "EnemyParty", enemyPartyList);

        // Battle Log
        LogWriterElement battleLog = new LogWriterElement(81, 1, 119, 29);
        UIManager.Instance.AddElement("Game", "BattleLog", battleLog);
        //--------------------------------------------------------------------
        // Start menu group
        UIManager.Instance.CreateGroup("Start");

        // Game title
        NamedListWriterElement title = new NamedListWriterElement(40, 10, 80, 11);
        UIManager.Instance.AddElement("Start", "Title", title);

        // Game start message
        NamedListWriterElement startMessage = new NamedListWriterElement(40, 11, 80, 13);
        UIManager.Instance.AddElement("Start", "StartMessage", startMessage);
    }

    public static int SelectFromList(IListWriter list)
    {
        int currentAction = 0;
        int previousAction = 0;
        list.GetElement(currentAction).IsHighlighted = true;
        while (true)
        {
            list.WriteList(true);
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow:
                    if (currentAction + 1 > list.Count - 1)
                    {
                        currentAction = 0;
                        break;
                    }
                    currentAction++;
                    break;
                case ConsoleKey.UpArrow:
                    if (currentAction - 1 < 0)
                    {
                        currentAction = list.Count - 1;
                        break;
                    }
                    currentAction--;
                    break;
                case ConsoleKey.Enter:
                    list.GetElement(currentAction).IsHighlighted = false;
                    list.WriteList(true);
                    return currentAction;
                default:
                    continue;
            }
            list.GetElement(currentAction).IsHighlighted = true;
            list.GetElement(previousAction).IsHighlighted = false;
            previousAction = currentAction;
        }
    }

    public static List<TextEntry> EntityListToTextEntry(List<Entity> entities)
    {
        List<TextEntry> list = new List<TextEntry>();
        foreach (Entity entity in entities)
        {
            list.Add(new TextEntry($"{entity.Name} {entity.HP}/{entity.MaxHP}"));
        }
        return list;
    }

    public static List<TextEntry> GetActionTextEntries()
    {
        return new List<TextEntry>
        {
            new TextEntry("Attack"),
            new TextEntry("Kill"),
            new TextEntry("Nothing")
        };
    }
}

public class GameManager
{
    public EntityManager Entities { get; }
    public PlayerType PlayerPartyMode { get; }
    public PlayerType EnemyPartyMode { get; }
    private GameUserManager _gameUserManager;
    private Dictionary<int, List<Entity>> _roundEnemyRoster =
        new Dictionary<int, List<Entity>>();
    private int _currentRound = 0;
    private int _maxRounds = 2;

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        Entities = new EntityManager();
        Entities.AddEntity(new Entity(EntityType.PLAYER, playerName));
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;
        _gameUserManager = new GameUserManager(Entities);
        InitializeEnemyRoster();
    }

    //public GameManager() : this(
    //    ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Ally),
    //    ConsoleManager.AskForPlayerTypeWithHighlight(Faction.Enemy),
    //    ConsoleManager.AskForPlayerName())
    //{ }

    public void InitializeEnemyRoster()
    {
        for (int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0:
                    _roundEnemyRoster[0] = [new Entity(EntityType.SKELETON),
                        new Entity(EntityType.SKELETON)];
                    break;
                case 1:
                    _roundEnemyRoster[1] = [new Entity(EntityType.SKELETON),
                        new Entity(EntityType.SKELETON)];
                    break;
                case 2:
                    _roundEnemyRoster[2] = [new Entity(EntityType.SKELETON),
                        new Entity(EntityType.SKELETON),
                        new Entity(EntityType.UNCODED_ONE)];
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public void SetUpRoundRoster()
    {
        foreach (Entity e in _roundEnemyRoster[_currentRound])
            Entities.AddEntity(e);
    }

    private bool IsFinalRound() => _currentRound >= _maxRounds;
    private bool EnemiesCleared() => Entities.GetEnemyParty().Count == 0;
    private bool PlayersCleared() => Entities.GetPlayerParty().Count == 0;

    public bool Run()
    {
        bool moveToNextRound = false;
        SetUpRoundRoster();
        while (true)
        {
            moveToNextRound = false;
            foreach (Entity entity in Entities.GetPlayerParty())
            {
                ExecuteTurn(entity, PlayerPartyMode);
                if (EnemiesCleared())
                {
                    if (IsFinalRound())
                    {
                        MenuManager.BattleLog.AddMessage("All enemies have been defeated! You Win!");
                        return true;
                    }
                    else
                    {
                        _currentRound++;
                        moveToNextRound = true;
                        SetUpRoundRoster();
                        MenuManager.BattleLog.AddMessage($"All enemies have been defeated in " +
                            $"round {_currentRound}. Proceeding to round " +
                            $"{_currentRound + 1}/{_maxRounds + 1}.");
                    }
                }
            }
            if (moveToNextRound) continue;
            foreach (Entity entity in Entities.GetEnemyParty())
            {
                ExecuteTurn(entity, EnemyPartyMode);
                if (PlayersCleared())
                {
                    MenuManager.BattleLog.AddMessage("You have been slain! GAME OVER...");
                    return false;
                }
            }
        }
    }

    private void ExecuteTurn(Entity entity, PlayerType type)
    {
        CallTurnUI();
        //ConsoleManager.Turn(entity.Name);
        _gameUserManager.DoTurn(entity, type);
    }

    public void CallTurnUI()
    {
        MenuManager.PlayerParty.SetList(MenuManager.EntityListToTextEntry(Entities.GetPlayerParty()), true);
        MenuManager.EnemyParty.SetList(MenuManager.EntityListToTextEntry(Entities.GetEnemyParty()), true);
        UIManager.Instance.DrawGroup("Clearer");
        UIManager.Instance.DrawGroup("Game");
    }
}

public class GameUserManager
{
    private EntityManager _entities { get; }

    public GameUserManager(EntityManager entities) => _entities = entities;

    public void DoTurn(Entity entity, PlayerType playerType)
    {
        IAction newAction = GetAction(playerType);
        switch (newAction.GetType().GetInterfaces())
        {
            case Type[] type when type.Contains(typeof(ITargetEnemy)):
                newAction.ExecuteAction(entity, EntitySelection(
                    _entities.GetEnemiesForEntity(entity), playerType));
                break;
            case Type[] type when type.Contains(typeof(ITargetSelf)):
                newAction.ExecuteAction(entity, entity);
                break;
            case Type[] type when type.Contains(typeof(ITargetAlly)):
                newAction.ExecuteAction(entity, EntitySelection(
                    _entities.GetAlliesForEntity(entity), playerType));
                break;
            default:
                throw new NotSupportedException();
        }
    }
    private Entity EntitySelection(List<Entity> entities, PlayerType type)
    {
        return type switch
        {
            PlayerType.Robot => entities[0],
            PlayerType.User => entities[MenuManager.SelectFromList(MenuManager.EnemyParty)],
            _ => throw new NotSupportedException()
        };
    }

    private IAction GetAction(PlayerType type)
    {
        return type switch
        {
            PlayerType.Robot => ActionFactory.GetAction(typeof(ActionAttack)),
            PlayerType.User => ActionMap(MenuManager.SelectFromList(MenuManager.ActionList)),
            _ => throw new NotSupportedException()
        };
    }

    private IAction ActionMap(int index)
    {
        return index switch
        {
            0 => ActionFactory.GetAction(typeof(ActionAttack)),
            1 => ActionFactory.GetAction(typeof(ActionKill)),
            2 => ActionFactory.GetAction(typeof(ActionNothing)),
            _ => throw new NotSupportedException()
        };
    }
}

public class EntityManager
{
    private Dictionary<Guid, Entity> _entityDict;

    public EntityManager()
    {
        _entityDict = new Dictionary<Guid, Entity>();
    }

    public List<Entity> GetPlayerParty() => GetFactionParty(Faction.Ally);
    public List<Entity> GetEnemyParty() => GetFactionParty(Faction.Enemy);

    private List<Entity> GetFactionParty(Faction faction)
    {
        List<Entity> list = new List<Entity>();
        foreach (KeyValuePair<Guid, Entity> kvp in _entityDict)
            if (kvp.Value.Faction == faction)
                list.Add(kvp.Value);
        return list;
    }
    public List<Entity> GetAlliesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => GetPlayerParty(),
            Faction.Enemy => GetEnemyParty(),
            _ => throw new NotImplementedException()
        };
    }
    public List<Entity> GetEnemiesForEntity(Entity entity)
    {
        return entity.Faction switch
        {
            Faction.Ally => GetEnemyParty(),
            Faction.Enemy => GetPlayerParty(),
            _ => throw new NotImplementedException()
        };
    }
    public void AddEntity(Entity entity)
    {
        entity.EntityDied += OnEntityDeath;
        _entityDict.Add(entity.UniqueID, entity);
    }
    private void OnEntityDeath(Entity entity)
    {
        _entityDict.Remove(entity.UniqueID);
    }
}

// ACTIONS
public interface IAction
{
    public void ExecuteAction(Entity source, Entity target);
    public string Name();
}

public interface ITargetSelf { }
public interface ITargetEnemy { }
public interface ITargetAlly { }

public class ActionAttack : IAction, ITargetEnemy
{
    public void ExecuteAction(Entity source, Entity target)
    {
        int damage = source.GetDamage();
        string attackName = source.GetDamageString();
        MenuManager.BattleLog.AddMessage($"{source.Name} used {attackName}" +
            $" on {target.Name}");
        MenuManager.BattleLog.AddMessage($"{attackName} dealt {damage} damage to " +
            $"{target.Name}");
        target.DamageHealth(damage);
    }
    public string Name() => "Attack";
}

public class ActionNothing : IAction, ITargetSelf
{
    public void ExecuteAction(Entity source, Entity target) =>
        MenuManager.BattleLog.AddMessage($"{source.Name} did NOTHING");

    public string Name() => "Do nothing";
}

public class ActionKill : IAction, ITargetEnemy
{
    public void ExecuteAction(Entity source, Entity target)
    {
        MenuManager.BattleLog.AddMessage($"{target.Name} was disintegrated!");
        target.Kill();
    }

    public string Name() => "Kill";
}

public class ActionUseItem
{
    private IConsumable _item;

    public ActionUseItem(Entity target, IConsumable item)
    {
        _item = item;
    }

    public void ExecuteAction(Entity source, Entity target)
    {
        _item.Use(target);
    }
}

public static class ActionFactory
{
    public static List<Type> ActionTypes = new List<Type>();

    static ActionFactory()
    {
        foreach (Type prodType in Assembly.GetExecutingAssembly().GetTypes()
            .Where(prodType => prodType.GetInterfaces().Contains(typeof(IAction))))
            ActionTypes.Add(prodType);
    }

    public static IAction GetAction(Type type)
    {
        object? a_Context = Activator.CreateInstance(type);
        if (a_Context == null)
            throw new ArgumentNullException();
        return (IAction)a_Context;
    }
}

public class BattleManager
{
    public PartyManager PlayerParty { get; }
    public PartyManager EnemyParty { get; }

    public BattleManager()
    {
        PlayerParty = new PartyManager();
        EnemyParty = new PartyManager();
    }

    public void DoAction(IAction action)
    {

    }
}

public class PartyManager
{
    public Party Party { get; }
    public Inventory Inventory { get; }

    public PartyManager()
    {
        Party = new Party();
        Inventory = new Inventory();
    }
}

public class Party
{
    private Dictionary<Guid, Entity> _party;

    public Party() => _party = new Dictionary<Guid, Entity>();

    public List<Entity> GetParty() => _party.Values.ToList();

    public void AddEntity(Entity entity)
    {
        entity.EntityDied += OnEntityDeath;
        _party.Add(entity.UniqueID, entity);
    }
    private void OnEntityDeath(Entity entity)
    {
        _party.Remove(entity.UniqueID);
    }
}

// ENTITY
public class Entity
{
    private static Dictionary<EntityType, EntityStats> _entityStats =
        new Dictionary<EntityType, EntityStats>();
    static Entity()
    {
        _entityStats.Add(EntityType.PLAYER,
            new EntityStats(25, () => 2, "PUNCH", Faction.Ally));
        _entityStats.Add(EntityType.SKELETON,
            new EntityStats(5, () => new Random().Next(2), "BONE CRUNCH", Faction.Enemy));
        _entityStats.Add(EntityType.UNCODED_ONE,
            new EntityStats(10, () => new Random().Next(3), "UNRAVEL", Faction.Enemy));
    }

    public string Name { get; }
    public int HP { get; private set; }
    public int MaxHP { get; }
    public Faction Faction { get; }
    public EntityType Type { get; }
    public Guid UniqueID { get; }
    public event Action<Entity>? EntityDied;

    private Entity(EntityType type, int hp, Faction faction, int maxhp, string name, Guid guid)
    {
        Type = type;
        HP = hp;
        Faction = faction;
        MaxHP = maxhp;
        Name = name;
        UniqueID = guid;
    }

    public Entity(EntityType type)
    {
        Type = type;
        HP = _entityStats[Type].HP;
        Faction = _entityStats[Type].Faction;
        MaxHP = HP;
        Name = type.ToString();
        UniqueID = Guid.NewGuid();
    }
    public Entity(EntityType type, string name)
    {
        Type = type;
        HP = _entityStats[Type].HP;
        Faction = _entityStats[Type].Faction;
        MaxHP = HP;
        Name = name;
        UniqueID = Guid.NewGuid();
    }

    public string GetDamageString() => _entityStats[Type].DamageString;
    public int GetDamage() => _entityStats[Type].Damage.Invoke();

    public void DamageHealth(int damage)
    {
        if (this.HP - damage > 0)
        {
            this.HP -= damage;
            HPAmountMessage();
            return;
        }
        DeathMessage();
        EntityDied?.Invoke(this);
    }
    public void HealHealth(int amount)
    {
        this.HP = Math.Min(this.MaxHP, this.HP + amount);
        HPAmountMessage();
    }

    private void HPAmountMessage()
    {
        MenuManager.BattleLog.AddMessage($"{this.Name} is now at {this.HP} / " +
            $"{this.MaxHP}");
    }
    private void DeathMessage() =>
        MenuManager.BattleLog.AddMessage($"{this.Name} has been defeated!");
    public void Kill() => this.DamageHealth(10000);
}

public record EntityStats(int HP, Func<int> Damage, string DamageString, Faction Faction);

public class Inventory
{
    private List<IConsumable> _inventory { get; }
    public Inventory()
    {
        _inventory = new List<IConsumable>();
    }

    public List<IConsumable> GetInventory() => _inventory;

    public void AddItem(IConsumable item) => _inventory.Add(item);

    // Doesn't matter which one is removed, just that a single of that type is removed
    public void RemoveItem(IConsumable item) => _inventory.Remove(item);
}

public interface IConsumable
{
    public string Name { get; }
    public void Use(Entity target);
}

public class HealthPotion : IConsumable
{
    public string Name { get => "Health potion"; }
    private int _healAmount = 10;

    public void Use(Entity target)
    {
        target.HealHealth(_healAmount);
    }
}

public class Bomb : IConsumable
{
    public string Name { get => "Bomb"; }
    private int _damageAmount = 5;

    public void Use(Entity target) => target.DamageHealth(_damageAmount);
}

// ENUMS
public enum PlayerType { User, Robot }
public enum Faction { Ally, Enemy }
public enum EntityType { SKELETON, UNCODED_ONE, PLAYER }
public enum SelectionArea { ActionColumnOne, ActionColumnTwo, PartyLeft, PartyRight }