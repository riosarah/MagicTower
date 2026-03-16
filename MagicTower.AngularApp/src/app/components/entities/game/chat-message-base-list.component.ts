//@GeneratedCode
import { Directive, inject } from '@angular/core';
import { GenericEntityListComponent } from '@app/components/base/generic-entity-list.component';
import { IChatMessage } from '@app-models/entities/game/i-chat-message';
import { ChatMessageService } from '@app-services/http/entities/game/chat-message-service';
//@CustomImportBegin
//@CustomImportEnd
@Directive()
export abstract class ChatMessageBaseListComponent extends GenericEntityListComponent<IChatMessage> {
  constructor()
  {
    super(inject(ChatMessageService));
  }
  override ngOnInit(): void {
    super.ngOnInit();
  }
//@CustomCodeBegin
//@CustomCodeEnd
}
