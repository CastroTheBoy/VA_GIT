public class BattleManager
{
    private Party _leftParty;
    private Party _rightParty;

    public BattleManager(Party leftParty, Party rightParty)
    {
        _rightParty = rightParty;
        _leftParty = leftParty;
    }

    public Party DoBattle()
    {
        Party? winningParty;
        while (true)
        {
            foreach (Entity entity in _leftParty.Entities)
            {
                UpdateDisplayState(
                    _leftParty.Controller == PlayerType.User ? entity : null
                    );
                ChooseAndExecuteAction(entity, _leftParty, _rightParty);
                UpdateDisplayState(
                    _leftParty.Controller == PlayerType.User ? entity : null
                    );
                winningParty = CheckParties();
                if (!(winningParty == null))
                {
                    RemoveLosingParty(winningParty);
                    return winningParty;
                }
            }
            foreach (Entity entity in _rightParty.Entities)
            {
                UpdateDisplayState(
                    _rightParty.Controller == PlayerType.User ? entity : null
                    );
                ChooseAndExecuteAction(entity, _rightParty, _leftParty);
                UpdateDisplayState(
                    _rightParty.Controller == PlayerType.User ? entity : null
                    );
                winningParty = CheckParties();
                if (!(winningParty == null))
                {
                    RemoveLosingParty(winningParty);
                    return winningParty;
                }
            }
        }
    }

    private void UpdateDisplayState(Entity? entity = null)
    {
        UIManager.LeftPartyHeader.SetList(_leftParty.Name);
        UIManager.LeftParty.ClearList();
        foreach (Entity ent in _leftParty.Entities)
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(ent == entity ? "> " : "");
            builder.AddString(ent.Name);
            builder.AddString(ent.HasGear() ? $" ({ent.GetGear().Name}) " : " ");
            builder.AddString(ent.HPAmount());
            UIManager.LeftParty.AddEntry(builder.Build());
        }

        UIManager.RightPartyHeader.SetList(_rightParty.Name);
        UIManager.RightParty.ClearList();
        foreach (Entity ent in _rightParty.Entities)
        {
            ConsoleCharacterBuilder builder = new();
            builder.AddString(ent == entity ? "> " : "");
            builder.AddString(ent.Name);
            builder.AddString(ent.HasGear() ? $" ({ent.GetGear().Name}) " : " ");
            builder.AddString(ent.HPAmount());
            UIManager.RightParty.AddEntry(builder.Build());
        }
    }

    private void RemoveLosingParty(Party winner)
    {
        if (winner == _leftParty)
            PartyManager.Instance.RemoveParty(_rightParty);
        else
            PartyManager.Instance.RemoveParty(_leftParty);
    }

    private Party? CheckParties()
    {
        if (!IsPartyAlive(_leftParty)) return _rightParty;
        if (!IsPartyAlive(_rightParty)) return _leftParty;
        return null;
    }
    private bool IsPartyAlive(Party party) => party.Entities.Count > 0;

    private void ChooseAndExecuteAction(Entity entity, Party allyParty, Party enemyParty)
    {
        IActionManager? actionManager = null;
        switch (allyParty.Controller)
        {
            case PlayerType.User:
                actionManager = new PlayerActionManager(this);
                break;
            case PlayerType.Robot:
                actionManager = new RobotActionManager(this);
                break;
            default:
                throw new NotSupportedException("Unrecognized PlayerType detected in BattleManager!");
        }
        actionManager.DoAction(entity);
    }

    public Party GetAllyParty(Entity entity)
    {
        if (_leftParty.Entities.Contains(entity))
            return _leftParty;
        if (_rightParty.Entities.Contains(entity))
            return _rightParty;
        throw new Exception("Entity was not found in any party belonging to battle manager!");
    }
    public Party GetEnemyParty(Entity entity)
    {
        if (_leftParty.Entities.Contains(entity))
            return _rightParty;
        if (_rightParty.Entities.Contains(entity))
            return _leftParty;
        throw new Exception("Entity was not found in any party belonging to battle manager!");
    }

    public Entity? SelectFromAllies(Entity entity)
    {
        if (_leftParty.Entities.Contains(entity))
        {
            int index = UIManager.LeftParty.SelectFromList();
            if (index < 0) return null;
            return _leftParty.Entities[index];
        }
        if (_rightParty.Entities.Contains(entity))
        {
            int index = UIManager.RightParty.SelectFromList();
            if (index < 0) return null;
            return _rightParty.Entities[index];
        }
        throw new Exception("Entity was not found in any party belonging to battle manager!");
    }
    public Entity? SelectFromEnemies(Entity entity)
    {
        if (_leftParty.Entities.Contains(entity))
        {
            int index = UIManager.RightParty.SelectFromList();
            if (index < 0) return null;
            return _rightParty.Entities[index];
        }
        if (_rightParty.Entities.Contains(entity))
        {
            int index = UIManager.LeftParty.SelectFromList();
            if (index < 0) return null;
            return _leftParty.Entities[index];
        }
        throw new Exception("Entity was not found in any party belonging to battle manager!");
    }
}
