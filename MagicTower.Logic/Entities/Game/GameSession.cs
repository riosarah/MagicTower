//@CustomCode
using System.ComponentModel.DataAnnotations.Schema;
using MagicTower.Logic.Contracts;
using MagicTower.Common.Models.Game;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Represents a game session where a character progresses through the tower.
    /// </summary>
    [Table("GameSessions", Schema = "game")]
    public partial class GameSession : VersionEntityObject
    {
        /// <summary>
        /// Gets or sets the character ID.
        /// </summary>
        public IdType CharacterId { get; set; }
        
        /// <summary>
        /// Gets or sets the difficulty level.
        /// </summary>
        public Difficulty Difficulty { get; set; }
        
        /// <summary>
        /// Gets or sets the current floor number.
        /// </summary>
        public int CurrentFloor { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets the maximum floor for this difficulty.
        /// </summary>
        public int MaxFloor { get; set; }
        
        /// <summary>
        /// Gets or sets whether the session is completed.
        /// </summary>
        public bool IsCompleted { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the session start time.
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the session completion time.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        #region Navigation properties
        /// <summary>
        /// Gets or sets the character.
        /// </summary>
        public Character Character { get; internal set; } = null!;
        
        /// <summary>
        /// Gets or sets the list of defeated enemies.
        /// </summary>
        public List<DefeatedEnemy> DefeatedEnemies { get; internal set; } = [];
        #endregion Navigation properties

        public GameSession()
        {
            Difficulty = new Difficulty();
            CurrentFloor = 1;
            MaxFloor = 10;
            IsCompleted = false;
        }
    }
}
