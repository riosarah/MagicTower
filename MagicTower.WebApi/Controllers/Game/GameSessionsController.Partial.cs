#if GENERATEDCODE_ON
//@CustomCode
using Microsoft.AspNetCore.Mvc;
using MagicTower.Logic.Contracts;
using System.Diagnostics;

namespace MagicTower.WebApi.Controllers.Game
{
    using TModel = MagicTower.WebApi.Models.Game.GameSession;
    using TEntity = MagicTower.Logic.Entities.Game.GameSession;

    /// <summary>
    /// Partial class extension for GameSessionsController with debugging capabilities.
    /// </summary>
    public sealed partial class GameSessionsController
    {
        /// <summary>
        /// Gets a game session by ID with ChatMessages included.
        /// Custom endpoint: GET /api/GameSessions/{id}/with-chat
        /// </summary>
        /// <param name="id">The game session ID</param>
        /// <returns>Game session with chat messages</returns>
        [HttpGet("{id}/with-chat")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TModel?>> GetByIdWithChatAsync(IdType id)
        {
            try
            {
                Console.WriteLine($"=== GameSession GET BY ID WITH CHAT {id} START ===");
                Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                // Get the entity
                var entity = await EntitySet.GetByIdAsync(id);
                
                if (entity == null)
                {
                    Console.WriteLine($"!!! GameSession {id} NOT FOUND !!!");
                    return NotFound();
                }

                Console.WriteLine($"GameSession {id} found");
                Console.WriteLine($"  CharacterId: {entity.CharacterId}");
                Console.WriteLine($"  CurrentFloor: {entity.CurrentFloor}/{entity.MaxFloor}");
                
                // Convert to model first
                var model = ToModel(entity);
                
                // Load ChatMessages explicitly and add to model
                var chatMessageSet = Context.ChatMessageSet;
                var allMessages = await chatMessageSet.GetAllAsync();
                var sessionMessages = allMessages
                    .Where(m => m.GameSessionId == id)
                    .OrderBy(m => m.SequenceNumber)
                    .ToList();
                
                // Convert entity chat messages to model chat messages
                model.ChatMessages = sessionMessages
                    .Select(cm => new MagicTower.WebApi.Models.Game.ChatMessage
                    {
                        Id = cm.Id,
                        GameSessionId = cm.GameSessionId,
                        Message = cm.Message,
                        IsAiMessage = cm.IsAiMessage,
                        SentAt = cm.SentAt,
                        SequenceNumber = cm.SequenceNumber
                    })
                    .ToList();
                
                Console.WriteLine($"  Loaded {model.ChatMessages.Count} ChatMessages");
                Console.WriteLine("=== GameSession GET BY ID WITH CHAT END ===");
                Console.WriteLine();

                return Ok(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GameSession GET BY ID WITH CHAT ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("============================================");
                
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets all game sessions with detailed logging for debugging.
        /// </summary>
        /// <returns>List of all game sessions</returns>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public override async Task<ActionResult<IEnumerable<TModel>>> GetAsync()
        {
            try
            {
                Console.WriteLine("=== GameSessions DEBUG GET START ===");
                Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                var stopwatch = Stopwatch.StartNew();

                // Get all entities
                var entities = await EntitySet.GetAllAsync();
                var entityCount = entities.Count();
                
                Console.WriteLine($"Retrieved {entityCount} entities from database");

                // Convert to models
                var models = entities.Select(e => ToModel(e)).ToList();
                
                stopwatch.Stop();
                Console.WriteLine($"Converted to {models.Count} models");
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");

                // Log first session details if any exist
                if (models.Any())
                {
                    var first = models.First();
                    Console.WriteLine("=== First Session Details ===");
                    Console.WriteLine($"ID: {first.Id}");
                    Console.WriteLine($"CharacterId: {first.CharacterId}");
                    Console.WriteLine($"Difficulty: {first.Difficulty}");
                    Console.WriteLine($"CurrentFloor: {first.CurrentFloor}/{first.MaxFloor}");
                    Console.WriteLine($"IsCompleted: {first.IsCompleted}");
                    Console.WriteLine($"StartedAt: {first.StartedAt:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"CompletedAt: {first.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL"}");
                }
                else
                {
                    Console.WriteLine("!!! NO SESSIONS FOUND IN DATABASE !!!");
                }

                Console.WriteLine("=== GameSessions DEBUG GET END ===");
                Console.WriteLine();

                return Ok(models);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GameSessions DEBUG GET ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("====================================");
                
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets sessions for a specific character with detailed logging.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>List of game sessions for the character</returns>
        [HttpGet("character/{characterId}/debug")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TModel>>> GetByCharacterWithDebug(IdType characterId)
        {
            try
            {
                Console.WriteLine($"=== GameSessions DEBUG GET BY CHARACTER {characterId} START ===");
                Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

                var stopwatch = Stopwatch.StartNew();

                // Get all entities
                var allEntities = await EntitySet.GetAllAsync();
                Console.WriteLine($"Total entities in database: {allEntities.Count()}");

                // Filter by character
                var filteredEntities = allEntities.Where(s => s.CharacterId == characterId).ToList();
                Console.WriteLine($"Sessions for character {characterId}: {filteredEntities.Count}");

                // Convert to models and sort
                var models = filteredEntities
                    .Select(e => ToModel(e))
                    .OrderByDescending(s => s.StartedAt)
                    .ToList();
                
                stopwatch.Stop();
                Console.WriteLine($"Converted and sorted {models.Count} models");
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");

                // Log all sessions for this character
                Console.WriteLine($"=== All {models.Count} Sessions for Character {characterId} ===");
                foreach (var session in models)
                {
                    Console.WriteLine($"  Session {session.Id}: Floor {session.CurrentFloor}/{session.MaxFloor}, " +
                                    $"Started: {session.StartedAt:yyyy-MM-dd HH:mm:ss}, " +
                                    $"Completed: {(session.IsCompleted ? "Yes" : "No")}");
                }

                Console.WriteLine("=== GameSessions DEBUG GET BY CHARACTER END ===");
                Console.WriteLine();

                return Ok(models);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GameSessions DEBUG GET BY CHARACTER ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("================================================");
                
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Creates a test game session for debugging purposes.
        /// </summary>
        /// <param name="characterId">Character ID to associate with the session</param>
        /// <returns>Created game session</returns>
        [HttpPost("test/{characterId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<TModel>> CreateTestSession(IdType characterId)
        {
            try
            {
                Console.WriteLine($"=== Creating Test GameSession for Character {characterId} ===");

                var testSession = new TEntity
                {
                    CharacterId = characterId,
                    Difficulty = MagicTower.Common.Models.Game.Difficulty.Medium,
                    CurrentFloor = 1,
                    MaxFloor = 20,
                    IsCompleted = false,
                    StartedAt = DateTime.UtcNow
                };

                await EntitySet.AddAsync(testSession);
                await Context.SaveChangesAsync();

                Console.WriteLine($"Test session created with ID: {testSession.Id}");
                Console.WriteLine("=========================================");

                var model = ToModel(testSession);
                return CreatedAtAction("GetById", new { id = testSession.Id }, model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== Error Creating Test Session ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("===================================");
                
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets paged game sessions with sorting and filtering.
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="characterId">Optional: Filter by character ID</param>
        /// <param name="isCompleted">Optional: Filter by completion status</param>
        /// <param name="sortBy">Sort field: StartedAt, CurrentFloor, Difficulty (default: StartedAt)</param>
        /// <param name="sortOrder">Sort order: asc or desc (default: desc)</param>
        /// <returns>Paged list of game sessions</returns>
        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TModel>>> GetPagedAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? characterId = null,
            [FromQuery] bool? isCompleted = null,
            [FromQuery] string sortBy = "StartedAt",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                Console.WriteLine("=== GameSessions PAGED GET START ===");
                Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"PageNumber: {pageNumber}, PageSize: {pageSize}");
                Console.WriteLine($"CharacterId Filter: {characterId?.ToString() ?? "None"}");
                Console.WriteLine($"IsCompleted Filter: {isCompleted?.ToString() ?? "None"}");
                Console.WriteLine($"Sort: {sortBy} {sortOrder}");

                var stopwatch = Stopwatch.StartNew();

                // Validate parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Max page size

                // Get all entities
                var allEntities = await EntitySet.GetAllAsync();
                Console.WriteLine($"Total entities in database: {allEntities.Count()}");

                // Apply filters
                IEnumerable<TEntity> filteredEntities = allEntities;

                if (characterId.HasValue)
                {
                    filteredEntities = filteredEntities.Where(s => s.CharacterId == characterId.Value);
                    Console.WriteLine($"After CharacterId filter: {filteredEntities.Count()} entities");
                }

                if (isCompleted.HasValue)
                {
                    filteredEntities = filteredEntities.Where(s => s.IsCompleted == isCompleted.Value);
                    Console.WriteLine($"After IsCompleted filter: {filteredEntities.Count()} entities");
                }

                // Apply sorting
                filteredEntities = sortBy.ToLower() switch
                {
                    "currentfloor" => sortOrder.ToLower() == "asc"
                        ? filteredEntities.OrderBy(s => s.CurrentFloor)
                        : filteredEntities.OrderByDescending(s => s.CurrentFloor),
                    "difficulty" => sortOrder.ToLower() == "asc"
                        ? filteredEntities.OrderBy(s => s.Difficulty)
                        : filteredEntities.OrderByDescending(s => s.Difficulty),
                    "completedat" => sortOrder.ToLower() == "asc"
                        ? filteredEntities.OrderBy(s => s.CompletedAt ?? DateTime.MaxValue)
                        : filteredEntities.OrderByDescending(s => s.CompletedAt ?? DateTime.MinValue),
                    _ => sortOrder.ToLower() == "asc" // Default: StartedAt
                        ? filteredEntities.OrderBy(s => s.StartedAt)
                        : filteredEntities.OrderByDescending(s => s.StartedAt)
                };

                // Calculate totals
                var totalCount = filteredEntities.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                Console.WriteLine($"Total filtered entities: {totalCount}");
                Console.WriteLine($"Total pages: {totalPages}");

                // Apply pagination
                var pagedEntities = filteredEntities
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Console.WriteLine($"Entities on current page: {pagedEntities.Count}");

                // Convert to models
                var models = pagedEntities.Select(e => ToModel(e)).ToList();

                stopwatch.Stop();
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine("=== GameSessions PAGED GET END ===");
                Console.WriteLine();

                var result = new PagedResult<TModel>
                {
                    Items = models,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GameSessions PAGED GET ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine("====================================");

                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Gets database connection info for debugging.
        /// </summary>
        /// <returns>Database information</returns>
        [HttpGet("dbinfo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetDatabaseInfo()
        {
            try
            {
                var info = new
                {
                    contextType = Context.GetType().Name,
                    entitySetType = EntitySet.GetType().Name,
                    timestamp = DateTime.UtcNow,
                    message = "Database connection is working"
                };

                Console.WriteLine("=== Database Info Request ===");
                Console.WriteLine($"Context: {info.contextType}");
                Console.WriteLine($"EntitySet: {info.entitySetType}");
                Console.WriteLine("=============================");

                return Ok(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== Database Info Error ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("===========================");
                
                return BadRequest(new
                {
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }
    }

    /// <summary>
    /// Represents a paged result for API responses.
    /// </summary>
    /// <typeparam name="T">Type of items in the result</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The items on the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Indicates if there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }
    }
}
#endif
