import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CreateTaskComponent } from './create-task.component';

describe('CreateTaskComponent', () => {
  async function createComponent() {
    await TestBed.configureTestingModule({
      imports: [CreateTaskComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), provideAnimationsAsync()],
    }).compileComponents();

    const fixture = TestBed.createComponent(CreateTaskComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('title is required', async () => {
    const fixture = await createComponent();
    const titleControl = fixture.componentInstance['form'].get('title')!;

    titleControl.setValue('');

    expect(titleControl.hasError('required')).toBe(true);
  });

  it('title exceeding the max length is invalid', async () => {
    const fixture = await createComponent();
    const titleControl = fixture.componentInstance['form'].get('title')!;

    titleControl.setValue('a'.repeat(201));

    expect(titleControl.hasError('maxlength')).toBe(true);
  });

  it('description exceeding the max length is invalid', async () => {
    const fixture = await createComponent();
    const descriptionControl = fixture.componentInstance['form'].get('description')!;

    descriptionControl.setValue('a'.repeat(2001));

    expect(descriptionControl.hasError('maxlength')).toBe(true);
  });

  it('defaults priority to Medium', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    expect(form.get('priority')?.value).toBe('Medium');
  });

  it('a blank title fails the cross-field required-fields validator', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    form.patchValue({ title: '   ' });

    expect(form.hasError('titleRequired')).toBe(true);
  });

  it('a populated title passes the cross-field required-fields validator', async () => {
    const fixture = await createComponent();
    const form = fixture.componentInstance['form'];

    form.patchValue({ title: 'Draft rollout checklist' });

    expect(form.hasError('titleRequired')).toBe(false);
  });
});
