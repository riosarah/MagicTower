//@GeneratedCode
import { IVersionModel } from '@app-models/i-version-model';
import { Difficulty } from '@app-enums/game/difficulty';
import { IDefeatedEnemy } from '@app-models/entities/game/i-defeated-enemy';
import { IChatMessage } from '@app-models/entities/game/i-chat-message';
import { ICharacter } from '@app-models/entities/game/i-character';
//@CustomImportBegin
//@CustomImportEnd
export interface IGameSession extends IVersionModel {
  characterId: number;
  difficulty:   Difficulty;
  currentFloor: number;
  maxFloor: number;
  isCompleted: boolean;
  startedAt: Date;
  completedAt: Date | null;
  currentEnemyState: string | null;
  character: ICharacter | null;
  defeatedEnemies: IDefeatedEnemy[];
  chatMessages: IChatMessage[];
//@CustomCodeBegin
//@CustomCodeEnd
}
