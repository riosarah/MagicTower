//@GeneratedCode
import { IdType, IdDefault } from '@app-models/i-key-model';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiEntityBaseService } from '@app-services/api-entity-base.service';
import { environment } from '@environment/environment';
import { IChatMessage } from '@app-models/entities/game/i-chat-message';
//@CustomImportBegin
//@CustomImportEnd
@Injectable({
  providedIn: 'root',
})
export class ChatMessageService extends ApiEntityBaseService<IChatMessage> {
  constructor(public override http: HttpClient) {
    super(http, environment.API_BASE_URL + '/chatmessages');
  }

  public override getItemKey(item: IChatMessage): IdType {
    return item?.id || IdDefault;
  }

//@CustomCodeBegin
//@CustomCodeEnd
}
