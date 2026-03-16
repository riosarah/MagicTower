import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'lineBreaks',
  standalone: false
})
export class LineBreaksPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';
    // Convert \n to <br> tags
    return value.replace(/\n/g, '<br>');
  }
}
