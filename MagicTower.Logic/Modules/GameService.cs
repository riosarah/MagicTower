#if GENERATEDCODE_ON
//@CustomCode
using MagicTower.Logic.Contracts;
using MagicTower.Logic.Entities.Game;
using MagicTower.Common.Models.Game;
using Microsoft.EntityFrameworkCore;

namespace MagicTower.Logic.Modules
{
    /// <summary>
    /// Service for managing game logic and operations.
    /// </summary>
    public class GameService
    {
        internal readonly IContext _context;
        private static readonly Random _random = new Random();

        public GameService(IContext context)
        {
            _context = context;
        }

        #region Character Management

        /// <summary>
        /// Creates a new character with initial stats based on class.
        /// </summary>
        public async Task<Character> CreateCharacterAsync(string name, CharacterClass characterClass)
        {
            var character = new Character
            {
                Name = name,
                Class = characterClass,
                Level = 1,
                MaxHealth = GetInitialHealth(characterClass),
                AttackPower = GetInitialAttackPower(characterClass),
                SpecialAttackLevel = 1,
                Gold = 0
            };
            
            character.CurrentHealth = character.MaxHealth;

            var characterSet = _context.CharacterSet;
            await characterSet.AddAsync(character);
            await _context.SaveChangesAsync();
            
            return character;
        }

        /// <summary>
        /// Gets the initial health based on character class.
        /// </summary>
        private int GetInitialHealth(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => 120,
                CharacterClass.Archer => 100,
                CharacterClass.Druid => 110,
                _ => 100
            };
        }

        /// <summary>
        /// Gets the initial attack power based on character class.
        /// </summary>
        private int GetInitialAttackPower(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => 15,
                CharacterClass.Archer => 12,
                CharacterClass.Druid => 10,
                _ => 10
            };
        }

        /// <summary>
        /// Levels up a character after defeating an enemy.
        /// </summary>
        public async Task LevelUpCharacterAsync(IdType characterId)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            character.Level++;
            character.MaxHealth += GetHealthIncreasePerLevel(character.Class);
            character.AttackPower += GetAttackPowerIncreasePerLevel(character.Class);
            character.CurrentHealth = character.MaxHealth; // Full heal on level up
            
            await characterSet.UpdateAsync(character.Id, character);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets health increase per level based on character class.
        /// </summary>
        private int GetHealthIncreasePerLevel(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => 10,
                CharacterClass.Archer => 8,
                CharacterClass.Druid => 9,
                _ => 8
            };
        }

        /// <summary>
        /// Gets attack power increase per level based on character class.
        /// </summary>
        private int GetAttackPowerIncreasePerLevel(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => 3,
                CharacterClass.Archer => 2,
                CharacterClass.Druid => 2,
                _ => 2
            };
        }

        /// <summary>
        /// Upgrades the character's special attack level after a boss fight.
        /// </summary>
        public async Task UpgradeSpecialAttackAsync(IdType characterId)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            character.SpecialAttackLevel++;
            
            await characterSet.UpdateAsync(character.Id, character);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds gold to a character.
        /// </summary>
        public async Task AddGoldAsync(IdType characterId, int amount)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            character.Gold += amount;
            
            await characterSet.UpdateAsync(character.Id, character);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates total attack power including equipped weapons.
        /// </summary>
        public async Task<int> GetTotalAttackPowerAsync(IdType characterId)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            var weaponSet = _context.WeaponSet;
            var weapons = await weaponSet.GetAllAsync();
            var equippedWeapons = weapons.Where(w => w.CharacterId == characterId && w.IsEquipped).ToList();
            
            int totalDamageBonus = equippedWeapons.Sum(w => w.DamageBonus + (w.UpgradeLevel * 5));
            
            return character.AttackPower + totalDamageBonus;
        }

        #endregion

        #region Game Session Management

        /// <summary>
        /// Creates a new game session.
        /// </summary>
        public async Task<GameSession> CreateGameSessionAsync(IdType characterId, Difficulty difficulty)
        {
            var gameSession = new GameSession
            {
                CharacterId = characterId,
                Difficulty = difficulty,
                CurrentFloor = 1,
                MaxFloor = (int)difficulty,
                IsCompleted = false,
                StartedAt = DateTime.UtcNow
            };

            var sessionSet = _context.GameSessionSet;
            await sessionSet.AddAsync(gameSession);
            await _context.SaveChangesAsync();
            
            return gameSession;
        }

        /// <summary>
        /// Advances the game session to the next floor.
        /// </summary>
        public async Task AdvanceFloorAsync(IdType gameSessionId)
        {
            var sessionSet = _context.GameSessionSet;
            var session = await sessionSet.GetByIdAsync(gameSessionId);
            
            if (session == null)
                throw new InvalidOperationException($"GameSession with ID {gameSessionId} not found.");

            session.CurrentFloor++;
            
            if (session.CurrentFloor > session.MaxFloor)
            {
                session.IsCompleted = true;
                session.CompletedAt = DateTime.UtcNow;
            }
            
            await sessionSet.UpdateAsync(session.Id, session);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Combat System

        /// <summary>
        /// Generates a random enemy for the specified floor.
        /// </summary>
        public EnemyData GenerateEnemy(int floor, bool isBoss)
        {
            var enemyLevel = floor;
            var multiplier = isBoss ? 1.5 : 1.0;

            return new EnemyData
            {
                Type = GetRandomEnemyType(),
                Race = GetRandomRace(),
                Level = enemyLevel,
                Health = (int)(enemyLevel * 15 * multiplier),
                AttackPower = (int)(enemyLevel * 8 * multiplier),
                Weapon = GetRandomWeapon(),
                IsBoss = isBoss,
                HasSpecialAttack = isBoss
            };
        }

        /// <summary>
        /// Simulates a combat round.
        /// </summary>
        public async Task<CombatResult> SimulateCombatRoundAsync(IdType characterId, EnemyData enemy, bool useSpecialAttack)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            int totalAttackPower = await GetTotalAttackPowerAsync(characterId);
            
            // Calculate damage
            int damageToEnemy = _random.Next(totalAttackPower, totalAttackPower * 2);
            
            if (useSpecialAttack)
            {
                double specialMultiplier = GetSpecialAttackMultiplier(character.Class, character.SpecialAttackLevel);
                damageToEnemy = (int)(damageToEnemy * specialMultiplier);
            }

            int damageToCharacter = _random.Next(enemy.AttackPower, enemy.AttackPower * 2);
            
            // Boss special attack (30% chance)
            if (enemy.IsBoss && _random.Next(100) < 30)
            {
                damageToCharacter = (int)(damageToCharacter * 1.5);
            }

            // Apply damage
            enemy.Health = Math.Max(0, enemy.Health - damageToEnemy);
            character.CurrentHealth = Math.Max(0, character.CurrentHealth - damageToCharacter);
            
            await characterSet.UpdateAsync(character.Id, character);
            await _context.SaveChangesAsync();

            return new CombatResult
            {
                DamageToEnemy = damageToEnemy,
                DamageToCharacter = damageToCharacter,
                RemainingEnemyHealth = enemy.Health,
                RemainingCharacterHealth = character.CurrentHealth,
                IsEnemyDefeated = enemy.Health == 0,
                IsCharacterDefeated = character.CurrentHealth == 0,
                SpecialAttackUsed = useSpecialAttack,
                BossSpecialAttackUsed = enemy.IsBoss && damageToCharacter > enemy.AttackPower * 2
            };
        }

        /// <summary>
        /// Records a defeated enemy and awards rewards.
        /// </summary>
        public async Task RecordDefeatedEnemyAsync(IdType gameSessionId, int floorNumber, EnemyData enemy)
        {
            var defeatedEnemy = new DefeatedEnemy
            {
                GameSessionId = gameSessionId,
                FloorNumber = floorNumber,
                IsBoss = enemy.IsBoss,
                EnemyType = enemy.Type,
                EnemyRace = enemy.Race,
                EnemyLevel = enemy.Level,
                EnemyWeapon = enemy.Weapon,
                GoldReward = CalculateGoldReward(enemy.Level, enemy.IsBoss),
                DefeatedAt = DateTime.UtcNow
            };

            var defeatedEnemySet = _context.DefeatedEnemySet;
            await defeatedEnemySet.AddAsync(defeatedEnemy);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates gold reward based on enemy level and boss status.
        /// </summary>
        private int CalculateGoldReward(int level, bool isBoss)
        {
            return isBoss ? level * 50 : level * 10;
        }

        /// <summary>
        /// Gets special attack multiplier based on class and level.
        /// </summary>
        private double GetSpecialAttackMultiplier(CharacterClass characterClass, int specialAttackLevel)
        {
            double baseMultiplier = characterClass switch
            {
                CharacterClass.Warrior => 2.0,   // Wütender Schlag
                CharacterClass.Archer => 2.5,    // Präzisionsschuss
                CharacterClass.Druid => 1.8,     // Naturzorn (+ Heilung)
                _ => 1.5
            };

            return baseMultiplier + (specialAttackLevel - 1) * 0.3;
        }

        #endregion

        #region Weapon Management

        /// <summary>
        /// Generates a random weapon for the character's class.
        /// </summary>
        public async Task<Weapon> GenerateWeaponForCharacterAsync(IdType characterId)
        {
            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            var weaponType = GetWeaponTypeForClass(character.Class);
            var damageBonus = _random.Next(5, 15);
            
            var weapon = new Weapon
            {
                CharacterId = characterId,
                Name = $"{GetRandomWeaponPrefix()} {weaponType}",
                Type = weaponType,
                DamageBonus = damageBonus,
                UpgradeLevel = 0,
                SuitableForClass = character.Class,
                IsEquipped = false,
                SellValue = damageBonus * 5,
                UpgradeCost = CalculateUpgradeCost(0, damageBonus)
            };

            var weaponSet = _context.WeaponSet;
            await weaponSet.AddAsync(weapon);
            await _context.SaveChangesAsync();
            
            return weapon;
        }

        /// <summary>
        /// Equips a weapon for the character.
        /// </summary>
        public async Task EquipWeaponAsync(IdType weaponId)
        {
            var weaponSet = _context.WeaponSet;
            var weapon = await weaponSet.GetByIdAsync(weaponId);
            
            if (weapon == null)
                throw new InvalidOperationException($"Weapon with ID {weaponId} not found.");

            weapon.IsEquipped = true;
            
            await weaponSet.UpdateAsync(weapon.Id, weapon);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Unequips a weapon.
        /// </summary>
        public async Task UnequipWeaponAsync(IdType weaponId)
        {
            var weaponSet = _context.WeaponSet;
            var weapon = await weaponSet.GetByIdAsync(weaponId);
            
            if (weapon == null)
                throw new InvalidOperationException($"Weapon with ID {weaponId} not found.");

            weapon.IsEquipped = false;
            
            await weaponSet.UpdateAsync(weapon.Id, weapon);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Upgrades a weapon if the character has enough gold.
        /// </summary>
        public async Task<bool> UpgradeWeaponAsync(IdType weaponId, IdType characterId)
        {
            var weaponSet = _context.WeaponSet;
            var weapon = await weaponSet.GetByIdAsync(weaponId);
            
            if (weapon == null)
                throw new InvalidOperationException($"Weapon with ID {weaponId} not found.");

            if (weapon.UpgradeLevel >= 5)
                return false; // Max upgrade level reached

            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            if (character.Gold < weapon.UpgradeCost)
                return false; // Not enough gold

            // Deduct gold and upgrade weapon
            character.Gold -= weapon.UpgradeCost;
            weapon.UpgradeLevel++;
            weapon.UpgradeCost = CalculateUpgradeCost(weapon.UpgradeLevel, weapon.DamageBonus);
            
            await characterSet.UpdateAsync(character.Id, character);
            await weaponSet.UpdateAsync(weapon.Id, weapon);
            await _context.SaveChangesAsync();
            
            return true;
        }

        /// <summary>
        /// Sells a weapon for gold.
        /// </summary>
        public async Task SellWeaponAsync(IdType weaponId, IdType characterId)
        {
            var weaponSet = _context.WeaponSet;
            var weapon = await weaponSet.GetByIdAsync(weaponId);
            
            if (weapon == null)
                throw new InvalidOperationException($"Weapon with ID {weaponId} not found.");

            var characterSet = _context.CharacterSet;
            var character = await characterSet.GetByIdAsync(characterId);
            
            if (character == null)
                throw new InvalidOperationException($"Character with ID {characterId} not found.");

            // Add gold and remove weapon
            character.Gold += weapon.SellValue;
            
            await characterSet.UpdateAsync(character.Id, character);
            await weaponSet.RemoveAsync(weapon.Id);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates the upgrade cost based on current level and damage bonus.
        /// </summary>
        private int CalculateUpgradeCost(int currentLevel, int damageBonus)
        {
            // Cost increases with each upgrade level
            // Base cost: 30 gold per level (so every 3 fights they can upgrade)
            return (currentLevel + 1) * 30 + (damageBonus * 2);
        }

        /// <summary>
        /// Gets weapon count for a character.
        /// </summary>
        public async Task<int> GetWeaponCountAsync(IdType characterId)
        {
            var weaponSet = _context.WeaponSet;
            var weapons = await weaponSet.GetAllAsync();
            return weapons.Count(w => w.CharacterId == characterId);
        }

        /// <summary>
        /// Gets weapon type based on character class.
        /// </summary>
        private string GetWeaponTypeForClass(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => _random.Next(3) switch
                {
                    0 => "Schwert",
                    1 => "Axt",
                    _ => "Keule"
                },
                CharacterClass.Archer => _random.Next(2) switch
                {
                    0 => "Bogen",
                    _ => "Armbrust"
                },
                CharacterClass.Druid => _random.Next(2) switch
                {
                    0 => "Stab",
                    _ => "Dolch"
                },
                _ => "Schwert"
            };
        }

        /// <summary>
        /// Gets a random weapon prefix for naming.
        /// </summary>
        private string GetRandomWeaponPrefix()
        {
            string[] prefixes = { "Alter", "Verzauberter", "Legendärer", "Rostiger", "Glänzender", "Uralter", "Mächtiger", "Mystischer" };
            return prefixes[_random.Next(prefixes.Length)];
        }

        #endregion

        #region Random Generators

        private string GetRandomEnemyType()
        {
            string[] types = { "Ritter", "König", "Prinz", "Dieb", "Assassine", "Plünderer", "Krieger", "Berserker" };
            return types[_random.Next(types.Length)];
        }

        private string GetRandomRace()
        {
            string[] races = { "Ork", "Goblin", "Troll", "Untoter", "Dämon", "Drache", "Riese", "Vampir" };
            return races[_random.Next(races.Length)];
        }

        private string GetRandomWeapon()
        {
            string[] weapons = { "Schwert", "Axt", "Bogen", "Dolch", "Stab", "Keule", "Speer", "Armbrust" };
            return weapons[_random.Next(weapons.Length)];
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// Represents enemy data for combat.
    /// </summary>
    public class EnemyData
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

    /// <summary>
    /// Represents the result of a combat round.
    /// </summary>
    public class CombatResult
    {
        public int DamageToEnemy { get; set; }
        public int DamageToCharacter { get; set; }
        public int RemainingEnemyHealth { get; set; }
        public int RemainingCharacterHealth { get; set; }
        public bool IsEnemyDefeated { get; set; }
        public bool IsCharacterDefeated { get; set; }
        public bool SpecialAttackUsed { get; set; }
        public bool BossSpecialAttackUsed { get; set; }
    }

    #endregion
}
#endif
