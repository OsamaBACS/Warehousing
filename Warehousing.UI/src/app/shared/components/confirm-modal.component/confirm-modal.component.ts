import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-modal.component',
  standalone: false,
  templateUrl: './confirm-modal.component.html',
  styleUrl: './confirm-modal.component.scss'
})
export class ConfirmModalComponent {

  message!: string;
  cancelBtn!: string;
  confirmBtn!: string;

  @Output() onClose = new EventEmitter<boolean>();

  constructor(public bsModalRef: BsModalRef) {}

  confirm(): void {
    this.onClose.emit(true);
    this.bsModalRef.hide();
  }

  cancel(): void {
    this.onClose.emit(false);
    this.bsModalRef.hide();
  }
}
