public class PartyManager
{
    private Dictionary<string, Party> _parties = new Dictionary<string, Party>();

    private PartyManager() { }
    private static PartyManager? _instance = null;
    public static PartyManager Instance
    {
        get
        {
            if( _instance == null )
            {
                _instance = new PartyManager();
                return _instance;
            }
            return _instance;
        }
    }

    public bool PartyExists(string name)
    {
        return _parties.ContainsKey(name);
    }
    public void AddParty(string name, PlayerType controller)
    {
        _parties.Add(name, new Party(name, controller));
    }
    public void AddParty(string name, string partyName, PlayerType controller)
    {
        _parties.Add(name, new Party(partyName, controller));
    }
    public void RemoveParty(string name)
    {
        _parties.Remove(name);
    }
    public void RemoveParty(Party party)
    {
        _parties.Remove(party.Name);
    }
    public Party GetParty(string name)
    {
        return _parties[name];
    }
    public Party GetParty(Party party)
    {
        return _parties[party.Name];
    }
}

public class Party
{
    private Dictionary<Guid, Entity> _party = new Dictionary<Guid, Entity>();
    public string Name { get; init; }
    public PlayerType Controller { get; init; }
    public List<Entity> Entities
    {
        get => _party.Values.ToList();
    }
    public Inventory Invetory { get; } = new Inventory();

    public Party(string name, PlayerType controller)
    {
        Name = name;
        Controller = controller;
    }

    public void AddEntity(params Entity[] entities)
    {
        foreach (Entity entity in entities)
        {
            entity.EntityDied += OnEntityDeath;
            _party.Add(entity.UniqueID, entity);
        }
    }
    public void RemoveEntity(params Entity[] entities)
    {
        foreach (Entity entity in entities)
            _party.Remove(entity.UniqueID);
    }
    private void OnEntityDeath(Entity entity)
    {
        _party.Remove(entity.UniqueID);
    }
}