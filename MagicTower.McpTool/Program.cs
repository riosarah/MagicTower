using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

app.MapGet("/", () => "MagicTower MCP Server - Ready! MCP endpoint available at /mcp");

app.Lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "http://localhost:5000";
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("    MagicTower MCP Server gestartet!");
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine($"MCP Endpoint: {urls}/mcp");
    Console.WriteLine($"Health Check: {urls}/");
    Console.WriteLine("═══════════════════════════════════════════════════════");
});

app.Run();


[McpServerToolType]
public class GameMcpTools
{
    private static readonly Random _random = new Random();

    [McpServerTool(Name = "create_character")]
    [Description("Creates a new character with the specified name and class. Character classes: 1=Warrior (120 HP, 15 ATK), 2=Archer (100 HP, 12 ATK), 3=Druid (110 HP, 10 ATK). Returns the created character with initial stats, level 1, 0 gold, and 3 special attacks.")]
    public string CreateCharacter(
        [Description("Character name")] string name,
        [Description("Character class: 1=Warrior, 2=Archer, 3=Druid")] int characterClass)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Fehler: Name darf nicht leer sein.";

        if (characterClass < 1 || characterClass > 3)
            return "Fehler: Ungültige Charakterklasse. Wähle 1=Warrior, 2=Archer oder 3=Druid.";

        var character = new Character
        {
            Name = name,
            CharacterClass = characterClass,
            Level = 1,
            Gold = 0,
            SpecialAttackMultiplier = 1.5,
            AvailableSpecialAttacks = 3
        };

        // Set base stats based on class
        switch (characterClass)
        {
            case 1: // Warrior
                character.MaxHealth = 120;
                character.CurrentHealth = 120;
                character.AttackPower = 15;
                break;
            case 2: // Archer
                character.MaxHealth = 100;
                character.CurrentHealth = 100;
                character.AttackPower = 12;
                break;
            case 3: // Druid
                character.MaxHealth = 110;
                character.CurrentHealth = 110;
                character.AttackPower = 10;
                break;
        }

        string className = characterClass switch
        {
            1 => "Warrior",
            2 => "Archer",
            3 => "Druid",
            _ => "Unknown"
        };

        return $"✨ Charakter erstellt!\n" +
               $"Name: {character.Name}\n" +
               $"Klasse: {className}\n" +
               $"Level: {character.Level}\n" +
               $"Gesundheit: {character.CurrentHealth}/{character.MaxHealth} HP\n" +
               $"Angriffskraft: {character.AttackPower}\n" +
               $"Gold: {character.Gold}\n" +
               $"Spezialattacken: {character.AvailableSpecialAttacks}\n" +
               $"Spezial-Multiplikator: {character.SpecialAttackMultiplier}x";
    }

    [McpServerTool(Name = "start_game_session")]
    [Description("Starts a new game session with the specified difficulty. Can be called with or without a character - if no character exists yet, the AI can create one first. Difficulty levels: 10=Easy (10 floors), 20=Medium (20 floors), 30=Hard (30 floors). Returns session info with starting floor 1 and total floors based on difficulty.")]
    public string StartGameSession(
        [Description("Difficulty: 10=Easy, 20=Medium, 30=Hard")] int difficulty,
        [Description("Character name (optional, leave empty if no character exists yet)")] string? characterName = null,
        [Description("Character level (optional)")] int? characterLevel = null)
    {
        if (difficulty != 10 && difficulty != 20 && difficulty != 30)
            return "Fehler: Ungültige Schwierigkeit. Wähle 10=Easy, 20=Medium oder 30=Hard.";

        var session = new GameSession
        {
            CurrentFloor = 1,
            TotalFloors = difficulty,
            IsCompleted = false,
            StartTime = DateTime.Now
        };

        string difficultyName = difficulty switch
        {
            10 => "Easy (10 Etagen)",
            20 => "Medium (20 Etagen)",
            30 => "Hard (30 Etagen)",
            _ => "Unknown"
        };

        string characterInfo = string.IsNullOrEmpty(characterName)
            ? "Noch kein Charakter erstellt - bitte erstelle zuerst einen Charakter mit create_character!"
            : $"Charakter: {characterName} (Level {characterLevel ?? 1})";

        return $"🎮 Spiel-Session gestartet!\n" +
               $"{characterInfo}\n" +
               $"Schwierigkeit: {difficultyName}\n" +
               $"Aktuelle Etage: {session.CurrentFloor}/{session.TotalFloors}\n" +
               $"Status: Aktiv\n" +
               $"Startzeit: {session.StartTime:HH:mm:ss}";
    }

    [McpServerTool(Name = "generate_enemy")]
    [Description("Generates a new enemy for the current floor based on character level. Boss fights occur every 5 floors with 1.5x stronger stats. Returns enemy type, race, level, health, attack power, weapon, IsBoss flag, and HasSpecialAttack flag.")]
    public string GenerateEnemy(
        [Description("Character level")] int characterLevel,
        [Description("Current floor number")] int currentFloor)
    {
        if (characterLevel < 1 || characterLevel > 100)
            return "Fehler: Charakter-Level muss zwischen 1 und 100 liegen.";

        if (currentFloor < 1)
            return "Fehler: Etage muss mindestens 1 sein.";

        bool isBoss = currentFloor % 5 == 0;
        double bossMultiplier = isBoss ? 1.5 : 1.0;
        bool hasSpecialAttack = _random.Next(0, 100) < 30 || isBoss; // 30% chance or always for boss

        var enemy = new Enemy
        {
            Type = GetRandomEnemyType(),
            Race = GetRandomRace(),
            Level = characterLevel,
            Health = (int)((_random.Next(characterLevel * 10, characterLevel * 20)) * bossMultiplier),
            AttackPower = (int)((_random.Next(characterLevel * 1, characterLevel * 10)) * bossMultiplier),
            Weapon = GetRandomWeapon(),
            IsBoss = isBoss,
            HasSpecialAttack = hasSpecialAttack
        };

        string enemyInfo = isBoss ? "👑 BOSS KAMPF!" : "⚔️ Neuer Gegner!";
        string specialInfo = hasSpecialAttack ? "\n🔥 ACHTUNG: Hat Spezialattacke!" : "";

        return $"{enemyInfo}\n" +
               $"Etage: {currentFloor}\n" +
               $"Gegner: {enemy.Race} {enemy.Type} (Level {enemy.Level})\n" +
               $"Gesundheit: {enemy.Health} HP\n" +
               $"Angriffskraft: {enemy.AttackPower}\n" +
               $"Waffe: {enemy.Weapon}{specialInfo}";
    }

    [McpServerTool(Name = "execute_combat_action")]
    [Description("Executes a combat action (normal or special attack) against the current enemy. Calculates damage for both player and enemy. If enemy is defeated, awards gold (10-50 for normal, 50-150 for boss), level up chance, and possibly new weapons. Returns updated health, damage dealt, rewards, and whether to advance floor.")]
    public string ExecuteCombatAction(
        [Description("Character name")] string characterName,
        [Description("Character level")] int characterLevel,
        [Description("Character max health")] int characterMaxHealth,
        [Description("Character current health")] int characterCurrentHealth,
        [Description("Character attack power")] int characterAttackPower,
        [Description("Character gold")] int characterGold,
        [Description("Character available special attacks")] int availableSpecialAttacks,
        [Description("Special attack multiplier (typically 1.5)")] double specialAttackMultiplier,
        [Description("Use special attack (true) or normal attack (false)")] bool useSpecialAttack,
        [Description("Enemy type (e.g., Ritter, König)")] string enemyType,
        [Description("Enemy race (e.g., Ork, Goblin)")] string enemyRace,
        [Description("Enemy level")] int enemyLevel,
        [Description("Enemy current health")] int enemyHealth,
        [Description("Enemy attack power")] int enemyAttackPower,
        [Description("Enemy weapon")] string enemyWeapon,
        [Description("Is boss enemy")] bool isBoss,
        [Description("Enemy has special attack ability")] bool hasSpecialAttack,
        [Description("Current floor number")] int currentFloor)
    {
        if (characterCurrentHealth <= 0)
            return "Fehler: Charakter ist besiegt und kann nicht kämpfen.";

        if (enemyHealth <= 0)
            return "Fehler: Gegner ist bereits besiegt.";

        if (useSpecialAttack && availableSpecialAttacks <= 0)
            return "Fehler: Keine Spezialattacken verfügbar!";

        // Calculate player damage
        int playerDamage = useSpecialAttack 
            ? (int)(characterAttackPower * specialAttackMultiplier * _random.Next(5, 11))
            : _random.Next(characterAttackPower * 5, characterAttackPower * 11);

        // Enemy uses special attack randomly if available
        bool enemyUsesSpecial = hasSpecialAttack && _random.Next(0, 100) < 40; // 40% chance
        int enemyDamage = enemyUsesSpecial
            ? (int)(enemyAttackPower * 1.5 * _random.Next(5, 11))
            : _random.Next(enemyAttackPower * 5, enemyAttackPower * 11);

        int newCharacterHealth = Math.Max(0, characterCurrentHealth - enemyDamage);
        int newEnemyHealth = Math.Max(0, enemyHealth - playerDamage);
        int newAvailableSpecialAttacks = useSpecialAttack ? availableSpecialAttacks - 1 : availableSpecialAttacks;

        string attackType = useSpecialAttack ? "🔥 SPEZIALATTACKE" : "⚔️ Normale Attacke";
        string enemyAttackType = enemyUsesSpecial ? "🔥 Gegner SPEZIALATTACKE" : "⚔️ Gegner Attacke";
        
        string result = $"{attackType}!\n" +
                       $"{characterName} verursacht {playerDamage} Schaden!\n" +
                       $"{enemyAttackType}!\n" +
                       $"{enemyRace} {enemyType} verursacht {enemyDamage} Schaden!\n\n" +
                       $"{characterName}: {newCharacterHealth}/{characterMaxHealth} HP\n" +
                       $"{enemyRace} {enemyType}: {newEnemyHealth}/{enemyHealth} HP\n" +
                       $"Spezialattacken übrig: {newAvailableSpecialAttacks}";

        // Check combat outcome
        if (newEnemyHealth == 0)
        {
            int goldReward = isBoss ? _random.Next(50, 151) : _random.Next(10, 51);
            int newGold = characterGold + goldReward;
            bool levelUp = _random.Next(0, 100) < 40; // 40% chance
            int newLevel = levelUp ? characterLevel + 1 : characterLevel;
            int newMaxHealth = characterMaxHealth;
            int newAttackPower = characterAttackPower;

            if (levelUp)
            {
                newMaxHealth += 10;
                newAttackPower += 2;
            }

            // Weapon drop chance
            bool weaponDrop = _random.Next(0, 100) < (isBoss ? 60 : 20); // 60% boss, 20% normal
            string weaponInfo = weaponDrop ? $"\n⚔️ Neue Waffe gefunden: {GetRandomWeapon()} (+{_random.Next(3, 8)} Schaden, Wert: {_random.Next(20, 101)} Gold)" : "";

            result += $"\n\n🏆 {characterName} ist siegreich!\n" +
                     $"💰 Gold erhalten: +{goldReward} (Total: {newGold})\n" +
                     $"Spezialattacken aufgefüllt: +3 (Total: {newAvailableSpecialAttacks + 3})";

            if (levelUp)
                result += $"\n⭐ LEVEL UP! {characterLevel} → {newLevel}\n" +
                         $"   HP: {characterMaxHealth} → {newMaxHealth}\n" +
                         $"   ATK: {characterAttackPower} → {newAttackPower}";

            result += weaponInfo;
            result += $"\n\n➡️ Weiter zur nächsten Etage: {currentFloor + 1}";
        }
        else if (newCharacterHealth == 0)
        {
            result += $"\n\n💀 {characterName} wurde besiegt!\n" +
                     $"Spiel beendet auf Etage {currentFloor}.";
        }

        return result;
    }

    [McpServerTool(Name = "heal_character")]
    [Description("Heals the character by spending gold. Cost: 20 gold per heal. Restores 30% of max health. Returns new health and remaining gold.")]
    public string HealCharacter(
        [Description("Character name")] string characterName,
        [Description("Character max health")] int characterMaxHealth,
        [Description("Character current health")] int characterCurrentHealth,
        [Description("Character gold")] int characterGold)
    {
        const int healCost = 20;
        if (characterGold < healCost)
            return $"Fehler: Nicht genug Gold! Benötigt: {healCost}, Verfügbar: {characterGold}";

        if (characterCurrentHealth >= characterMaxHealth)
            return "Fehler: Charakter hat bereits volle Gesundheit!";

        int healAmount = (int)(characterMaxHealth * 0.3);
        int newHealth = Math.Min(characterMaxHealth, characterCurrentHealth + healAmount);
        int newGold = characterGold - healCost;

        return $"💚 {characterName} wurde geheilt!\n" +
               $"Heilung: +{healAmount} HP\n" +
               $"Gesundheit: {characterCurrentHealth} → {newHealth}/{characterMaxHealth}\n" +
               $"Gold: {characterGold} → {newGold} (-{healCost})";
    }

    [McpServerTool(Name = "buy_special_attacks")]
    [Description("Purchases special attacks with gold. Cost: 30 gold for 2 special attacks. Returns new special attack count and remaining gold.")]
    public string BuySpecialAttacks(
        [Description("Character name")] string characterName,
        [Description("Character available special attacks")] int availableSpecialAttacks,
        [Description("Character gold")] int characterGold)
    {
        const int cost = 30;
        const int amount = 2;

        if (characterGold < cost)
            return $"Fehler: Nicht genug Gold! Benötigt: {cost}, Verfügbar: {characterGold}";

        int newSpecialAttacks = availableSpecialAttacks + amount;
        int newGold = characterGold - cost;

        return $"🔥 Spezialattacken gekauft!\n" +
               $"Spezialattacken: {availableSpecialAttacks} → {newSpecialAttacks} (+{amount})\n" +
               $"Gold: {characterGold} → {newGold} (-{cost})";
    }

    // Helper classes
    public class Character
    {
        public string Name { get; set; } = string.Empty;
        public int CharacterClass { get; set; }
        public int Level { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int AttackPower { get; set; }
        public int Gold { get; set; }
        public double SpecialAttackMultiplier { get; set; }
        public int AvailableSpecialAttacks { get; set; }
    }

    public class GameSession
    {
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class Enemy
    {
        public string Type { get; set; } = string.Empty;
        public string Race { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Health { get; set; }
        public int AttackPower { get; set; }
        public string Weapon { get; set; } = string.Empty;
        public bool IsBoss { get; set; }
        public bool HasSpecialAttack { get; set; }
    }

    // Helper methods
    private string GetRandomEnemyType()
    {
        string[] enemyTypes = { "Ritter", "König", "Prinz", "Dieb", "Assassine", "Plünderer", "Krieger", "Berserker" };
        return enemyTypes[_random.Next(enemyTypes.Length)];
    }

    private string GetRandomWeapon()
    {
        string[] weapons = { "Schwert", "Axt", "Bogen", "Dolch", "Stab", "Keule", "Speer", "Armbrust", "Zauberstab" };
        return weapons[_random.Next(weapons.Length)];
    }

    private string GetRandomRace()
    {
        string[] races = { "Ork", "Goblin", "Troll", "Wolf", "Drache", "Riese", "Hydra" };
        return races[_random.Next(races.Length)];
    }
}
