// Console formatting
Console.Title = "The Final Battle";
Console.Clear();
Console.CursorVisible = false;

// Code
Console.SetWindowSize(120, 30);

Thread uiDrawer = new Thread(UIManager.Draw);
uiDrawer.Start();

UIManager.ActivePane = UIManager.GameScreens["Start"];
Thread.Sleep(50); // Make sure uiDrawer has had time to render atleast once
UIManager.DrawScreen = false;
Console.SetCursorPosition(30, 17);
Console.ForegroundColor = ConsoleColor.White;
Console.Write("What should we call You?: ");
string? name = Console.ReadLine();
name = string.IsNullOrEmpty(name) ? "Player" : name;

GameManager game = new GameManager(PlayerType.User, PlayerType.Robot, name);

UIManager.ActivePane = UIManager.GameScreens["Game"];
UIManager.DrawScreen = true;
game.Run();
UIManager.Exit();

public class GameManager
{
    public PlayerType PlayerPartyMode { get; }
    public PlayerType EnemyPartyMode { get; }
    private int _currentRound = 1;
    private int _maxRounds = 3;
    private string _playerName;

    public GameManager(PlayerType ally, PlayerType enemy, string playerName)
    {
        _playerName = playerName;
        PlayerPartyMode = ally;
        EnemyPartyMode = enemy;
        SetupParties();
    }

    public void SetupParties()
    {
        PartyManager parties = PartyManager.Instance;
        if (parties.PartyExists("Player"))
            parties.RemoveParty("Player");
        parties.AddParty("Player", "Adventurers", PlayerType.User);
        parties.GetParty("Player").AddEntity(EntityFactory.CreatePlayer(_playerName));
        Entity vin = EntityFactory.CreateVin();
        vin.EquipGear(Weapon.CreateVinsBow());
        parties.GetParty("Player").AddEntity(vin);
        parties.GetParty("Player").Invetory.AddConsumable(
            new HealthPotion(), new HealthPotion(), new HealthPotion(),
            new FirePotion(), new FirePotion(), new FirePotion(),
            new LightningPotion(), new LightningPotion(), new LightningPotion());
        parties.GetParty("Player").Invetory.AddConsumable(
            Weapon.CreateExcalibur(), Weapon.CreateWoodenSword());

        if (parties.PartyExists("Round1"))
            parties.RemoveParty("Round1");
        parties.AddParty("Round1", "Band of Skeletons", PlayerType.Robot);
        parties.GetParty("Round1").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton());

        if (parties.PartyExists("Round2"))
            parties.RemoveParty("Round2");
        parties.AddParty("Round2", "Calcium Avengers", PlayerType.Robot);
        parties.GetParty("Round2").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton());

        if (parties.PartyExists("Round3"))
            parties.RemoveParty("Round3");
        parties.AddParty("Round3", "Forces of Evil", PlayerType.Robot);
        parties.GetParty("Round3").AddEntity(EntityFactory.CreateSkeleton(), EntityFactory.CreateSkeleton(), EntityFactory.CreateUncodedOne());
    }

    private bool DoRound(int round)
    {
        BattleManager battle = SetupRoundBattleManager(round);
        return battle.DoBattle().Name == "Adventurers";
    }

    private BattleManager SetupRoundBattleManager(int round)
    {
        PartyManager parties = PartyManager.Instance;
        switch (round)
        {
            case 1:
                return new BattleManager(
                    parties.GetParty("Player"),
                    parties.GetParty("Round1"));
            case 2:
                return new BattleManager(
                    parties.GetParty("Player"),
                    parties.GetParty("Round2"));
            case 3:
                return new BattleManager(
                    parties.GetParty("Player"),
                    parties.GetParty("Round3"));
            default:
                throw new NotSupportedException();
        }
    }

    public void Run()
    {
        while (true)
        {
            for (; _currentRound <= _maxRounds; _currentRound++)
            {
                if (DoRound(_currentRound))
                    UIManager.GameLog.AddEntry($"All enemies have been defeated in round {_currentRound}. Proceeding to round {_currentRound + 1}/{_maxRounds}.");
                else
                {
                    UIManager.GameLog.AddEntry("You have been slain! GAME OVER...");
                    return;
                }
            }
            UIManager.ActivePane = UIManager.GameScreens["End"];
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Escape:
                    return;
                case ConsoleKey.Enter:
                    UIManager.GameLog.ClearList();
                    UIManager.ActivePane = UIManager.GameScreens["Game"];
                    _currentRound = 1;
                    SetupParties();
                    break;
            }
        }
    }
}

// ENUMS
public enum PlayerType { User, Robot }