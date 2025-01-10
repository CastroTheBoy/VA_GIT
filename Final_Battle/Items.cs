// INVENTORY
public class Inventory
{
    private List<Consumable> _consumables { get; } = new List<Consumable> { };
    private List<Gear> _gear { get; } = new List<Gear> { };

    public List<Consumable> GetConsumables() => _consumables;
    public List<Gear> GetGear() => _gear;

    public void AddConsumable(params Consumable[] Consumable)
    {
        foreach (Consumable item in Consumable)
            _consumables.Add(item);
    }
    public void AddConsumable(params Gear[] Consumable)
    {
        foreach (Gear item in Consumable)
            _gear.Add(item);
    }

    public void RemoveConsumable(params Consumable[] Consumable)
    {
        foreach (Consumable item in Consumable)
            _consumables.Remove(item);
    }
    public void RemoveConsumable(params Gear[] Consumable)
    {
        foreach (Gear item in Consumable)
            _gear.Remove(item);
    }
}

// TARGET TYPES
public interface ITargetSingleEnemy
{
    public void Target(Entity source, Entity target);
}

public interface ITargetSingleAlly
{
    public void Target(Entity source, Entity target);
}

public interface ITargetEnemyParty
{
    public void Target(Entity source, Party party);
}

public interface ITargetAllyParty
{
    public void Target(Entity source, Party party);
}

// CONSUMABLES
public abstract class Consumable
{
    public abstract ConsoleCharacterString Name { get; }
}

public class FirePotion : Consumable, ITargetSingleEnemy
{
    public override ConsoleCharacterString Name { get; } = new ConsoleCharacterString("Fire potion", ConsoleColor.Red);
    private int _damage = 3;
    public void Target(Entity source, Entity target)
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString(source.Name);
        builder.AddString(" used a fire potion on ");
        builder.AddString(target.Name);
        UIManager.GameLog.AddEntry(builder.Build());
        builder.Clear();

        builder.AddString(target.Name);
        builder.AddString(" receives ");
        builder.AddString($"{_damage}", ConsoleColor.Red);
        builder.AddString(" damage!");
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(_damage);
    }
}

public class LightningPotion : Consumable, ITargetEnemyParty
{
    public override ConsoleCharacterString Name { get; } = new ConsoleCharacterString("Lightning potion", ConsoleColor.Yellow);
    private int _damage = 1;
    public void Target(Entity source, Party target)
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString(source.Name);
        builder.AddString(" used a lightning potion! All enemies get receive ");
        builder.AddString($"{_damage}", ConsoleColor.Red);
        builder.AddString(" damage!");
        UIManager.GameLog.AddEntry(builder.Build());
        foreach (Entity entity in target.Entities)
            entity.DamageHealth(_damage);
    }
}

public class HealthPotion : Consumable, ITargetSingleAlly
{
    public override ConsoleCharacterString Name { get; } = new ConsoleCharacterString("Health potion", ConsoleColor.Green);
    private int _healAmount = 10;

    public void Target(Entity source, Entity target)
    {
        ConsoleCharacterString name = source == target ? new ConsoleCharacterString("themselves", source.Name.Foreground) : target.Name;
        ConsoleCharacterBuilder builder = new();
        builder.AddString(source.Name);
        builder.AddString(" used a health potion to heal ");
        builder.AddString(name);
        builder.AddString(" by ");
        builder.AddString($"{_healAmount}", ConsoleColor.Green);
        builder.AddString(" points!");
        UIManager.GameLog.AddEntry(builder.Build());
        target.HealHealth(_healAmount);
    }
}

// GEAR
public abstract class Gear
{
    public abstract string Name { get; }
    public abstract string AttackName { get; }
}

public class Weapon : Gear, ITargetSingleEnemy
{
    public override string Name { get; }
    public override string AttackName { get; }
    private int _damage { get; }

    private Weapon(string name, string attackName, int damage)
    {
        Name = name;
        AttackName = attackName;
        _damage = damage;
    }

    public void Target(Entity source, Entity target)
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString(source.Name);
        builder.AddString($" used {Name} to attack ");
        builder.AddString(target.Name);
        builder.AddString("!");
        UIManager.GameLog.AddEntry(builder.Build());
        builder.Clear();

        builder.AddString($"{Name}'s {AttackName} dealt {_damage} damage to ");
        builder.AddString(target.Name);
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(_damage);
    }

    public static Weapon CreateExcalibur() => new Weapon("Excalibur", "Holy slash", 100);
    public static Weapon CreateIronSword() => new Weapon("Iron sword", "Slash", 3);
    public static Weapon CreateWoodenSword() => new Weapon("Wooden sword", "Slash", 1);
    public static Weapon CreateVinsBow() => new Weapon("Vin's bow", "Quick shot", 3);
}