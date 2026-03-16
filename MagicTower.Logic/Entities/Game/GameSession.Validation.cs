#if GENERATEDCODE_ON
////@CustomCode
//using MagicTower.Logic.Contracts;
//using MagicTower.Common.Modules.Exceptions;
//using Microsoft.EntityFrameworkCore;

//namespace MagicTower.Logic.Entities.Game
//{
//    /// <summary>
//    /// Validation logic for GameSession entity.
//    /// </summary>
//    public partial class GameSession : IValidatableEntity
//    {
//        private const int MinFloor = 1;

//        public void Validate(IContext context, EntityState entityState)
//        {
//            var errors = new List<string>();

//            if (CharacterId <= 0)
//                errors.Add($"{nameof(CharacterId)} must be a valid reference.");

//            //if (!Enum.IsDefined(typeof(Diffi), Difficulty))
//            //    errors.Add($"{nameof(Difficulty)} has an invalid value.");

//            if (CurrentFloor < MinFloor)
//                errors.Add($"{nameof(CurrentFloor)} must be at least {MinFloor}.");

//            if (MaxFloor < MinFloor)
//                errors.Add($"{nameof(MaxFloor)} must be at least {MinFloor}.");

//            if (CurrentFloor > MaxFloor)
//                errors.Add($"{nameof(CurrentFloor)} cannot exceed {nameof(MaxFloor)}.");

//            if (MaxFloor != (int)Difficulty)
//                errors.Add($"{nameof(MaxFloor)} must match the difficulty level ({(int)Difficulty} floors for {Difficulty}).");

//            if (IsCompleted && CompletedAt == null)
//                errors.Add($"{nameof(CompletedAt)} must be set when session is completed.");

//            if (!IsCompleted && CompletedAt != null)
//                errors.Add($"{nameof(CompletedAt)} should only be set when session is completed.");

//            if (CompletedAt.HasValue && CompletedAt.Value < StartedAt)
//                errors.Add($"{nameof(CompletedAt)} cannot be earlier than {nameof(StartedAt)}.");

//            if (errors.Any())
//                throw new ValidationException(string.Join(" | ", errors));
//        }
//    }
//}
#endif