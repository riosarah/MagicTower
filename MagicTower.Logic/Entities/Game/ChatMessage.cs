
//@CustomCode
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MagicTower.Logic.Contracts;
using MagicTower.Common.Contracts.Game;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Represents a single chat message in a game session conversation.
    /// </summary>
    [Table("ChatMessages", Schema = "game")]
    public partial class ChatMessage : VersionEntityObject, IChatMessage
    {
        /// <summary>
        /// Gets or sets the game session ID this message belongs to.
        /// </summary>
        public IdType GameSessionId { get; set; }
        
        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether this message is from the AI (true) or the user (false).
        /// </summary>
        public bool IsAiMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the message was sent.
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the sequence number of the message in the conversation.
        /// Used to maintain correct ordering.
        /// </summary>
        public int SequenceNumber { get; set; }

        #region Navigation properties
        /// <summary>
        /// Gets or sets the game session this message belongs to.
        /// </summary>
        public GameSession GameSession { get; internal set; } = null!;
        #endregion Navigation properties
    }
}
