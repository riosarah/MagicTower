using System;
using System.Collections.Generic;

namespace MagicTower.Common.Contracts.DTOs
{
    /// <summary>
    /// DTO for n8n workflow communication.
    /// Contains all required data for AI-driven game interaction via Gemini + MCP Tools.
    /// </summary>
    public class N8nRequestDto
    {
        /// <summary>
        /// User's message text.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Session ID for n8n chat history tracking.
        /// Can be GameSession.Id or temporary session ID.
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Optional character ID for game context.
        /// </summary>
        public int? CharacterId { get; set; }

        /// <summary>
        /// Optional game session ID for game context.
        /// </summary>
        public int? GameSessionId { get; set; }

        /// <summary>
        /// Conversation history (last 10 messages) for AI context.
        /// </summary>
        public List<ChatMessageDto> ConversationHistory { get; set; } = new List<ChatMessageDto>();

        /// <summary>
        /// Current game state (character, session, enemy) for AI context.
        /// </summary>
        public GameStateDto? GameState { get; set; }
    }

    /// <summary>
    /// Single chat message in conversation history.
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// Message role: "user" or "assistant".
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Message content text.
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Complete game state snapshot for AI context.
    /// </summary>
    public class GameStateDto
    {
        /// <summary>
        /// Current character information.
        /// </summary>
        public CharacterStateDto? Character { get; set; }

        /// <summary>
        /// Current game session information.
        /// </summary>
        public GameSessionStateDto? GameSession { get; set; }

        /// <summary>
        /// Current enemy information (if in combat).
        /// </summary>
        public EnemyStateDto? CurrentEnemy { get; set; }
    }

    /// <summary>
    /// Character state for AI context.
    /// </summary>
    public class CharacterStateDto
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
        public List<WeaponStateDto> Weapons { get; set; } = new List<WeaponStateDto>();
    }

    /// <summary>
    /// Game session state for AI context.
    /// </summary>
    public class GameSessionStateDto
    {
        public int Id { get; set; }
        public int? CharacterId { get; set; }  // Nullable - kann von n8n als null kommen
        public int Difficulty { get; set; }
        public int CurrentFloor { get; set; }
        public int MaxFloor { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Enemy state for AI context.
    /// </summary>
    public class EnemyStateDto
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
    }

    /// <summary>
    /// Weapon state for AI context.
    /// </summary>
    public class WeaponStateDto
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
    /// Response from n8n workflow.
    /// </summary>
    public class N8nResponseDto
    {
        /// <summary>
        /// AI-generated response text.
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Updated game state from AI actions.
        /// </summary>
        public GameStateUpdateDto? GameStateUpdate { get; set; }

        /// <summary>
        /// Optional: Character ID if character was created.
        /// </summary>
        public int? CharacterId { get; set; }

        /// <summary>
        /// Optional: Game session ID if session was created.
        /// </summary>
        public int? GameSessionId { get; set; }

        /// <summary>
        /// Optional: Current game state snapshot (returned by n8n for context).
        /// </summary>
        public GameStateDto? GameState { get; set; }

        /// <summary>
        /// Optional: List of MCP tool actions performed by AI (for logging/debugging).
        /// </summary>
        public List<string>? ActionsPerformed { get; set; }
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
        public EnemyStateDto? CurrentEnemy { get; set; }

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
}
