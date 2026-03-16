#if GENERATEDCODE_ON
//@CustomCode
using Microsoft.AspNetCore.Mvc;
using MagicTower.Logic.Contracts;
using MagicTower.Logic.Modules;
using MagicTower.Logic.Entities.Game;
using MagicTower.WebApi.Models;
using MagicTower.WebApi.Contracts;
using MagicTower.WebApi.Extensions;
using MagicTower.Common.Contracts.DTOs;
using CharacterClass = MagicTower.Common.Models.Game.CharacterClass;
using Difficulty = MagicTower.Common.Models.Game.Difficulty;
using WebApiGameStateDto = MagicTower.WebApi.Models.GameStateDto;
using WebApiCharacterUpdateDto = MagicTower.WebApi.Models.CharacterUpdateDto;
using WebApiGameSessionUpdateDto = MagicTower.WebApi.Models.GameSessionUpdateDto;
using N8nGameStateDto = MagicTower.Common.Contracts.DTOs.GameStateDto;
using N8nGameStateUpdateDto = MagicTower.Common.Contracts.DTOs.GameStateUpdateDto;

namespace MagicTower.WebApi.Controllers
{
    /// <summary>
    /// Controller for N8N MCP game endpoints.
    /// Provides stateless endpoints for the fighting game simulation.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GameMcpController : ControllerBase
    {
        private readonly IContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public GameMcpController(
            IContextAccessor contextAccessor, 
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _contextAccessor = contextAccessor;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        #region Chat Endpoint

        /// <summary>
        /// Main chat endpoint for AI-driven game interaction.
        /// Forwards user messages to n8n workflow which uses Gemini AI with MCP tools.
        /// </summary>
        /// <param name="request">Chat request with user message and context</param>
        /// <returns>AI response with updated game state</returns>
        [HttpPost("chat")]
        public async Task<ActionResult<ApiResponse<ChatResponse>>> Chat([FromBody] ChatRequest request)
        {
            using var context = _contextAccessor.GetContext();

            try
            {
                var n8nUrl = _configuration["N8N_COnnectionString"];
                if (string.IsNullOrEmpty(n8nUrl))
                {
                    return BadRequest(new ApiResponse<ChatResponse>
                    {
                        Success = false,
                        Message = "n8n Webhook URL nicht konfiguriert"
                    });
                }

                // Persist game state updates if provided (from n8n code node)
                if (request.GameStateUpdate != null)
                {
                    await PersistGameStateUpdateAsync(request);
                }

                // CRITICAL: Ensure we ALWAYS have a Character and GameSession before calling n8n
                // This prevents "null value in column session_id" errors in n8n database
                
                int characterIdForN8n;
                int gameSessionIdForN8n;
                
                if (request.CharacterId.HasValue && request.GameSessionId.HasValue)
                {
                    // Both IDs exist - use them
                    characterIdForN8n = request.CharacterId.Value;
                    gameSessionIdForN8n = request.GameSessionId.Value;
                }
                else if (request.CharacterId.HasValue)
                {
                    // Character exists but no session - create session
                    var gameService = new GameService(context);
                    var newSession = await gameService.CreateGameSessionAsync(
                        request.CharacterId.Value,
                        Difficulty.Medium
                    );
                    
                    characterIdForN8n = request.CharacterId.Value;
                    gameSessionIdForN8n = newSession.Id;
                    request.GameSessionId = newSession.Id;
                }
                else
                {
                    // NO Character and NO Session - create both with default values
                    // This ensures n8n ALWAYS has valid IDs to work with
                    
                    var tempCharacter = new Character
                    {
                        Name = "Held",
                        Class = CharacterClass.Warrior,
                        Level = 1,
                        MaxHealth = 100,
                        CurrentHealth = 100,
                        AttackPower = 10,
                        SpecialAttackLevel = 1,
                        Gold = 0
                    };
                    
                    await context.CharacterSet.AddAsync(tempCharacter);
                    await context.SaveChangesAsync();
                    
                    var tempSession = new GameSession
                    {
                        CharacterId = tempCharacter.Id,
                        Difficulty = Difficulty.Medium,
                        CurrentFloor = 1,
                        MaxFloor = 20,
                        IsCompleted = false,
                        StartedAt = DateTime.UtcNow
                    };
                    
                    await context.GameSessionSet.AddAsync(tempSession);
                    await context.SaveChangesAsync();
                    
                    characterIdForN8n = tempCharacter.Id;
                    gameSessionIdForN8n = tempSession.Id;
                    request.CharacterId = tempCharacter.Id;
                    request.GameSessionId = tempSession.Id;
                }

                // Build game context for AI
                WebApiGameStateDto? gameState = null;
                if (request.CharacterId.HasValue)
                {
                    gameState = await BuildGameStateAsync(context, request.CharacterId.Value, request.GameSessionId);
                }

                // Prepare n8n request using N8nRequestDto
                // NOTE: ConversationHistory is NOT sent - n8n tracks chat history via SessionId in its own database
                var n8nRequest = new N8nRequestDto
                {
                    Message = request.Message,
                    SessionId = gameSessionIdForN8n.ToString(),  // ALWAYS set - guaranteed to be valid
                    CharacterId = characterIdForN8n,
                    GameSessionId = gameSessionIdForN8n,
                    ConversationHistory = new List<ChatMessageDto>(),  // Empty - n8n manages history via SessionId
                    GameState = gameState != null ? MapToN8nGameStateDto(gameState) : null
                };

                // Call n8n webhook
                var httpClient = _httpClientFactory.CreateClient();
                
                // Log outgoing request for debugging
                var requestJson = System.Text.Json.JsonSerializer.Serialize(n8nRequest, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                Console.WriteLine("=== N8N REQUEST ===");
                Console.WriteLine(requestJson);
                Console.WriteLine("==================");
                
                var response = await httpClient.PostAsJsonAsync(n8nUrl, n8nRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"=== N8N ERROR ({response.StatusCode}) ===");
                    Console.WriteLine(errorContent);
                    Console.WriteLine("==================");
                    
                    return BadRequest(new ApiResponse<ChatResponse>
                    {
                        Success = false,
                        Message = $"n8n Webhook Fehler: {response.StatusCode}",
                        Error = errorContent
                    });
                }

                // Parse n8n response (n8n can return either an object or an array)
                // Read raw content first for debugging
                var rawContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine("=== N8N RESPONSE (RAW) ===");
                Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}");
                Console.WriteLine($"Length: {rawContent.Length} chars");
                Console.WriteLine($"First 100 chars: {rawContent.Substring(0, Math.Min(100, rawContent.Length))}");
                Console.WriteLine($"Last 100 chars: {(rawContent.Length > 100 ? rawContent.Substring(rawContent.Length - 100) : "N/A")}");
                Console.WriteLine("Full content:");
                Console.WriteLine(rawContent);
                Console.WriteLine("=========================");
                
                // Try to parse with case-insensitive property names
                N8nResponseDto? n8nResponse = null;
                try
                {
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };
                    
                    // Check if response starts with '[' (array) or '{' (object)
                    var trimmedContent = rawContent.TrimStart();
                    if (trimmedContent.StartsWith("["))
                    {
                        // Parse as array and take first element
                        Console.WriteLine("=== Parsing as ARRAY ===");
                        var n8nResponseArray = System.Text.Json.JsonSerializer.Deserialize<List<N8nResponseDto>>(rawContent, jsonOptions);
                        
                        if (n8nResponseArray == null || n8nResponseArray.Count == 0)
                        {
                            Console.WriteLine("=== EMPTY RESPONSE ARRAY ===");
                            return BadRequest(new ApiResponse<ChatResponse>
                            {
                                Success = false,
                                Message = "Ungültige n8n Response - Array ist leer",
                                Error = $"Raw Content:\n{rawContent}"
                            });
                        }
                        n8nResponse = n8nResponseArray[0];
                    }
                    else if (trimmedContent.StartsWith("{"))
                    {
                        // Parse as single object
                        Console.WriteLine("=== Parsing as OBJECT ===");
                        n8nResponse = System.Text.Json.JsonSerializer.Deserialize<N8nResponseDto>(rawContent, jsonOptions);
                    }
                    else
                    {
                        Console.WriteLine($"=== INVALID JSON START CHARACTER: '{trimmedContent.Substring(0, Math.Min(10, trimmedContent.Length))}' ===");
                        return BadRequest(new ApiResponse<ChatResponse>
                        {
                            Success = false,
                            Message = "Ungültige n8n Response - weder Array noch Objekt",
                            Error = $"Content startet mit: {trimmedContent.Substring(0, Math.Min(50, trimmedContent.Length))}"
                        });
                    }
                    
                    Console.WriteLine("=== DESERIALIZATION SUCCESS ===");
                    if (n8nResponse != null)
                    {
                        Console.WriteLine($"Response: {n8nResponse.Response?.Substring(0, Math.Min(100, n8nResponse.Response?.Length ?? 0))}...");
                        Console.WriteLine($"CharacterId: {n8nResponse.CharacterId}");
                        Console.WriteLine($"GameSessionId: {n8nResponse.GameSessionId}");
                        Console.WriteLine($"GameState: {(n8nResponse.GameState != null ? "Present" : "Null")}");
                        Console.WriteLine($"ActionsPerformed: {n8nResponse.ActionsPerformed?.Count ?? 0} items");
                    }
                    Console.WriteLine("==============================");
                }
                catch (System.Text.Json.JsonException ex)
                {
                    Console.WriteLine("=== DESERIALIZATION ERROR ===");
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    Console.WriteLine($"Path: {ex.Path}");
                    Console.WriteLine($"LineNumber: {ex.LineNumber}");
                    Console.WriteLine($"BytePositionInLine: {ex.BytePositionInLine}");
                    Console.WriteLine("============================");
                    
                    // If parsing fails, log and return error
                    return BadRequest(new ApiResponse<ChatResponse>
                    {
                        Success = false,
                        Message = "Fehler beim Parsen der n8n Response",
                        Error = $"JSON Error: {ex.Message}\n" +
                               $"Path: {ex.Path}\n" +
                               $"Line: {ex.LineNumber}, Position: {ex.BytePositionInLine}\n\n" +
                               $"Raw Content:\n{rawContent}"
                    });
                }
                
                if (n8nResponse == null)
                {
                    Console.WriteLine("=== NULL RESPONSE ===");
                    return BadRequest(new ApiResponse<ChatResponse>
                    {
                        Success = false,
                        Message = "Ungültige n8n Response - Objekt ist null",
                        Error = $"Raw Content:\n{rawContent}"
                    });
                }

                // Use CharacterId and GameSessionId from n8n response if provided (overrides request values)
                if (n8nResponse.CharacterId.HasValue)
                {
                    characterIdForN8n = n8nResponse.CharacterId.Value;
                    request.CharacterId = n8nResponse.CharacterId.Value;
                }
                if (n8nResponse.GameSessionId.HasValue)
                {
                    gameSessionIdForN8n = n8nResponse.GameSessionId.Value;
                    request.GameSessionId = n8nResponse.GameSessionId.Value;
                }

                // Update Character and GameSession if AI made changes
                // CRITICAL: MCP Tools update their own state but DON'T update our database
                // We must sync the changes from n8nResponse.GameState back to our DB!
                if (n8nResponse.GameState?.Character != null)
                {
                    Console.WriteLine("=== SYNCING CHARACTER STATE FROM N8N TO DB ===");
                    var character = await context.CharacterSet.GetByIdAsync(characterIdForN8n);
                    if (character != null)
                    {
                        // Sync all character stats from n8n response to database
                        character.Name = n8nResponse.GameState.Character.Name;
                        character.Class = (CharacterClass)n8nResponse.GameState.Character.CharacterClass;
                        character.Level = n8nResponse.GameState.Character.Level;
                        character.MaxHealth = n8nResponse.GameState.Character.MaxHealth;
                        character.CurrentHealth = n8nResponse.GameState.Character.CurrentHealth;
                        character.AttackPower = n8nResponse.GameState.Character.AttackPower;
                        character.SpecialAttackLevel = n8nResponse.GameState.Character.SpecialAttackLevel;
                        character.Gold = n8nResponse.GameState.Character.Gold;
                        
                        Console.WriteLine($"Updating Character {character.Id} in DB:");
                        Console.WriteLine($"  Name: {character.Name}");
                        Console.WriteLine($"  Class: {character.Class}");
                        Console.WriteLine($"  Level: {character.Level}");
                        Console.WriteLine($"  HP: {character.CurrentHealth}/{character.MaxHealth}");
                        Console.WriteLine($"  ATK: {character.AttackPower}");
                        Console.WriteLine($"  Gold: {character.Gold}");
                        Console.WriteLine($"  Special Attacks: {character.SpecialAttackLevel}");
                        
                        await context.CharacterSet.UpdateAsync(character.Id, character);
                        await context.SaveChangesAsync();
                    }
                }
                
                // Also update GameSession if present
                if (n8nResponse.GameState?.GameSession != null)
                {
                    Console.WriteLine("=== SYNCING GAME SESSION STATE FROM N8N TO DB ===");
                    var session = await context.GameSessionSet.GetByIdAsync(gameSessionIdForN8n);
                    if (session != null)
                    {
                        session.CurrentFloor = n8nResponse.GameState.GameSession.CurrentFloor;
                        session.IsCompleted = n8nResponse.GameState.GameSession.IsCompleted;
                        
                        // CRITICAL: Save or clear CurrentEnemy state
                        if (n8nResponse.GameState.CurrentEnemy != null)
                        {
                            // Save enemy to database
                            var enemyDto = new EnemyDto
                            {
                                Type = n8nResponse.GameState.CurrentEnemy.Type,
                                Race = n8nResponse.GameState.CurrentEnemy.Race,
                                Level = n8nResponse.GameState.CurrentEnemy.Level,
                                Health = n8nResponse.GameState.CurrentEnemy.Health,
                                MaxHealth = n8nResponse.GameState.CurrentEnemy.MaxHealth,
                                AttackPower = n8nResponse.GameState.CurrentEnemy.AttackPower,
                                Weapon = n8nResponse.GameState.CurrentEnemy.Weapon,
                                IsBoss = n8nResponse.GameState.CurrentEnemy.IsBoss,
                                HasSpecialAttack = n8nResponse.GameState.CurrentEnemy.HasSpecialAttack
                            };
                            session.SaveCurrentEnemy(enemyDto);
                            Console.WriteLine($"  Saved CurrentEnemy: {enemyDto.Race} {enemyDto.Type} (HP: {enemyDto.Health}/{enemyDto.MaxHealth})");
                        }
                        else
                        {
                            // Clear enemy from database
                            session.SaveCurrentEnemy(null);
                            Console.WriteLine($"  Cleared CurrentEnemy (combat ended)");
                        }
                        
                        Console.WriteLine($"Updating GameSession {session.Id} in DB:");
                        Console.WriteLine($"  Current Floor: {session.CurrentFloor}/{session.MaxFloor}");
                        Console.WriteLine($"  Completed: {session.IsCompleted}");
                        
                        await context.GameSessionSet.UpdateAsync(session.Id, session);
                        await context.SaveChangesAsync();
                    }

                    // CRITICAL: Save chat messages to database for history tracking
                    // This enables chat history retrieval via GET /api/GameSessions/{id}/with-chat
                    if (session != null)
                    {
                        // Get the next sequence number for user message
                        var chatMessageSet = context.ChatMessageSet;
                        var allMessages = await chatMessageSet.GetAllAsync();
                        var sessionMessages = allMessages.Where(m => m.GameSessionId == gameSessionIdForN8n).ToList();
                        int nextSequenceNumber = sessionMessages.Any() 
                            ? sessionMessages.Max(m => m.SequenceNumber) + 1 
                            : 1;

                        // Save user message
                        var userMessage = new Logic.Entities.Game.ChatMessage
                        {
                            GameSessionId = gameSessionIdForN8n,
                            Message = request.Message,
                            IsAiMessage = false,
                            SentAt = DateTime.UtcNow,
                            SequenceNumber = nextSequenceNumber
                        };
                        await chatMessageSet.AddAsync(userMessage);
                        Console.WriteLine($"Saved user message to DB (GameSession {gameSessionIdForN8n}, Seq: {nextSequenceNumber})");

                        // Save AI response
                        var aiMessage = new Logic.Entities.Game.ChatMessage
                        {
                            GameSessionId = gameSessionIdForN8n,
                            Message = n8nResponse.Response,
                            IsAiMessage = true,
                            SentAt = DateTime.UtcNow,
                            SequenceNumber = nextSequenceNumber + 1
                        };
                        await chatMessageSet.AddAsync(aiMessage);
                        Console.WriteLine($"Saved AI response to DB (GameSession {gameSessionIdForN8n}, Seq: {nextSequenceNumber + 1})");
                        
                        await context.SaveChangesAsync();
                    }
                }
                
                // Legacy support: Also handle GameStateUpdate if provided
                if (n8nResponse.GameStateUpdate != null)
                {
                    await UpdateEntitiesFromN8nResponse(context, n8nResponse.GameStateUpdate, characterIdForN8n, gameSessionIdForN8n);
                }

                // Refresh game state after AI actions
                // NOTE: BuildGameStateAsync now loads CurrentEnemy from database, so no need to manually set it
                WebApiGameStateDto? updatedGameState = null;
                if (request.CharacterId.HasValue)
                {
                    updatedGameState = await BuildGameStateAsync(context, request.CharacterId.Value, request.GameSessionId);
                }

               

                var chatResponse = new ChatResponse
                {
                    Response = n8nResponse.Response,
                    GameState = updatedGameState
                };

                return Ok(new ApiResponse<ChatResponse>
                {
                    Success = true,
                    Data = chatResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<ChatResponse>
                {
                    Success = false,
                    Message = "Fehler bei der Chat-Verarbeitung",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates Character and GameSession entities based on AI response.
        /// </summary>
        private async Task UpdateEntitiesFromN8nResponse(
            IContext context, 
            N8nGameStateUpdateDto update, 
            int characterId, 
            int gameSessionId)
        {
            // Update Character
            if (update.Character != null)
            {
                var character = await context.CharacterSet.GetByIdAsync(characterId);
                if (character != null)
                {
                    if (!string.IsNullOrEmpty(update.Character.Name))
                        character.Name = update.Character.Name;
                    if (update.Character.CharacterClass.HasValue)
                        character.Class = (CharacterClass)update.Character.CharacterClass.Value;
                    if (update.Character.Level.HasValue)
                        character.Level = update.Character.Level.Value;
                    if (update.Character.MaxHealth.HasValue)
                        character.MaxHealth = update.Character.MaxHealth.Value;
                    if (update.Character.CurrentHealth.HasValue)
                        character.CurrentHealth = update.Character.CurrentHealth.Value;
                    if (update.Character.AttackPower.HasValue)
                        character.AttackPower = update.Character.AttackPower.Value;
                    if (update.Character.Gold.HasValue)
                        character.Gold = update.Character.Gold.Value;
                    if (update.Character.SpecialAttackLevel.HasValue)
                        character.SpecialAttackLevel = update.Character.SpecialAttackLevel.Value;

                    await context.CharacterSet.UpdateAsync(character.Id, character);
                }
            }

            // Update GameSession
            if (update.GameSession != null)
            {
                var session = await context.GameSessionSet.GetByIdAsync(gameSessionId);
                if (session != null)
                {
                    if (update.GameSession.Difficulty.HasValue)
                        session.Difficulty = (Difficulty)update.GameSession.Difficulty.Value;
                    if (update.GameSession.CurrentFloor.HasValue)
                        session.CurrentFloor = update.GameSession.CurrentFloor.Value;
                    if (update.GameSession.IsCompleted.HasValue)
                        session.IsCompleted = update.GameSession.IsCompleted.Value;

                    await context.GameSessionSet.UpdateAsync(session.Id, session);
                }
            }
        }

        /// <summary>
        /// Maps GameStateDto to N8N GameStateDto.
        /// </summary>
        private N8nGameStateDto? MapToN8nGameStateDto(WebApiGameStateDto gameState)
        {
            return new N8nGameStateDto
            {
                Character = gameState.Character != null ? new CharacterStateDto
                {
                    Id = gameState.Character.Id,
                    Name = gameState.Character.Name,
                    CharacterClass = gameState.Character.CharacterClass,
                    ClassName = gameState.Character.ClassName,
                    Level = gameState.Character.Level,
                    MaxHealth = gameState.Character.MaxHealth,
                    CurrentHealth = gameState.Character.CurrentHealth,
                    AttackPower = gameState.Character.AttackPower,
                    SpecialAttackLevel = gameState.Character.SpecialAttackLevel,
                    Gold = gameState.Character.Gold,
                    TotalAttackPower = gameState.Character.TotalAttackPower,
                    Weapons = gameState.Character.Weapons?.Select(w => new WeaponStateDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        Type = w.Type,
                        DamageBonus = w.DamageBonus,
                        UpgradeLevel = w.UpgradeLevel,
                        SuitableForClass = w.SuitableForClass,
                        IsEquipped = w.IsEquipped,
                        SellValue = w.SellValue,
                        UpgradeCost = w.UpgradeCost
                    }).ToList() ?? new List<WeaponStateDto>()
                } : null,
                GameSession = gameState.GameSession != null ? new GameSessionStateDto
                {
                    Id = gameState.GameSession.Id,
                    CharacterId = gameState.GameSession.CharacterId,
                    Difficulty = gameState.GameSession.Difficulty,
                    CurrentFloor = gameState.GameSession.CurrentFloor,
                    MaxFloor = gameState.GameSession.MaxFloor,
                    IsCompleted = gameState.GameSession.IsCompleted,
                    StartedAt = gameState.GameSession.StartedAt,
                    CompletedAt = gameState.GameSession.CompletedAt
                } : null,
                CurrentEnemy = gameState.CurrentEnemy != null ? new EnemyStateDto
                {
                    Type = gameState.CurrentEnemy.Type,
                    Race = gameState.CurrentEnemy.Race,
                    Level = gameState.CurrentEnemy.Level,
                    Health = gameState.CurrentEnemy.Health,
                    MaxHealth = gameState.CurrentEnemy.MaxHealth,
                    AttackPower = gameState.CurrentEnemy.AttackPower,
                    Weapon = gameState.CurrentEnemy.Weapon,
                    IsBoss = gameState.CurrentEnemy.IsBoss,
                    HasSpecialAttack = gameState.CurrentEnemy.HasSpecialAttack
                } : null
            };
        }

        /// <summary>
        /// Persists game state updates from AI to database.
        /// </summary>
        private async Task PersistGameStateUpdateAsync(ChatRequest request)
        {
            using var context = _contextAccessor.GetContext();
            var gameService = new GameService(context);

            var update = request.GameStateUpdate!;

            // Update or create character
            if (update.Character != null)
            {
                if (update.Character.Id.HasValue && update.Character.Id.Value > 0)
                {
                    // Update existing character
                    var character = await context.CharacterSet.GetByIdAsync(update.Character.Id.Value);
                    if (character != null)
                    {
                        if (update.Character.Level.HasValue)
                            character.Level = update.Character.Level.Value;
                        if (update.Character.MaxHealth.HasValue)
                            character.MaxHealth = update.Character.MaxHealth.Value;
                        if (update.Character.CurrentHealth.HasValue)
                            character.CurrentHealth = update.Character.CurrentHealth.Value;
                        if (update.Character.AttackPower.HasValue)
                            character.AttackPower = update.Character.AttackPower.Value;
                        if (update.Character.Gold.HasValue)
                            character.Gold = update.Character.Gold.Value;
                        if (update.Character.SpecialAttackLevel.HasValue)
                            character.SpecialAttackLevel = update.Character.SpecialAttackLevel.Value;

                        await context.CharacterSet.UpdateAsync(character.Id, character);
                    }
                }
                else if (!string.IsNullOrEmpty(update.Character.Name) && update.Character.CharacterClass.HasValue)
                {
                    // Create new character
                    var newChar = await gameService.CreateCharacterAsync(
                        update.Character.Name,
                        (CharacterClass)update.Character.CharacterClass.Value
                    );
                    request.CharacterId = newChar.Id;
                }
            }

            // Update or create game session
            if (update.GameSession != null)
            {
                if (update.GameSession.Id.HasValue && update.GameSession.Id.Value > 0)
                {
                    // Update existing session
                    var session = await context.GameSessionSet.GetByIdAsync(update.GameSession.Id.Value);
                    if (session != null)
                    {
                        if (update.GameSession.CurrentFloor.HasValue)
                            session.CurrentFloor = update.GameSession.CurrentFloor.Value;
                        if (update.GameSession.IsCompleted.HasValue)
                            session.IsCompleted = update.GameSession.IsCompleted.Value;

                        await context.GameSessionSet.UpdateAsync(session.Id, session);
                    }
                }
                else if (request.CharacterId.HasValue && update.GameSession.Difficulty.HasValue)
                {
                    // Create new session
                    var newSession = await gameService.CreateGameSessionAsync(
                        request.CharacterId.Value,
                        (Difficulty)update.GameSession.Difficulty.Value
                    );
                    request.GameSessionId = newSession.Id;
                }
            }
        }

        /// <summary>
        /// Builds current game state for AI context.
        /// </summary>
        private async Task<WebApiGameStateDto> BuildGameStateAsync(IContext context, int characterId, int? gameSessionId)
        {
            var gameService = new GameService(context);
            var characterSet = context.CharacterSet;

            var character = await characterSet.GetByIdAsync(characterId);
            if (character == null)
            {
                return new WebApiGameStateDto();
            }

            var gameState = new WebApiGameStateDto
            {
                Character = await MapCharacterToDto(context, character, gameService)
            };

            if (gameSessionId.HasValue)
            {
                var sessionSet = context.GameSessionSet;
                var session = await sessionSet.GetByIdAsync(gameSessionId.Value);
                if (session != null)
                {
                    gameState.GameSession = MapGameSessionToDto(session);
                    
                    // Load CurrentEnemy from database if it exists
                    var currentEnemy = session.LoadCurrentEnemy();
                    if (currentEnemy != null)
                    {
                        gameState.CurrentEnemy = currentEnemy;
                        Console.WriteLine($"Loaded CurrentEnemy from DB: {currentEnemy.Race} {currentEnemy.Type} (HP: {currentEnemy.Health}/{currentEnemy.MaxHealth})");
                    }
                }
            }

            return gameState;
        }

        #endregion

        #region Character Endpoints

        /// <summary>
        /// Creates a new character with specified class.
        /// </summary>
        /// <param name="request">Character creation request</param>
        /// <returns>Created character information</returns>
        [HttpPost("character/create")]
        public async Task<ActionResult<ApiResponse<CharacterDto>>> CreateCharacter([FromBody] CreateCharacterRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);

                var character = await gameService.CreateCharacterAsync(
                    request.Name,
                    (CharacterClass)request.CharacterClass
                );

                var characterDto = await MapCharacterToDto(context, character, gameService);

                return Ok(new ApiResponse<CharacterDto>
                {
                    Success = true,
                    Message = $"Charakter '{character.Name}' erfolgreich erstellt!",
                    Data = characterDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CharacterDto>
                {
                    Success = false,
                    Message = "Fehler beim Erstellen des Charakters",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets character information by ID.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>Character information</returns>
        [HttpGet("character/{characterId}")]
        public async Task<ActionResult<ApiResponse<CharacterDto>>> GetCharacter(int characterId)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var characterSet = context.CharacterSet;
                
                var character = await characterSet.GetByIdAsync(characterId);
                if (character == null)
                {
                    return NotFound(new ApiResponse<CharacterDto>
                    {
                        Success = false,
                        Message = $"Charakter mit ID {characterId} nicht gefunden"
                    });
                }

                var characterDto = await MapCharacterToDto(context, character, gameService);

                return Ok(new ApiResponse<CharacterDto>
                {
                    Success = true,
                    Data = characterDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CharacterDto>
                {
                    Success = false,
                    Message = "Fehler beim Laden des Charakters",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets all characters.
        /// </summary>
        /// <returns>List of all characters</returns>
        [HttpGet("character/list")]
        public async Task<ActionResult<ApiResponse<List<CharacterDto>>>> GetAllCharacters()
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var characterSet = context.CharacterSet;
                
                var characters = await characterSet.GetAllAsync();
                var characterDtos = new List<CharacterDto>();

                foreach (var character in characters)
                {
                    characterDtos.Add(await MapCharacterToDto(context, character, gameService));
                }

                return Ok(new ApiResponse<List<CharacterDto>>
                {
                    Success = true,
                    Data = characterDtos
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<CharacterDto>>
                {
                    Success = false,
                    Message = "Fehler beim Laden der Charaktere",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates character stats (for AI-driven game state management).
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="update">Character update data</param>
        /// <returns>Updated character</returns>
        [HttpPut("character/{characterId}/update")]
        public async Task<ActionResult<ApiResponse<CharacterDto>>> UpdateCharacter(
            int characterId,
            [FromBody] WebApiCharacterUpdateDto update)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var characterSet = context.CharacterSet;
                
                var character = await characterSet.GetByIdAsync(characterId);
                if (character == null)
                {
                    return NotFound(new ApiResponse<CharacterDto>
                    {
                        Success = false,
                        Message = $"Charakter mit ID {characterId} nicht gefunden"
                    });
                }

                // Update nur die Felder die nicht null sind
                if (update.Level.HasValue)
                    character.Level = update.Level.Value;
                if (update.MaxHealth.HasValue)
                    character.MaxHealth = update.MaxHealth.Value;
                if (update.CurrentHealth.HasValue)
                    character.CurrentHealth = update.CurrentHealth.Value;
                if (update.AttackPower.HasValue)
                    character.AttackPower = update.AttackPower.Value;
                if (update.Gold.HasValue)
                    character.Gold = update.Gold.Value;
                if (update.SpecialAttackLevel.HasValue)
                    character.SpecialAttackLevel = update.SpecialAttackLevel.Value;

                await characterSet.UpdateAsync(character.Id, character);

                var characterDto = await MapCharacterToDto(context, character, gameService);

                return Ok(new ApiResponse<CharacterDto>
                {
                    Success = true,
                    Message = "Charakter erfolgreich aktualisiert",
                    Data = characterDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CharacterDto>
                {
                    Success = false,
                    Message = "Fehler beim Aktualisieren des Charakters",
                    Error = ex.Message
                });
            }
        }

        #endregion

        #region Game Session Endpoints

        /// <summary>
        /// Starts a new game session.
        /// </summary>
        /// <param name="request">Game session start request</param>
        /// <returns>Created game session</returns>
        [HttpPost("session/start")]
        public async Task<ActionResult<ApiResponse<GameSessionDto>>> StartGameSession([FromBody] StartGameSessionRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);

                var session = await gameService.CreateGameSessionAsync(
                    request.CharacterId,
                    (Difficulty)request.Difficulty
                );

                var sessionDto = MapGameSessionToDto(session);

                return Ok(new ApiResponse<GameSessionDto>
                {
                    Success = true,
                    Message = $"Spiel-Session gestartet! {session.MaxFloor} Stockwerke warten auf dich!",
                    Data = sessionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<GameSessionDto>
                {
                    Success = false,
                    Message = "Fehler beim Starten der Session",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets game session information by ID.
        /// </summary>
        /// <param name="sessionId">Game session ID</param>
        /// <returns>Game session information</returns>
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<ApiResponse<GameSessionDto>>> GetGameSession(int sessionId)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var sessionSet = context.GameSessionSet;
                
                var session = await sessionSet.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return NotFound(new ApiResponse<GameSessionDto>
                    {
                        Success = false,
                        Message = $"Session mit ID {sessionId} nicht gefunden"
                    });
                }

                var sessionDto = MapGameSessionToDto(session);

                return Ok(new ApiResponse<GameSessionDto>
                {
                    Success = true,
                    Data = sessionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<GameSessionDto>
                {
                    Success = false,
                    Message = "Fehler beim Laden der Session",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets all game sessions for a character.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>List of game sessions</returns>
        [HttpGet("session/character/{characterId}")]
        public async Task<ActionResult<ApiResponse<List<GameSessionDto>>>> GetCharacterSessions(int characterId)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var sessionSet = context.GameSessionSet;
                
                var sessions = await sessionSet.GetAllAsync();
                var characterSessions = sessions
                    .Where(s => s.CharacterId == characterId)
                    .OrderByDescending(s => s.StartedAt)
                    .Select(MapGameSessionToDto)
                    .ToList();

                return Ok(new ApiResponse<List<GameSessionDto>>
                {
                    Success = true,
                    Data = characterSessions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<GameSessionDto>>
                {
                    Success = false,
                    Message = "Fehler beim Laden der Sessions",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates game session (for AI-driven game state management).
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="update">Session update data</param>
        /// <returns>Updated session</returns>
        [HttpPut("session/{sessionId}/update")]
        public async Task<ActionResult<ApiResponse<GameSessionDto>>> UpdateGameSession(
            int sessionId,
            [FromBody] WebApiGameSessionUpdateDto update)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var sessionSet = context.GameSessionSet;
                
                var session = await sessionSet.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return NotFound(new ApiResponse<GameSessionDto>
                    {
                        Success = false,
                        Message = $"Session mit ID {sessionId} nicht gefunden"
                    });
                }

                // Update nur die Felder die nicht null sind
                if (update.CurrentFloor.HasValue)
                    session.CurrentFloor = update.CurrentFloor.Value;
                if (update.IsCompleted.HasValue)
                    session.IsCompleted = update.IsCompleted.Value;

                await sessionSet.UpdateAsync(session.Id, session);

                var sessionDto = MapGameSessionToDto(session);

                return Ok(new ApiResponse<GameSessionDto>
                {
                    Success = true,
                    Message = "Session erfolgreich aktualisiert",
                    Data = sessionDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<GameSessionDto>
                {
                    Success = false,
                    Message = "Fehler beim Aktualisieren der Session",
                    Error = ex.Message
                });
            }
        }

        #endregion

        #region Combat Endpoints

        /// <summary>
        /// Starts a new fight and generates an enemy.
        /// </summary>
        /// <param name="request">Fight start request</param>
        /// <returns>Enemy information</returns>
        [HttpPost("fight/start")]
        public async Task<ActionResult<ApiResponse<EnemyDto>>> StartFight([FromBody] StartFightRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var sessionSet = context.GameSessionSet;
                
                var session = await sessionSet.GetByIdAsync(request.GameSessionId);
                if (session == null)
                {
                    return NotFound(new ApiResponse<EnemyDto>
                    {
                        Success = false,
                        Message = $"Session mit ID {request.GameSessionId} nicht gefunden"
                    });
                }

                // Check if current floor is a boss fight (every 5th floor)
                bool isBoss = session.CurrentFloor % 5 == 0;

                var enemy = gameService.GenerateEnemy(session.CurrentFloor, isBoss);
                var enemyDto = MapEnemyToDto(enemy);

                string message = isBoss
                    ? $"🔥 BOSS KAMPF auf Stockwerk {session.CurrentFloor}! 🔥\n{enemy.Race} {enemy.Type} erscheint mit seiner besonderen Fähigkeit!"
                    : $"Stockwerk {session.CurrentFloor}: Ein wilder {enemy.Race} {enemy.Type} erscheint!";

                return Ok(new ApiResponse<EnemyDto>
                {
                    Success = true,
                    Message = message,
                    Data = enemyDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<EnemyDto>
                {
                    Success = false,
                    Message = "Fehler beim Starten des Kampfes",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Executes a combat action (attack or special attack).
        /// </summary>
        /// <param name="request">Combat action request</param>
        /// <returns>Combat result</returns>
        [HttpPost("fight/action")]
        public async Task<ActionResult<ApiResponse<CombatResultDto>>> ExecuteCombatAction([FromBody] CombatActionRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var sessionSet = context.GameSessionSet;
                var characterSet = context.CharacterSet;

                var session = await sessionSet.GetByIdAsync(request.GameSessionId);
                if (session == null)
                {
                    return NotFound(new ApiResponse<CombatResultDto>
                    {
                        Success = false,
                        Message = $"Session mit ID {request.GameSessionId} nicht gefunden"
                    });
                }

                // Map enemy DTO to enemy data
                var enemy = new Logic.Modules.EnemyData
                {
                    Type = request.Enemy.Type,
                    Race = request.Enemy.Race,
                    Level = request.Enemy.Level,
                    Health = request.Enemy.Health,
                    AttackPower = request.Enemy.AttackPower,
                    Weapon = request.Enemy.Weapon,
                    IsBoss = request.Enemy.IsBoss,
                    HasSpecialAttack = request.Enemy.HasSpecialAttack
                };

                // Execute combat
                var result = await gameService.SimulateCombatRoundAsync(
                    request.CharacterId,
                    enemy,
                    request.UseSpecialAttack
                );

                var character = await characterSet.GetByIdAsync(request.CharacterId);
                var resultDto = new CombatResultDto
                {
                    DamageToEnemy = result.DamageToEnemy,
                    DamageToCharacter = result.DamageToCharacter,
                    RemainingEnemyHealth = result.RemainingEnemyHealth,
                    RemainingCharacterHealth = result.RemainingCharacterHealth,
                    IsEnemyDefeated = result.IsEnemyDefeated,
                    IsCharacterDefeated = result.IsCharacterDefeated,
                    SpecialAttackUsed = result.SpecialAttackUsed,
                    BossSpecialAttackUsed = result.BossSpecialAttackUsed,
                    Enemy = MapEnemyToDto(enemy),
                    Character = await MapCharacterToDto(context, character, gameService)
                };

                // Build message
                string message = $"⚔️ {character.Name} verursacht {result.DamageToEnemy} Schaden!\n";
                
                if (result.SpecialAttackUsed)
                {
                    message += $"💥 SPEZIALANGRIFF verwendet!\n";
                }

                message += $"⚔️ {request.Enemy.Race} {request.Enemy.Type} verursacht {result.DamageToCharacter} Schaden!\n";
                
                if (result.BossSpecialAttackUsed)
                {
                    message += $"🔥 BOSS SPEZIALANGRIFF!\n";
                }

                // Handle victory
                if (result.IsEnemyDefeated)
                {
                    // Record defeated enemy
                    await gameService.RecordDefeatedEnemyAsync(
                        request.GameSessionId,
                        session.CurrentFloor,
                        enemy
                    );

                    // Award rewards
                    var rewards = await ProcessRewardsAsync(
                        context,
                        gameService,
                        request.CharacterId,
                        session,
                        enemy.IsBoss
                    );

                    resultDto.Rewards = rewards;
                    message += $"\n🏆 SIEG!\n{rewards.Message}";

                    // Advance to next floor
                    await gameService.AdvanceFloorAsync(request.GameSessionId);
                }
                else if (result.IsCharacterDefeated)
                {
                    message += "\n💀 NIEDERLAGE! Du wurdest besiegt!";
                }

                resultDto.Message = message;

                return Ok(new ApiResponse<CombatResultDto>
                {
                    Success = true,
                    Data = resultDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<CombatResultDto>
                {
                    Success = false,
                    Message = "Fehler beim Ausführen der Kampfaktion",
                    Error = ex.Message
                });
            }
        }

        #endregion

        #region Weapon Endpoints

        /// <summary>
        /// Upgrades a weapon.
        /// </summary>
        /// <param name="request">Upgrade weapon request</param>
        /// <returns>Success status</returns>
        [HttpPost("weapon/upgrade")]
        public async Task<ActionResult<ApiResponse<WeaponDto>>> UpgradeWeapon([FromBody] UpgradeWeaponRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);

                var success = await gameService.UpgradeWeaponAsync(request.WeaponId, request.CharacterId);

                if (!success)
                {
                    return BadRequest(new ApiResponse<WeaponDto>
                    {
                        Success = false,
                        Message = "Upgrade fehlgeschlagen. Nicht genug Gold oder maximales Level erreicht."
                    });
                }

                var weaponSet = context.WeaponSet;
                var weapon = await weaponSet.GetByIdAsync(request.WeaponId);
                var weaponDto = MapWeaponToDto(weapon!);

                return Ok(new ApiResponse<WeaponDto>
                {
                    Success = true,
                    Message = $"Waffe '{weapon!.Name}' erfolgreich auf Level {weapon.UpgradeLevel} aufgewertet!",
                    Data = weaponDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<WeaponDto>
                {
                    Success = false,
                    Message = "Fehler beim Aufwerten der Waffe",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Sells a weapon for gold.
        /// </summary>
        /// <param name="request">Sell weapon request</param>
        /// <returns>Success status</returns>
        [HttpPost("weapon/sell")]
        public async Task<ActionResult<ApiResponse<object>>> SellWeapon([FromBody] SellWeaponRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);
                var weaponSet = context.WeaponSet;
                
                var weapon = await weaponSet.GetByIdAsync(request.WeaponId);
                if (weapon == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Waffe mit ID {request.WeaponId} nicht gefunden"
                    });
                }

                int sellValue = weapon.SellValue;
                await gameService.SellWeaponAsync(request.WeaponId, request.CharacterId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Waffe verkauft für {sellValue} Gold!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fehler beim Verkaufen der Waffe",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Equips or unequips a weapon.
        /// </summary>
        /// <param name="request">Equip weapon request</param>
        /// <returns>Success status</returns>
        [HttpPost("weapon/equip")]
        public async Task<ActionResult<ApiResponse<WeaponDto>>> EquipWeapon([FromBody] EquipWeaponRequest request)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var gameService = new GameService(context);

                if (request.Equip)
                {
                    await gameService.EquipWeaponAsync(request.WeaponId);
                }
                else
                {
                    await gameService.UnequipWeaponAsync(request.WeaponId);
                }

                var weaponSet = context.WeaponSet;
                var weapon = await weaponSet.GetByIdAsync(request.WeaponId);
                var weaponDto = MapWeaponToDto(weapon!);

                return Ok(new ApiResponse<WeaponDto>
                {
                    Success = true,
                    Message = request.Equip 
                        ? $"Waffe '{weapon!.Name}' ausgerüstet!" 
                        : $"Waffe '{weapon!.Name}' abgelegt!",
                    Data = weaponDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<WeaponDto>
                {
                    Success = false,
                    Message = "Fehler beim Ausrüsten der Waffe",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets all weapons for a character.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>List of weapons</returns>
        [HttpGet("weapon/character/{characterId}")]
        public async Task<ActionResult<ApiResponse<List<WeaponDto>>>> GetCharacterWeapons(int characterId)
        {
            try
            {
                using var context = _contextAccessor.GetContext();
                var weaponSet = context.WeaponSet;
                
                var weapons = await weaponSet.GetAllAsync();
                var characterWeapons = weapons
                    .Where(w => w.CharacterId == characterId)
                    .Select(MapWeaponToDto)
                    .ToList();

                return Ok(new ApiResponse<List<WeaponDto>>
                {
                    Success = true,
                    Data = characterWeapons
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<WeaponDto>>
                {
                    Success = false,
                    Message = "Fehler beim Laden der Waffen",
                    Error = ex.Message
                });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Processes rewards after defeating an enemy.
        /// </summary>
        private async Task<RewardsDto> ProcessRewardsAsync(
            IContext context,
            GameService gameService,
            int characterId,
            GameSession session,
            bool isBoss)
        {
            var rewards = new RewardsDto
            {
                LevelUp = true,
                FullHeal = true
            };

            // Award gold
            int goldReward = isBoss ? session.CurrentFloor * 50 : session.CurrentFloor * 10;
            await gameService.AddGoldAsync(characterId, goldReward);
            rewards.GoldReward = goldReward;

            // Level up
            await gameService.LevelUpCharacterAsync(characterId);

            // New weapon every 3 fights
            var defeatedEnemySet = context.DefeatedEnemySet;
            var defeatedEnemies = await defeatedEnemySet.GetAllAsync();
            int totalDefeated = defeatedEnemies.Count(e => e.GameSessionId == session.Id);

            if (totalDefeated % 3 == 0)
            {
                int weaponCount = await gameService.GetWeaponCountAsync(characterId);
                if (weaponCount < 5)
                {
                    var weapon = await gameService.GenerateWeaponForCharacterAsync(characterId);
                    rewards.NewWeapon = true;
                    rewards.Weapon = MapWeaponToDto(weapon);
                }
            }

            // Boss rewards: upgrade special attack
            if (isBoss)
            {
                await gameService.UpgradeSpecialAttackAsync(characterId);
                rewards.SpecialAttackUpgrade = true;
            }

            // Build message
            var messages = new List<string>();
            messages.Add($"💰 {goldReward} Gold erhalten!");
            messages.Add("⬆️ Level Up!");
            messages.Add("❤️ Vollständig geheilt!");
            
            if (rewards.NewWeapon)
            {
                messages.Add($"⚔️ Neue Waffe erhalten: {rewards.Weapon!.Name}!");
            }
            
            if (rewards.SpecialAttackUpgrade)
            {
                messages.Add("🌟 Spezialangriff aufgewertet!");
            }

            rewards.Message = string.Join("\n", messages);

            return rewards;
        }

        /// <summary>
        /// Maps Character entity to DTO.
        /// </summary>
        private async Task<CharacterDto> MapCharacterToDto(IContext context, Character character, GameService gameService)
        {
            var weaponSet = context.WeaponSet;
            var weapons = await weaponSet.GetAllAsync();
            var characterWeapons = weapons.Where(w => w.CharacterId == character.Id).ToList();

            return new CharacterDto
            {
                Id = character.Id,
                Name = character.Name,
                CharacterClass = (int)character.Class,
                ClassName = character.Class.ToString(),
                Level = character.Level,
                MaxHealth = character.MaxHealth,
                CurrentHealth = character.CurrentHealth,
                AttackPower = character.AttackPower,
                SpecialAttackLevel = character.SpecialAttackLevel,
                Gold = character.Gold,
                TotalAttackPower = await gameService.GetTotalAttackPowerAsync(character.Id),
                Weapons = characterWeapons.Select(MapWeaponToDto).ToList()
            };
        }

        /// <summary>
        /// Maps GameSession entity to DTO.
        /// </summary>
        private GameSessionDto MapGameSessionToDto(GameSession session)
        {
            return new GameSessionDto
            {
                Id = session.Id,
                CharacterId = session.CharacterId,
                Difficulty = (int)session.Difficulty,
                CurrentFloor = session.CurrentFloor,
                MaxFloor = session.MaxFloor,
                IsCompleted = session.IsCompleted,
                StartedAt = session.StartedAt,
                CompletedAt = session.CompletedAt
            };
        }

        /// <summary>
        /// Maps EnemyData to DTO.
        /// </summary>
        private EnemyDto MapEnemyToDto(Logic.Modules.EnemyData enemy)
        {
            return new EnemyDto
            {
                Type = enemy.Type,
                Race = enemy.Race,
                Level = enemy.Level,
                Health = enemy.Health,
                MaxHealth = enemy.Health,
                AttackPower = enemy.AttackPower,
                Weapon = enemy.Weapon,
                IsBoss = enemy.IsBoss,
                HasSpecialAttack = enemy.HasSpecialAttack
            };
        }

        /// <summary>
        /// Maps Weapon entity to DTO.
        /// </summary>
        private WeaponDto MapWeaponToDto(Weapon weapon)
        {
            return new WeaponDto
            {
                Id = weapon.Id,
                Name = weapon.Name,
                Type = weapon.Type,
                DamageBonus = weapon.DamageBonus,
                UpgradeLevel = weapon.UpgradeLevel,
                SuitableForClass = (int)weapon.SuitableForClass,
                IsEquipped = weapon.IsEquipped,
                SellValue = weapon.SellValue,
                UpgradeCost = weapon.UpgradeCost
            };
        }

        #endregion
    }
}
#endif







