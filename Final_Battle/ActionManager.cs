public interface IActionManager
{
    public void DoAction(Entity entity);
}

public class RobotActionManager : IActionManager
{
    private BattleManager _battle;

    public RobotActionManager(BattleManager battleManager)
    {
        _battle = battleManager;
    }

    public void DoAction(Entity entity)
    {
        Attack(entity, _battle.GetEnemyParty(entity).Entities[0]);
    }

    private void Attack(Entity attacker, Entity target)
    {
        int damage = attacker.GetDamage();
        ConsoleCharacterBuilder builder = new();
        builder.AddString(attacker.Name);
        builder.AddString($" used {attacker.AttackName} ");
        builder.AddString(target.Name);
        builder.AddString(" took ");
        builder.AddString($"{damage}", foreground: ConsoleColor.Red);
        builder.AddString(" damage.");
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(damage);
    }
}

public class PlayerActionManager : IActionManager
{
    private BattleManager _battle;

    public PlayerActionManager(BattleManager battle)
    {
        _battle = battle;
    }

    public void DoAction(Entity entity)
    {
        int action = 0;
        while (true)
        {
            UIManager.ActionList.ClearHighlights();
            action = UIManager.ActionList.SelectFromList(action, false);
            switch (action)
            {
                case -1:
                    action = 0;
                    continue;
                case 0:
                    if (Attack(entity, _battle.SelectFromEnemies(entity)))
                        return;
                    break;
                case 1:
                    if (Kill(_battle.SelectFromEnemies(entity)))
                        return;
                    break;
                case 2:
                    if (UseItem(entity, _battle.GetAllyParty(entity), _battle.GetEnemyParty(entity)))
                        return;
                    break;
                case 3:
                    if (EquipGear(entity, _battle.GetAllyParty(entity)))
                        return;
                    break;
                case 4:
                    if (UseGear(entity, _battle.GetAllyParty(entity), _battle.GetEnemyParty(entity)))
                        return;
                    break;
                default:
                    throw new NotSupportedException("Unsupported action index in ActionManager.DoAction()");
            }
        }
    }

    private bool Attack(Entity attacker, Entity? target)
    {
        if (target == null) return false;
        int damage = attacker.GetDamage();
        ConsoleCharacterBuilder builder = new();
        builder.AddString(attacker.Name);
        builder.AddString($" used {attacker.AttackName} ");
        builder.AddString(target.Name);
        builder.AddString(" took ");
        builder.AddString($"{damage}", foreground: ConsoleColor.Red);
        builder.AddString(" damage.");
        UIManager.GameLog.AddEntry(builder.Build());
        target.DamageHealth(damage);
        return true;
    }

    private bool Kill(Entity? target)
    {
        if (target == null) return false;
        ConsoleCharacterBuilder builder = new();
        builder.AddString("~ ");
        builder.AddString("Hand of God", foreground: ConsoleColor.Red);
        builder.AddString($" struck ");
        builder.AddString(target.Name);
        builder.AddString(" ~");
        UIManager.GameLog.AddEntry(builder.Build());
        target.Kill();
        return true;
    }

    private bool UseGear(Entity entity, Party allyParty, Party enemyParty)
    {
        if (!entity.HasGear())
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(entity.Name);
            builder.AddString(" has no gear equipped.");
            UIManager.GameLog.AddEntry(builder.Build());
            return false;
        }
        if (entity.GetGear() is ITargetAllyParty)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetEnemyParty)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetSingleAlly)
        {
            throw new NotImplementedException();
        }
        if (entity.GetGear() is ITargetSingleEnemy)
        {
            Entity? target = _battle.SelectFromEnemies(entity);
            if (target is null) return false;
            ((ITargetSingleEnemy)entity.GetGear()).Target(entity, target);
            return true;
        }
        return false;
    }

    private bool EquipGear(Entity entity, Party allyParty)
    {
        if (allyParty.Invetory.GetGear().Count == 0)
        {
            UIManager.GameLog.AddEntry("No gear to equip!");
            return false;
        }

        WriteGearList(allyParty);
        int index = UIManager.ItemList.SelectFromList();
        ClearItemList();
        if (index == -1) return false;     

        Gear gear = allyParty.Invetory.GetGear()[index];
        Gear? oldGear = entity.EquipGear(gear);
        ConsoleCharacterBuilder builder = new();
        if (oldGear == null)
        {
            builder.AddString(entity.Name);
            builder.AddString(" equiped ");
            builder.AddString($"{gear.Name}.");
            UIManager.GameLog.AddEntry(builder.Build());
            allyParty.Invetory.RemoveConsumable(gear);
            return true;
        }
        else
        {
            builder.AddString(entity.Name);
            builder.AddString(" equiped ");
            builder.AddString($"{gear.Name}.");
            UIManager.GameLog.AddEntry(builder.Build());
            builder.Clear();

            UIManager.GameLog.AddEntry($"Previously equiped {oldGear.Name} was moved back to inventory.");
            allyParty.Invetory.RemoveConsumable(gear);
            allyParty.Invetory.AddConsumable(oldGear);
            return true;
        }
    }

    private bool UseItem(Entity entity, Party allyParty, Party enemyParty)
    {
        if (allyParty.Invetory.GetConsumables().Count == 0)
        {
            UIManager.GameLog.AddEntry("No Consumable in inventory!");
            return false;
        }
        int index = 0;
        WriteConsumableList(allyParty);
        while (true)
        {
            index = UIManager.ItemList.SelectFromList(index, false);
            if (index == -1)
            {
                ClearItemList();
                return false;
            }

            Consumable item = SelectFromConsumableList(allyParty, index);
            if (item is ITargetAllyParty)
            {
                ((ITargetAllyParty)item).Target(entity, allyParty);
                allyParty.Invetory.RemoveConsumable(item);
                ClearItemList();
                return true;
            }
            if (item is ITargetEnemyParty)
            {
                ((ITargetEnemyParty)item).Target(entity, enemyParty);
                allyParty.Invetory.RemoveConsumable(item);
                ClearItemList();
                return true;
            }
            if (item is ITargetSingleAlly)
            {
                Entity? target = _battle.SelectFromAllies(entity);
                if (target is null) continue;
                ((ITargetSingleAlly)item).Target(entity, target);
                allyParty.Invetory.RemoveConsumable(item);
                ClearItemList();
                return true;
            }
            if (item is ITargetSingleEnemy)
            {
                Entity? target = _battle.SelectFromEnemies(entity);
                if (target is null) continue;
                ((ITargetSingleEnemy)item).Target(entity, target);
                allyParty.Invetory.RemoveConsumable(item);
                ClearItemList();
                return true;
            }
        }
    }

    private void WriteGearList(Party party)
    {
        UIManager.ItemListHeader.SetList(new ConsoleCharacterString("Gear", ConsoleColor.DarkYellow));
        UIManager.ItemList.SetList(party.Invetory.GetGear().Select(o => o.Name).ToArray());
    }

    private void ClearItemList()
    {
        UIManager.ItemList.ClearList();
        UIManager.ItemListHeader.ClearList();
    }

    private void WriteConsumableList(Party party)
    {
        UIManager.ItemListHeader.SetList(new ConsoleCharacterString("Items", ConsoleColor.DarkYellow));

        var itemGroups = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).OrderBy(o => o.Key);
        var groupsWithCount = itemGroups.Select(group => new { Name = group.Key, Count = group.Count() }).OrderBy(o => o.Name);
        var groupColors = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).Select(o => o.First()).OrderBy(o => o.Name.Text);

        for (int i = 0; i < groupsWithCount.Count(); i++)
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(
                groupsWithCount.ElementAt(i).Name,
                groupColors.ElementAt(i).Name.Foreground);
            builder.AddString($" x{groupsWithCount.ElementAt(i).Count}");
            UIManager.ItemList.AddEntry(builder.Build());
        }
    }

    private Consumable SelectFromConsumableList(Party party, int index)
    {
        var itemGroups = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).OrderBy(o => o.Key);
        var groupsWithCount = itemGroups.Select(group => new { Name = group.Key, Count = group.Count() }).OrderBy(o => o.Name);
        var groupColors = party.Invetory.GetConsumables().GroupBy(o => o.Name.Text).Select(o => o.First()).OrderBy(o => o.Name.Text);

        string name = groupsWithCount.ElementAt(index).Name;
        return party.Invetory.GetConsumables().Where(o => o.Name.Text == name).Select(o => o).First();
    }
}