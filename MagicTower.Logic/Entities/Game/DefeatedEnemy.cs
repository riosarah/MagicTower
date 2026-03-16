
//@CustomCode
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MagicTower.Logic.Contracts;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Represents a defeated enemy in a game session.
    /// </summary>
    [Table("DefeatedEnemies", Schema = "game")]
    public partial class DefeatedEnemy : VersionEntityObject
    {
        /// <summary>
        /// Gets or sets the game session ID.
        /// </summary>
        public IdType GameSessionId { get; set; }
        
        /// <summary>
        /// Gets or sets the floor number where this enemy was defeated.
        /// </summary>
        public int FloorNumber { get; set; }
        
        /// <summary>
        /// Gets or sets whether this was a boss fight.
        /// </summary>
        public bool IsBoss { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the enemy type (Ritter, König, etc.).
        /// </summary>
        [MaxLength(64)]
        public string EnemyType { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the enemy race (Ork, Goblin, etc.).
        /// </summary>
        [MaxLength(64)]
        public string EnemyRace { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the enemy level.
        /// </summary>
        public int EnemyLevel { get; set; }
        
        /// <summary>
        /// Gets or sets the enemy weapon.
        /// </summary>
        [MaxLength(64)]
        public string EnemyWeapon { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the gold reward received.
        /// </summary>
        public int GoldReward { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the enemy was defeated.
        /// </summary>
        public DateTime DefeatedAt { get; set; } = DateTime.UtcNow;

        #region Navigation properties
        /// <summary>
        /// Gets or sets the game session.
        /// </summary>
        public GameSession GameSession { get; internal set; } = null!;
        #endregion Navigation properties
    }
}
