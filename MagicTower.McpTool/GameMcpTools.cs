//using ModelContextProtocol.Server;
//using System.ComponentModel;
//using System.Text.Json;

//[McpServerToolType]
//public class GameMcpTools
//{
//    private readonly IHttpClientFactory _httpClientFactory;
//    private const string BaseUrl = "http://localhost:5000/api/GameMcp";

//    public GameMcpTools(IHttpClientFactory httpClientFactory)
//    {
//        _httpClientFactory = httpClientFactory;
//    }

//    [McpServerTool(Name = "create_character")]
//    [Description("Creates a new character with the specified name and class. Character classes: 1=Warrior (120 HP, 15 ATK), 2=Archer (100 HP, 12 ATK), 3=Druid (110 HP, 10 ATK). Returns the created character with ID and stats.")]
//    public async Task<string> CreateCharacter(
//        [Description("Character name")] string name,
//        [Description("Character class: 1=Warrior, 2=Archer, 3=Druid")] int characterClass)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/character/create",
//                new { Name = name, CharacterClass = characterClass }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Erstellen des Charakters: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "get_character")]
//    [Description("Retrieves the current status and stats of a character by ID. Returns level, health, attack power, gold, special attack multiplier, and available special attacks.")]
//    public async Task<string> GetCharacter(
//        [Description("Character ID")] int characterId)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.GetAsync($"{BaseUrl}/character/{characterId}");

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Abrufen des Charakters: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "start_session")]
//    [Description("Starts a new game session with the specified difficulty. Difficulty levels: 10=Easy (10 floors), 20=Medium (20 floors), 30=Hard (30 floors). Returns session ID and initial floor status.")]
//    public async Task<string> StartSession(
//        [Description("Character ID")] int characterId,
//        [Description("Difficulty: 10=Easy, 20=Medium, 30=Hard")] int difficulty)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/session/start",
//                new { CharacterId = characterId, Difficulty = difficulty }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Starten der Session: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "generate_enemy")]
//    [Description("Generates a new enemy for the current floor based on character level. Boss fights occur every 5 floors with 1.5x stronger stats. Returns enemy type, race, level, health, attack power, weapon, and special abilities.")]
//    public async Task<string> GenerateEnemy(
//        [Description("Game session ID")] int gameSessionId,
//        [Description("Character ID")] int characterId)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/fight/start",
//                new { GameSessionId = gameSessionId, CharacterId = characterId }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Generieren des Gegners: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "execute_combat_action")]
//    [Description("Executes a combat action (normal or special attack) against the current enemy. Calculates damage for both player and enemy. If enemy is defeated, awards gold, level up, and possibly new weapons. Returns updated health, damage dealt, rewards, and floor advancement status.")]
//    public async Task<string> ExecuteCombatAction(
//        [Description("Game session ID")] int gameSessionId,
//        [Description("Character ID")] int characterId,
//        [Description("Use special attack (true) or normal attack (false)")] bool useSpecialAttack,
//        [Description("Enemy type (e.g., Ork, Goblin, Troll)")] string enemyType,
//        [Description("Enemy race (e.g., Ritter, König, Krieger)")] string enemyRace,
//        [Description("Enemy level")] int enemyLevel,
//        [Description("Enemy current health")] int enemyHealth,
//        [Description("Enemy attack power")] int enemyAttackPower,
//        [Description("Enemy weapon")] string enemyWeapon,
//        [Description("Is boss enemy")] bool isBoss,
//        [Description("Enemy has special attack ability")] bool hasSpecialAttack)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var enemy = new
//            {
//                Type = enemyType,
//                Race = enemyRace,
//                Level = enemyLevel,
//                Health = enemyHealth,
//                AttackPower = enemyAttackPower,
//                Weapon = enemyWeapon,
//                IsBoss = isBoss,
//                HasSpecialAttack = hasSpecialAttack
//            };

//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/fight/action",
//                new
//                {
//                    GameSessionId = gameSessionId,
//                    CharacterId = characterId,
//                    UseSpecialAttack = useSpecialAttack,
//                    Enemy = enemy
//                }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Ausführen der Kampfaktion: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "get_weapons")]
//    [Description("Retrieves all weapons owned by a character. Returns weapon ID, name, damage bonus, upgrade level (0-5), gold value, suitable class, and equipped status.")]
//    public async Task<string> GetWeapons(
//        [Description("Character ID")] int characterId)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.GetAsync($"{BaseUrl}/weapon/character/{characterId}");

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Abrufen der Waffen: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "upgrade_weapon")]
//    [Description("Upgrades a weapon to the next level (max level 5). Costs increase exponentially: Level 0?1: 50 gold, 1?2: 100, 2?3: 200, 3?4: 400, 4?5: 800. Each upgrade increases damage bonus by 1. Returns updated weapon stats.")]
//    public async Task<string> UpgradeWeapon(
//        [Description("Weapon ID to upgrade")] int weaponId,
//        [Description("Character ID (owner)")] int characterId)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/weapon/upgrade",
//                new { WeaponId = weaponId, CharacterId = characterId }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Aufwerten der Waffe: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }

//    [McpServerTool(Name = "sell_weapon")]
//    [Description("Sells a weapon for gold. Cannot sell equipped weapons. Gold received equals the weapon's current gold value (base value + upgrade bonuses). Returns gold received and new total gold.")]
//    public async Task<string> SellWeapon(
//        [Description("Weapon ID to sell")] int weaponId,
//        [Description("Character ID (owner)")] int characterId)
//    {
//        try
//        {
//            var httpClient = _httpClientFactory.CreateClient();
//            var response = await httpClient.PostAsJsonAsync(
//                $"{BaseUrl}/weapon/sell",
//                new { WeaponId = weaponId, CharacterId = characterId }
//            );

//            if (response.IsSuccessStatusCode)
//            {
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

//            return $"Fehler beim Verkaufen der Waffe: {response.StatusCode}";
//        }
//        catch (Exception ex)
//        {
//            return $"Fehler: {ex.Message}";
//        }
//    }
//}
