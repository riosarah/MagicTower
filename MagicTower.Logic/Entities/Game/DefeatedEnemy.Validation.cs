#if GENERATEDCODE_ON
using MagicTower.Logic.Contracts;
using MagicTower.Common.Modules.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MagicTower.Logic.Entities.Game
{
    /// <summary>
    /// Validation logic for DefeatedEnemy entity.
    /// </summary>
    public partial class DefeatedEnemy : IValidatableEntity
    {
        private const int MinFloor = 1;

        public void Validate(IContext context, EntityState entityState)
        {
            var errors = new List<string>();

            if (GameSessionId <= 0)
                errors.Add($"{nameof(GameSessionId)} must be a valid reference.");

            if (FloorNumber < MinFloor)
                errors.Add($"{nameof(FloorNumber)} must be at least {MinFloor}.");

            if (string.IsNullOrWhiteSpace(EnemyType))
                errors.Add($"{nameof(EnemyType)} must not be empty.");

            if (string.IsNullOrWhiteSpace(EnemyRace))
                errors.Add($"{nameof(EnemyRace)} must not be empty.");

            if (EnemyLevel < 1)
                errors.Add($"{nameof(EnemyLevel)} must be at least 1.");

            if (string.IsNullOrWhiteSpace(EnemyWeapon))
                errors.Add($"{nameof(EnemyWeapon)} must not be empty.");

            if (GoldReward < 0)
                errors.Add($"{nameof(GoldReward)} cannot be negative.");

            if (errors.Any())
                throw new ValidationException(string.Join(" | ", errors));
        }
    }
}
#endif