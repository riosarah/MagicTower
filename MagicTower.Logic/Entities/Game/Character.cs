
//@CustomCode
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MagicTower.Logic.Contracts;
using MagicTower.Common.Models.Game;
using Microsoft.EntityFrameworkCore;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Represents a player character in the game.
    /// </summary>
    [Table("Characters", Schema = "game")]
    public partial class Character : VersionEntityObject
    {
        /// <summary>
        /// Gets or sets the character name.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the character class.
        /// </summary>
        public CharacterClass Class { get; set; }
        
        /// <summary>
        /// Gets or sets the character level.
        /// </summary>
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets the maximum health points.
        /// </summary>
        public int MaxHealth { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets the current health points.
        /// </summary>
        public int CurrentHealth { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets the base attack power.
        /// </summary>
        public int AttackPower { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the special attack level (upgraded after boss fights).
        /// </summary>
        public int SpecialAttackLevel { get; set; } = 1;
        
        /// <summary>
        /// Gets or sets the amount of gold.
        /// </summary>
        public int Gold { get; set; } = 0;

        #region Navigation properties
        /// <summary>
        /// Gets or sets the list of game sessions.
        /// </summary>
        public List<GameSession> GameSessions { get; internal set; } = [];
        
        /// <summary>
        /// Gets or sets the list of weapons owned by this character.
        /// </summary>
        public List<Weapon> Weapons { get; internal set; } = [];
        #endregion Navigation properties
        public Character()
        {
            Name = "Hero";
            Class = new CharacterClass();
            Level = 1;
            MaxHealth = 100;
            CurrentHealth = 100;
            AttackPower = 10;
            SpecialAttackLevel = 1;
            Gold = 0;
        }
    }
}
