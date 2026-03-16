//@GeneratedCode
import { Directive } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IdType, IdDefault, IKeyModel } from '@app/models/i-key-model';
import { GenericEditComponent } from '@app/components/base/generic-edit.component';
import { IChatMessage } from '@app-models/entities/game/i-chat-message';
//@CustomImportBegin
//@CustomImportEnd
@Directive()
export abstract class ChatMessageBaseEditComponent extends GenericEditComponent<IChatMessage> {
  constructor()
  {
    super();
  }

  public override getItemKey(item: IChatMessage): IdType {
    return item?.id || IdDefault;
  }

  public override get title(): string {
    return 'ChatMessage' + super.title;
  }
//@CustomCodeBegin
//@CustomCodeEnd
}
