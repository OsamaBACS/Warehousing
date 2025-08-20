import { Component, EventEmitter, forwardRef, Input, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-image-uploader',
  standalone: false,
  templateUrl: './image-uploader.html',
  styleUrl: './image-uploader.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ImageUploader),
      multi: true
    }
  ]
})
export class ImageUploader implements ControlValueAccessor {
  @Input() initialImageUrl: string | null = null;
  @Input() readonly: boolean = false;
  @Output() imageSelected = new EventEmitter<File>();

  imageToShow: string | ArrayBuffer | null = null;
  file?: File;
  isDragging = false;

  ngOnInit(): void {
    if (this.initialImageUrl) {
      this.imageToShow = this.initialImageUrl;
    }
  }

  private onChange: (_: any) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: any): void {
    if (typeof value === 'string') {
      this.imageToShow = value;
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.readonly = isDisabled;
  }

  triggerFileInput(fileInput: HTMLInputElement): void {
    if (!this.readonly) {
      fileInput.click();
    }
  }

  onFileChange(event: Event): void {
    if (this.readonly) return;
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.previewFile(file);
      this.setFile(file);
    }
  }

  onDragOver(event: DragEvent): void {
    if (this.readonly) return;
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    if (this.readonly) return;
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    if (this.readonly) return;
    event.preventDefault();
    this.isDragging = false;

    if (event.dataTransfer?.files.length) {
      const file = event.dataTransfer.files[0];
      this.previewFile(file);
      this.setFile(file);
    }
  }

  private previewFile(file: File): void {
    const reader = new FileReader();
    reader.onload = () => {
      this.imageToShow = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  private setFile(file: File): void {
    this.file = file;
    this.imageSelected.emit(file);
    this.onChange(file);
    const reader = new FileReader();
    reader.onload = () => (this.imageToShow = reader.result);
    reader.readAsDataURL(file);
  }
}
