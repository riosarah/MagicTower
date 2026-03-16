//@CustomCode
namespace MagicTower.Common.Contracts.Game
{
    /// <summary>
    /// Defines the contract for a chat message entity.
    /// </summary>
    public partial interface IChatMessage : IIdentifiable, IVersionable
    {
        /// <summary>
        /// Gets or sets the game session ID this message belongs to.
        /// </summary>
        IdType GameSessionId { get; set; }
        
        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        string Message { get; set; }
        
        /// <summary>
        /// Gets or sets whether this message is from the AI (true) or the user (false).
        /// </summary>
        bool IsAiMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the message was sent.
        /// </summary>
        DateTime SentAt { get; set; }
        
        /// <summary>
        /// Gets or sets the sequence number of the message in the conversation.
        /// </summary>
        int SequenceNumber { get; set; }
    }
}
