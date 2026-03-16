
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
    [Description("Creates a new character with the specified name and class. Character classes: 1=Warrior (120 HP, 15 ATK), 2=Archer (100 HP, 12 ATK), 3=Druid (110 HP, 10 ATK). Returns the created character with initial stats, level 1, 0 gold, and 3 special attacks. If no weapon name is provided, a random weapon will be generated.")]
    public string CreateCharacter(
        [Description("Character name")] string name,
        [Description("Character class: 1=Warrior, 2=Archer, 3=Druid")] int characterClass,
        [Description("Character weapon name (optional, if null a random weapon will be generated)")] string? weaponName = null)
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

        // Generate weapon
        var weapon = CreateWeaponObject(characterClass, 1, weaponName);

        return $"✨ Charakter erstellt!\n" +
               $"Name: {character.Name}\n" +
               $"Klasse: {className}\n" +
               $"Level: {character.Level}\n" +
               $"Gesundheit: {character.CurrentHealth}/{character.MaxHealth} HP\n" +
               $"Angriffskraft: {character.AttackPower}\n" +
               $"Gold: {character.Gold}\n" +
               $"Spezialattacken: {character.AvailableSpecialAttacks}\n" +
               $"Spezial-Multiplikator: {character.SpecialAttackMultiplier}x\n" +
               $"\n⚔️ Startwaffe:\n" +
               $"Name: {weapon.Name}\n" +
               $"Typ: {weapon.Type}\n" +
               $"Schaden: +{weapon.DamageBonus}\n" +
               $"Level: {weapon.UpgradeLevel}\n" +
               $"Verkaufswert: {weapon.SellValue} Gold\n" +
               $"Upgrade-Kosten: {weapon.UpgradeCost} Gold";
    }

    [McpServerTool(Name = "start_game_session")]
    [Description("Starts a new game session with the specified difficulty. Can be called with or without a character - if no character exists yet, the AI can create one first. Difficulty levels: 10=Easy (10 floors), 20=Medium (20 floors), 30=Hard (30 floors). Returns session info with starting floor 1 and total floors based on difficulty.")]
    public string StartGameSession(
        [Description("Difficulty: 5=Easy, 10=Medium, 15=Hard")] int difficulty,
        [Description("Character name (optional, leave empty if no character exists yet)")] string? characterName = null,
        [Description("Character level (optional)")] int? characterLevel = null)
    {
        if (difficulty != 5 && difficulty != 10 && difficulty != 15)
            return "Fehler: Ungültige Schwierigkeit. Wähle 5=Easy, 10=Medium oder 15=Hard.";

        var session = new GameSession
        {
            CurrentFloor = 1,
            TotalFloors = difficulty,
            IsCompleted = false,
            StartTime = DateTime.Now
        };

        string difficultyName = difficulty switch
        {
            5 => "Easy (5 Etagen)",
            10 => "Medium (10 Etagen)",
            15 => "Hard (15 Etagen)",
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
    public string GenerateEnemy(int characterLevel, int currentFloor)
    {
        bool isBoss = currentFloor % 5 == 0;
        double bossMultiplier = isBoss ? 1.35 : 1.0;
        bool hasSpecialAttack = isBoss || _random.Next(0, 100) < 25;

        int attack = (int)(_random.Next(characterLevel * 2, characterLevel * 5 + 1) * bossMultiplier);
        int health = (int)(_random.Next(characterLevel * 18, characterLevel * 26 + 1) * bossMultiplier);

        return
            $"⚔️ Gegner: {GetRandomRace()} {GetRandomEnemyType()} (Lvl {characterLevel})\n" +
            $"HP: {health}\n" +
            $"ATK: {attack}\n" +
            $"Waffe: {GetRandomWeapon()}\n" +
            $"Boss: {isBoss}\n" +
            $"Special: {hasSpecialAttack}";
    }

    [McpServerTool(Name = "execute_combat_action")]
    [Description("Executes a combat action (normal or special attack) against the current enemy. Calculates damage for both player and enemy. Handles rewards, level ups, weapon drops and floor progression.")]
    public string ExecuteCombatAction(
    string characterName,
    int characterLevel,
    int characterMaxHealth,
    int characterCurrentHealth,
    int characterAttackPower,
    int characterGold,
    int availableSpecialAttacks,
    double specialAttackMultiplier,
    bool useSpecialAttack,
    string enemyType,
    string enemyRace,
    int enemyLevel,
    int enemyHealth,
    int enemyAttackPower,
    string enemyWeapon,
    bool isBoss,
    bool hasSpecialAttack,
    int currentFloor)
    {
        if (characterCurrentHealth <= 0)
            return "Fehler: Charakter ist besiegt und kann nicht kämpfen.";

        if (enemyHealth <= 0)
            return "Fehler: Gegner ist bereits besiegt.";

        if (useSpecialAttack && availableSpecialAttacks <= 0)
            return "Fehler: Keine Spezialattacken verfügbar!";

        // ─────────────────────────────
        // DAMAGE CALCULATION (BALANCED)
        // ─────────────────────────────

        int playerDamage = useSpecialAttack
            ? (int)(characterAttackPower * specialAttackMultiplier)
              + _random.Next(0, characterAttackPower / 2 + 1)
            : _random.Next(
                (int)(characterAttackPower * 0.9),
                (int)(characterAttackPower * 1.4) + 1
              );

        bool enemyUsesSpecial = hasSpecialAttack && _random.Next(0, 100) < 15;

        int enemyDamage = enemyUsesSpecial
            ? (int)(enemyAttackPower * 1.4)
              + _random.Next(0, enemyAttackPower / 2 + 1)
            : _random.Next(
                (int)(enemyAttackPower * 0.8),
                (int)(enemyAttackPower * 1.3) + 1
              );

        int newCharacterHealth = Math.Max(0, characterCurrentHealth - enemyDamage);
        int newEnemyHealth = Math.Max(0, enemyHealth - playerDamage);
        int newAvailableSpecialAttacks = useSpecialAttack
            ? availableSpecialAttacks - 1
            : availableSpecialAttacks;

        // ─────────────────────────────
        // COMBAT TEXT
        // ─────────────────────────────

        string result =
            $"{(useSpecialAttack ? "🔥 SPEZIALATTACKE" : "⚔️ Angriff")}!\n" +
            $"{characterName} verursacht {playerDamage} Schaden.\n" +
            $"{(enemyUsesSpecial ? "🔥 Gegner-Spezialangriff" : "⚔️ Gegner-Angriff")}!\n" +
            $"{enemyRace} {enemyType} verursacht {enemyDamage} Schaden.\n\n" +
            $"{characterName}: {newCharacterHealth}/{characterMaxHealth} HP\n" +
            $"{enemyRace} {enemyType}: {newEnemyHealth}/{enemyHealth} HP\n" +
            $"Spezialattacken übrig: {newAvailableSpecialAttacks}";

        // ─────────────────────────────
        // ENEMY DEFEATED
        // ─────────────────────────────

        if (newEnemyHealth == 0)
        {
            int goldReward = isBoss
                ? _random.Next(50, 151)
                : _random.Next(10, 51);

            int newGold = characterGold + goldReward;

            bool levelUp = _random.Next(0, 100) < 40;
            int newLevel = characterLevel;
            int newMaxHealth = characterMaxHealth;
            int newAttackPower = characterAttackPower;

            if (levelUp)
            {
                newLevel++;
                newMaxHealth += 10;
                newAttackPower += 2;
            }

            bool weaponDrop = _random.Next(0, 100) < (isBoss ? 60 : 20);
            string weaponInfo = weaponDrop
                ? $"\n⚔️ Neue Waffe gefunden: {GetRandomWeapon()} " +
                  $"(+{_random.Next(3, 8)} Schaden, Wert: {_random.Next(20, 101)} Gold)"
                : "";

            result +=
                $"\n\n🏆 {characterName} ist siegreich!\n" +
                $"💰 Gold erhalten: +{goldReward} (Total: {newGold})\n" +
                $"🔥 Spezialattacken aufgefüllt: +3 (Total: {newAvailableSpecialAttacks + 3})";

            if (levelUp)
            {
                result +=
                    $"\n⭐ LEVEL UP! {characterLevel} → {newLevel}\n" +
                    $"HP: {characterMaxHealth} → {newMaxHealth}\n" +
                    $"ATK: {characterAttackPower} → {newAttackPower}";
            }

            result += weaponInfo;
            result += $"\n\n➡️ Weiter zur nächsten Etage: {currentFloor + 1}";
        }
        // ─────────────────────────────
        // PLAYER DEFEATED
        // ─────────────────────────────
        else if (newCharacterHealth == 0)
        {
            result +=
                $"\n\n💀 {characterName} wurde besiegt!\n" +
                $"Spiel beendet auf Etage {currentFloor}.";
        }

        return result;
    }


    [McpServerTool(Name = "heal_character")]
    public string HealCharacter(string characterName, int characterMaxHealth, int characterCurrentHealth, int characterGold)
    {
        if (characterGold < 10)
            return "Fehler: Nicht genug Gold.";

        int heal = (int)(characterMaxHealth * (_random.Next(4,10)/10));
        int newHealth = Math.Min(characterMaxHealth, characterCurrentHealth + heal);

        return $"💚 {characterName} heilt sich um {newHealth} HP auf {characterCurrentHealth+newHealth}.";
    }

    [McpServerTool(Name = "buy_special_attacks")]
    [Description("Purchases special attacks with gold. Cost: 10 gold for 2 special attacks. Returns new special attack count and remaining gold.")]
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

    [McpServerTool(Name = "generate_weapon")]
    [Description("Generates a weapon with all status values for the character. Returns weapon details including name, type, damage bonus, upgrade level, sell value, and upgrade cost.")]
    public string GenerateWeapon(
        [Description("Character class: 1=Warrior, 2=Archer, 3=Druid")] int characterClass,
        [Description("Character level for scaling weapon stats")] int characterLevel,
        [Description("Optional weapon name, if null a random name will be generated")] string? weaponName = null)
    {
        var weapon = CreateWeaponObject(characterClass, characterLevel, weaponName);

        return $"⚔️ Waffe generiert!\n" +
               $"Name: {weapon.Name}\n" +
               $"Typ: {weapon.Type}\n" +
               $"Schaden: +{weapon.DamageBonus}\n" +
               $"Upgrade-Level: {weapon.UpgradeLevel}\n" +
               $"Geeignet für: {GetClassNameFromInt(weapon.SuitableForClass)}\n" +
               $"Verkaufswert: {weapon.SellValue} Gold\n" +
               $"Upgrade-Kosten: {weapon.UpgradeCost} Gold";
    }

    [McpServerTool(Name = "get_reward")]
    [Description("Generates a reward after defeating an enemy. Returns either gold (60% chance) or a new weapon with all stats (40% chance). Boss enemies have higher gold rewards and better weapon drops.")]
    public string GetReward(
        [Description("Character class: 1=Warrior, 2=Archer, 3=Druid")] int characterClass,
        [Description("Character level for scaling rewards")] int characterLevel,
        [Description("Whether the defeated enemy was a boss")] bool isBoss)
    {
        int rewardType = _random.Next(0, 100);
        
        // 60% chance for gold, 40% chance for weapon
        if (rewardType < 60)
        {
            // Gold reward
            int baseGold = isBoss ? 50 : 20;
            int goldReward = _random.Next(baseGold, baseGold * 3 + 1) + (characterLevel * 5);
            
            return $"💰 Gold Belohnung!\n" +
                   $"Erhalten: {goldReward} Gold\n" +
                   $"Typ: {(isBoss ? "Boss-Belohnung" : "Standard-Belohnung")}";
        }
        else
        {
            // Weapon reward
            var weapon = CreateWeaponObject(characterClass, characterLevel, null);
            
            // Boss weapons get a bonus
            if (isBoss)
            {
                weapon.DamageBonus += _random.Next(3, 8);
                weapon.SellValue += _random.Next(20, 51);
            }
            
            return $"⚔️ Waffen-Belohnung!\n" +
                   $"Gefunden: {weapon.Name}\n" +
                   $"Typ: {weapon.Type}\n" +
                   $"Schaden: +{weapon.DamageBonus}\n" +
                   $"Upgrade-Level: {weapon.UpgradeLevel}\n" +
                   $"Geeignet für: {GetClassNameFromInt(weapon.SuitableForClass)}\n" +
                   $"Verkaufswert: {weapon.SellValue} Gold\n" +
                   $"Upgrade-Kosten: {weapon.UpgradeCost} Gold\n" +
                   $"Typ: {(isBoss ? "Boss-Drop" : "Standard-Drop")}";
        }
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

    public class Weapon
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int DamageBonus { get; set; }
        public int UpgradeLevel { get; set; }
        public int SuitableForClass { get; set; }
        public bool IsEquipped { get; set; }
        public int SellValue { get; set; }
        public int UpgradeCost { get; set; }
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

    private Weapon CreateWeaponObject(int characterClass, int characterLevel, string? weaponName)
    {
        string weaponType = GetWeaponTypeForClass(characterClass);
        string name = weaponName ?? $"{GetRandomWeaponPrefix()} {weaponType}";
        
        int baseDamage = characterClass switch
        {
            1 => 5,  // Warrior - highest base damage
            2 => 4,  // Archer - medium base damage
            3 => 3,  // Druid - lowest base damage
            _ => 3
        };
        
        int damageBonus = baseDamage + _random.Next(0, characterLevel + 1);
        int upgradeLevel = 0;
        int sellValue = 10 + (damageBonus * 5);
        int upgradeCost = 50 + (characterLevel * 10);
        
        return new Weapon
        {
            Name = name,
            Type = weaponType,
            DamageBonus = damageBonus,
            UpgradeLevel = upgradeLevel,
            SuitableForClass = characterClass,
            IsEquipped = true,
            SellValue = sellValue,
            UpgradeCost = upgradeCost
        };
    }

    private string GetWeaponTypeForClass(int characterClass)
    {
        return characterClass switch
        {
            1 => GetRandomFromArray(new[] { "Schwert", "Axt", "Keule", "Speer" }),  // Warrior weapons
            2 => GetRandomFromArray(new[] { "Bogen", "Armbrust", "Dolch" }),        // Archer weapons
            3 => GetRandomFromArray(new[] { "Stab", "Zauberstab", "Dolch" }),       // Druid weapons
            _ => "Schwert"
        };
    }

    private string GetRandomWeaponPrefix()
    {
        string[] prefixes = { "Legendäres", "Episches", "Seltenes", "Magisches", "Verzaubertes", 
                             "Uraltes", "Mystisches", "Göttliches", "Dunkles", "Heiliges" };
        return prefixes[_random.Next(prefixes.Length)];
    }

    private string GetRandomFromArray(string[] array)
    {
        return array[_random.Next(array.Length)];
    }

    private string GetClassNameFromInt(int characterClass)
    {
        return characterClass switch
        {
            1 => "Warrior",
            2 => "Archer",
            3 => "Druid",
            _ => "Unknown"
        };
    }
}
