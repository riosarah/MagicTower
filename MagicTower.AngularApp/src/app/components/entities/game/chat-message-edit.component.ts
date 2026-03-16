//@CustomCode
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ChatMessageBaseEditComponent }from '@app/components/entities/game/chat-message-base-edit.component';
@Component({
  standalone: true,
  selector:'app-chat-message-edit',
  imports: [ CommonModule, FormsModule, TranslateModule],
  templateUrl: './chat-message-edit.component.html',
  styleUrl: './chat-message-edit.component.css'
})
export class ChatMessageEditComponent extends ChatMessageBaseEditComponent {
}
