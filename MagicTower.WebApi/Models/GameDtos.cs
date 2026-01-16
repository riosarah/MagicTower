//@CustomCode
namespace MagicTower.WebApi.Models
{
    #region Request DTOs

    /// <summary>
    /// Request to create a new character.
    /// </summary>
    public class CreateCharacterRequest
    {
        /// <summary>
        /// Gets or sets the character name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the character class (1=Warrior, 2=Archer, 3=Druid).
        /// </summary>
        public int CharacterClass { get; set; }
    }

    /// <summary>
    /// Request to start a new game session.
    /// </summary>
    public class StartGameSessionRequest
    {
        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// Gets or sets the difficulty (10=Easy, 20=Medium, 30=Hard).
        /// </summary>
        public int Difficulty { get; set; }
    }

    /// <summary>
    /// Request to start a fight.
    /// </summary>
    public class StartFightRequest
    {
        /// <summary>
        /// Gets or sets the game session ID.
        /// </summary>
        public int GameSessionId { get; set; }

        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public int CharacterId { get; set; }
    }

    /// <summary>
    /// Request to execute a combat action.
    /// </summary>
    public class CombatActionRequest
    {
        /// <summary>
        /// Gets or sets the game session ID.
        /// </summary>
        public int GameSessionId { get; set; }

        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public int CharacterId { get; set; }

        /// <summary>
        /// Gets or sets whether to use special attack.
        /// </summary>
        public bool UseSpecialAttack { get; set; } = false;

        /// <summary>
        /// Gets or sets the current enemy data.
        /// </summary>
        public EnemyDto Enemy { get; set; } = new EnemyDto();
    }

    /// <summary>
    /// Request to upgrade a weapon.
    /// </summary>
    public class UpgradeWeaponRequest
    {
        /// <summary>
        /// Gets or sets the weapon ID.
        /// </summary>
        public int WeaponId { get; set; }

        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public int CharacterId { get; set; }
    }

    /// <summary>
    /// Request to sell a weapon.
    /// </summary>
    public class SellWeaponRequest
    {
        /// <summary>
        /// Gets or sets the weapon ID.
        /// </summary>
        public int WeaponId { get; set; }

        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public int CharacterId { get; set; }
    }

    /// <summary>
    /// Request to equip/unequip a weapon.
    /// </summary>
    public class EquipWeaponRequest
    {
        /// <summary>
        /// Gets or sets the weapon ID.
        /// </summary>
        public int WeaponId { get; set; }

        /// <summary>
        /// Gets or sets whether to equip (true) or unequip (false).
        /// </summary>
        public bool Equip { get; set; }
    }

    /// <summary>
    /// Request for AI chat interaction.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the user message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional character ID for context.
        /// </summary>
        public int? CharacterId { get; set; }

        /// <summary>
        /// Gets or sets the optional game session ID for context.
        /// This is also used as sessionId for n8n chat history.
        /// </summary>
        public int? GameSessionId { get; set; }

        /// <summary>
        /// Gets or sets the conversation history (last 10 messages).
        /// </summary>
        public List<ChatMessage> ConversationHistory { get; set; } = new List<ChatMessage>();

        /// <summary>
        /// Gets or sets updated game state from AI (if any changes were made).
        /// </summary>
        public GameStateUpdateDto? GameStateUpdate { get; set; }
    }

    /// <summary>
    /// Game state updates from AI to be persisted.
    /// </summary>
    public class GameStateUpdateDto
    {
        /// <summary>
        /// Character updates (null if no character created/updated).
        /// </summary>
        public CharacterUpdateDto? Character { get; set; }

        /// <summary>
        /// Game session updates (null if no session created/updated).
        /// </summary>
        public GameSessionUpdateDto? GameSession { get; set; }

        /// <summary>
        /// Current enemy state (null if not in combat).
        /// </summary>
        public EnemyDto? CurrentEnemy { get; set; }

        /// <summary>
        /// List of actions performed (for logging/tracking).
        /// </summary>
        public List<string> ActionsPerformed { get; set; } = new List<string>();
    }

    /// <summary>
    /// Character updates from AI.
    /// </summary>
    public class CharacterUpdateDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int? CharacterClass { get; set; }
        public int? Level { get; set; }
        public int? MaxHealth { get; set; }
        public int? CurrentHealth { get; set; }
        public int? AttackPower { get; set; }
        public int? Gold { get; set; }
        public int? SpecialAttackLevel { get; set; }
    }

    /// <summary>
    /// Game session updates from AI.
    /// </summary>
    public class GameSessionUpdateDto
    {
        public int? Id { get; set; }
        public int? CharacterId { get; set; }
        public int? Difficulty { get; set; }
        public int? CurrentFloor { get; set; }
        public bool? IsCompleted { get; set; }
    }

    /// <summary>
    /// Single chat message in conversation history.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets or sets the role (user or assistant).
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Response from AI chat interaction.
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// Gets or sets the AI-generated response text.
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current game state (character, session, enemy if in combat).
        /// </summary>
        public GameStateDto? GameState { get; set; }
    }

    /// <summary>
    /// Current game state snapshot.
    /// </summary>
    public class GameStateDto
    {
        /// <summary>
        /// Gets or sets the character information if available.
        /// </summary>
        public CharacterDto? Character { get; set; }

        /// <summary>
        /// Gets or sets the game session information if available.
        /// </summary>
        public GameSessionDto? GameSession { get; set; }

        /// <summary>
        /// Gets or sets the current enemy if in combat.
        /// </summary>
        public EnemyDto? CurrentEnemy { get; set; }
    }

    /// <summary>
    /// Response containing character information.
    /// </summary>
    public class CharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CharacterClass { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int AttackPower { get; set; }
        public int SpecialAttackLevel { get; set; }
        public int Gold { get; set; }
        public int TotalAttackPower { get; set; }
        public List<WeaponDto> Weapons { get; set; } = new List<WeaponDto>();
    }

    /// <summary>
    /// Response containing game session information.
    /// </summary>
    public class GameSessionDto
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int Difficulty { get; set; }
        public int CurrentFloor { get; set; }
        public int MaxFloor { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Response containing enemy information.
    /// </summary>
    public class EnemyDto
    {
        public string Type { get; set; } = string.Empty;
        public string Race { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int AttackPower { get; set; }
        public string Weapon { get; set; } = string.Empty;
        public bool IsBoss { get; set; }
        public bool HasSpecialAttack { get; set; }
        public string DisplayName => $"{Race} {Type}";
        public string ImageKey => $"{Race.ToLower()}_{Type.ToLower()}";
    }

    /// <summary>
    /// Response containing combat result.
    /// </summary>
    public class CombatResultDto
    {
        public int DamageToEnemy { get; set; }
        public int DamageToCharacter { get; set; }
        public int RemainingEnemyHealth { get; set; }
        public int RemainingCharacterHealth { get; set; }
        public bool IsEnemyDefeated { get; set; }
        public bool IsCharacterDefeated { get; set; }
        public bool SpecialAttackUsed { get; set; }
        public bool BossSpecialAttackUsed { get; set; }
        public string Message { get; set; } = string.Empty;
        public EnemyDto? Enemy { get; set; }
        public CharacterDto? Character { get; set; }
        public RewardsDto? Rewards { get; set; }
    }

    /// <summary>
    /// Response containing weapon information.
    /// </summary>
    public class WeaponDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int DamageBonus { get; set; }
        public int UpgradeLevel { get; set; }
        public int SuitableForClass { get; set; }
        public bool IsEquipped { get; set; }
        public int SellValue { get; set; }
        public int UpgradeCost { get; set; }
        public int TotalDamage => DamageBonus + (UpgradeLevel * 5);
    }

    /// <summary>
    /// Response containing rewards after defeating an enemy.
    /// </summary>
    public class RewardsDto
    {
        public int GoldReward { get; set; }
        public bool LevelUp { get; set; }
        public bool NewWeapon { get; set; }
        public WeaponDto? Weapon { get; set; }
        public bool SpecialAttackUpgrade { get; set; }
        public bool FullHeal { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic API response wrapper.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? Error { get; set; }
    }

    #endregion
}
