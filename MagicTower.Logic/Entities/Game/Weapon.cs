
//@CustomCode
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MagicTower.Logic.Contracts;
using MagicTower.Common.Models.Game;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Represents a weapon that can be owned by a character.
    /// </summary>
    [Table("Weapons", Schema = "game")]
    public partial class Weapon : VersionEntityObject
    {
        /// <summary>
        /// Gets or sets the character ID who owns this weapon.
        /// </summary>
        public IdType CharacterId { get; set; }
        
        /// <summary>
        /// Gets or sets the weapon name.
        /// </summary>
        [MaxLength(128)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the weapon type (Schwert, Axt, Bogen, etc.).
        /// </summary>
        [MaxLength(64)]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the damage bonus this weapon provides.
        /// </summary>
        public int DamageBonus { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the weapon upgrade level.
        /// </summary>
        public int UpgradeLevel { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the character class this weapon is suited for.
        /// </summary>
        public CharacterClass SuitableForClass { get; set; }
        
        /// <summary>
        /// Gets or sets whether this weapon is currently equipped.
        /// </summary>
        public bool IsEquipped { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the sell value in gold.
        /// </summary>
        public int SellValue { get; set; } = 10;
        
        /// <summary>
        /// Gets or sets the cost to upgrade to the next level.
        /// </summary>
        public int UpgradeCost { get; set; } = 50;

        #region Navigation properties
        /// <summary>
        /// Gets or sets the character who owns this weapon.
        /// </summary>
        public Character Character { get; internal set; } = null!;
        #endregion Navigation properties
    }
}

