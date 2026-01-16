//@CustomCode
using MagicTower.Logic.Contracts;
using MagicTower.Common.Modules.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Validation logic for Weapon entity.
    /// </summary>
    public partial class Weapon : IValidatableEntity
    {
        private const int MinNameLength = 2;
        private const int MinUpgradeLevel = 0;

        public void Validate(IContext context, EntityState entityState)
        {
            var errors = new List<string>();

            if (CharacterId <= 0)
                errors.Add($"{nameof(CharacterId)} must be a valid reference.");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add($"{nameof(Name)} must not be empty.");
            else if (Name.Length < MinNameLength)
                errors.Add($"{nameof(Name)} must be at least {MinNameLength} characters long.");

            if (string.IsNullOrWhiteSpace(Type))
                errors.Add($"{nameof(Type)} must not be empty.");

            if (DamageBonus < 0)
                errors.Add($"{nameof(DamageBonus)} cannot be negative.");

            if (UpgradeLevel < MinUpgradeLevel)
                errors.Add($"{nameof(UpgradeLevel)} cannot be negative.");

            //if (!Enum.IsDefined(typeof(CharacterClass), SuitableForClass))
            //    errors.Add($"{nameof(SuitableForClass)} has an invalid value.");

            if (SellValue < 0)
                errors.Add($"{nameof(SellValue)} cannot be negative.");

            if (UpgradeCost < 0)
                errors.Add($"{nameof(UpgradeCost)} cannot be negative.");

            if (errors.Any())
                throw new ValidationException(string.Join(" | ", errors));
        }
    }
}
