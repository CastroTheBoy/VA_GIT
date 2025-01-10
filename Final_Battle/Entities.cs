// ENTITY
public class Entity
{
    public ConsoleCharacterString Name { get; }
    public int HP { get; private set; }
    public int MaxHP { get; }
    public Guid UniqueID { get; }
    public string AttackName;
    private Func<int> _damageFunction;
    public event Action<Entity>? EntityDied;
    private Gear? _gear;

    public Entity(int hp, int maxhp, ConsoleCharacterString name, Func<int> damage, string attackName)
    {
        HP = hp;
        MaxHP = maxhp;
        Name = name;
        UniqueID = Guid.NewGuid();
        _damageFunction = damage;
        AttackName = attackName;
    }

    public string GetDamageString() => AttackName;
    public int GetDamage() => _damageFunction.Invoke();

    public Gear? EquipGear(Gear gear)
    {
        if (HasGear())
        {
            Gear? old = UnequipGear();
            _gear = gear;
            return old;
        }
        else
        {
            _gear = gear;
            return null;
        }
    }

    public Gear? UnequipGear()
    {
        Gear? gear = _gear;
        _gear = null;
        return gear;
    }

    public bool HasGear() => _gear != null;
    public Gear? GetGear() => _gear;

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
        HP = Math.Min(MaxHP, HP + amount);
        HPAmountMessage();
    }

    private void HPAmountMessage()
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString(Name);
        builder.AddString(" is now at ");
        builder.AddString(HPAmount());
        builder.AddString(" HP.");
        UIManager.GameLog.AddEntry(builder.Build());
    }
    private void DeathMessage()
    {
        ConsoleCharacterBuilder builder = new();
        builder.AddString(Name);
        builder.AddString(" has been defeated!");
        UIManager.GameLog.AddEntry(builder.Build());
    }
    public ConsoleCharacterString HPAmount()
    {
        ConsoleCharacterBuilder builder = new();
        ConsoleColor hpColor = ConsoleColor.Green;
        int hpThird = MaxHP / 3;
        if (HP <= hpThird * 2 && HP > hpThird)
            hpColor = ConsoleColor.DarkYellow;
        if (HP <= hpThird)
            hpColor = ConsoleColor.Red;
        builder.AddString($"{HP}", foreground: hpColor);
        builder.AddString("/");
        builder.AddString($"{MaxHP}", foreground: ConsoleColor.Green);
        return builder.Build();
    }
    public void Kill() => this.DamageHealth(10000);
}

public static class EntityFactory
{
    public static Entity CreatePlayer() => new Entity(25, 25,
        new ConsoleCharacterString("Player", ConsoleColor.Black, ConsoleColor.Yellow), () => 2, "Slap");
    public static Entity CreatePlayer(string name) => new Entity(25, 25,
        new ConsoleCharacterString(name, ConsoleColor.Black, ConsoleColor.Yellow), () => 2, "Slap");
    public static Entity CreateVin() => new Entity(15, 15,
        new ConsoleCharacterString("Vin Fletcher", ConsoleColor.Black, ConsoleColor.Yellow), () => 2, "Slap");
    public static Entity CreateSkeleton() => new Entity(5, 5,
        new ConsoleCharacterString("Skeleton", ConsoleColor.Black, ConsoleColor.Gray), () => new Random().Next(2), "Bone zone");
    public static Entity CreateUncodedOne()
    {
        string name = "Uncoded One";
        ConsoleCharacterBuilder builder = new();
        for (int i = 0; i < name.Length; i++)
            builder.AddString(name.Substring(i, 1),
                i % 2 == 0 ? ConsoleColor.Blue : ConsoleColor.DarkBlue);
        return new Entity(10, 10, builder.Build(), () => new Random().Next(3), "Deconstruct");
    }
}