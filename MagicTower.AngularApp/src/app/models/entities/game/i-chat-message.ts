//@GeneratedCode
import { IVersionModel } from '@app-models/i-version-model';
import { IGameSession } from '@app-models/entities/game/i-game-session';
//@CustomImportBegin
//@CustomImportEnd
export interface IChatMessage extends IVersionModel {
  gameSessionId: number;
  message: string;
  isAiMessage: boolean;
  sentAt: Date;
  sequenceNumber: number;
  gameSession: IGameSession | null;
//@CustomCodeBegin
//@CustomCodeEnd
}
