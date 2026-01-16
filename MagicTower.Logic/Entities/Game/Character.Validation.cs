//@CustomCode
using MagicTower.Logic.Contracts;
using MagicTower.Common.Modules.Exceptions;
using Microsoft.EntityFrameworkCore;
using MagicTower.Common.Models.Game;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Validation logic for Character entity.
    /// </summary>
    public partial class Character : IValidatableEntity
    {
        private const int MinLevel = 1;
        private const int MaxLevel = 100;
        private const int MinNameLength = 3;
        private const int MaxWeaponCount = 5;

        public void Validate(IContext context, EntityState entityState)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add($"{nameof(Name)} must not be empty.");
            else if (Name.Length < MinNameLength)
                errors.Add($"{nameof(Name)} must be at least {MinNameLength} characters long.");

            if (Level < MinLevel || Level > MaxLevel)
                errors.Add($"{nameof(Level)} must be between {MinLevel} and {MaxLevel}.");

            if (MaxHealth <= 0)
                errors.Add($"{nameof(MaxHealth)} must be greater than 0.");

            if (CurrentHealth < 0)
                errors.Add($"{nameof(CurrentHealth)} cannot be negative.");

            if (CurrentHealth > MaxHealth)
                errors.Add($"{nameof(CurrentHealth)} cannot exceed {nameof(MaxHealth)}.");

            if (AttackPower < 0)
                errors.Add($"{nameof(AttackPower)} cannot be negative.");

            if (SpecialAttackLevel < 1)
                errors.Add($"{nameof(SpecialAttackLevel)} must be at least 1.");

            if (Gold < 0)
                errors.Add($"{nameof(Gold)} cannot be negative.");

           /* if (!Enum.IsDefined(typeof(CharacterClass)
                errors.Add($"{nameof(Class)} has an invalid value.");*/

            if (Weapons.Count > MaxWeaponCount)
                errors.Add($"Character can carry a maximum of {MaxWeaponCount} weapons.");

            if (errors.Any())
                throw new ValidationException(string.Join(" | ", errors));
        }
    }
}
