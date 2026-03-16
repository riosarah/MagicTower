//@CustomCode
using MagicTower.Logic.Entities.Game;
using MagicTower.WebApi.Contracts;
using MagicTower.WebApi.Models;
using System.Text.Json;
using ChatMessageEntity = MagicTower.Logic.Entities.Game.ChatMessage;
using ChatMessageDto = MagicTower.WebApi.Models.ChatMessage;

namespace MagicTower.WebApi.Extensions
{
    /// <summary>
    /// Extension methods for GameSession to handle chat history and enemy state persistence.
    /// </summary>
    public static class GameSessionExtensions
    {
        /// <summary>
        /// Adds a new chat message to the session.
        /// </summary>
        /// <param name="session">Game session</param>
        /// <param name="contextAccessor">Context accessor</param>
        /// <param name="message">Message content</param>
        /// <param name="isAiMessage">True if AI message, false if user message</param>
        public static async Task AddChatMessageAsync(
            this GameSession session, 
            IContextAccessor contextAccessor,
            string message, 
            bool isAiMessage)
        {
            using var context = contextAccessor.GetContext();
            var chatMessageSet = context.ChatMessageSet;

            // Get the next sequence number
            var existingMessages = await chatMessageSet.GetAllAsync();
            var sessionMessages = existingMessages
                .Where(m => m.GameSessionId == session.Id)
                .ToList();
            
            int nextSequenceNumber = sessionMessages.Any() 
                ? sessionMessages.Max(m => m.SequenceNumber) + 1 
                : 1;

            var chatMessage = new ChatMessageEntity
            {
                GameSessionId = session.Id,
                Message = message,
                IsAiMessage = isAiMessage,
                SentAt = DateTime.UtcNow,
                SequenceNumber = nextSequenceNumber
            };

            await chatMessageSet.AddAsync(chatMessage);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets all chat messages for the session, ordered by sequence number.
        /// </summary>
        public static async Task<List<ChatMessageDto>> GetChatHistoryAsync(
            this GameSession session,
            IContextAccessor contextAccessor)
        {
            using var context = contextAccessor.GetContext();
            var chatMessageSet = context.ChatMessageSet;

            var allMessages = await chatMessageSet.GetAllAsync();
            var sessionMessages = allMessages
                .Where(m => m.GameSessionId == session.Id)
                .OrderBy(m => m.SequenceNumber)
                .Select(m => new ChatMessageDto
                {
                    Role = m.IsAiMessage ? "assistant" : "user",
                    Content = m.Message
                })
                .ToList();

            return sessionMessages;
        }

        /// <summary>
        /// Gets the last N chat messages for the session.
        /// </summary>
        public static async Task<List<ChatMessageDto>> GetRecentChatHistoryAsync(
            this GameSession session,
            IContextAccessor contextAccessor,
            int count = 10)
        {
            using var context = contextAccessor.GetContext();
            var chatMessageSet = context.ChatMessageSet;

            var allMessages = await chatMessageSet.GetAllAsync();
            var sessionMessages = allMessages
                .Where(m => m.GameSessionId == session.Id)
                .OrderByDescending(m => m.SequenceNumber)
                .Take(count)
                .OrderBy(m => m.SequenceNumber)
                .Select(m => new ChatMessageDto
                {
                    Role = m.IsAiMessage ? "assistant" : "user",
                    Content = m.Message
                })
                .ToList();

            return sessionMessages;
        }

        /// <summary>
        /// Saves the current enemy state to the GameSession as JSON.
        /// </summary>
        public static void SaveCurrentEnemy(this GameSession session, EnemyDto? enemy)
        {
            if (enemy == null)
            {
                session.CurrentEnemyState = null;
                return;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            session.CurrentEnemyState = JsonSerializer.Serialize(enemy, jsonOptions);
        }

        /// <summary>
        /// Loads the current enemy state from the GameSession JSON.
        /// </summary>
        public static EnemyDto? LoadCurrentEnemy(this GameSession session)
        {
            if (string.IsNullOrEmpty(session.CurrentEnemyState))
            {
                return null;
            }

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var enemy = JsonSerializer.Deserialize<EnemyDto>(session.CurrentEnemyState, jsonOptions);
                return enemy;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing enemy state: {ex.Message}");
                return null;
            }
        }
    }
}


