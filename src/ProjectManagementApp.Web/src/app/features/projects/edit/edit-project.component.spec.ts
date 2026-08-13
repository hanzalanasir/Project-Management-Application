import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideStore } from '@ngrx/store';
import { provideState } from '@ngrx/store';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideNativeDateAdapter } from '@angular/material/core';
import { convertToParamMap } from '@angular/router';
import { authFeature } from '../../../core/store/auth/auth.feature';
import { EditProjectComponent } from './edit-project.component';

describe('EditProjectComponent', () => {
  async function createComponent() {
    await TestBed.configureTestingModule({
      imports: [EditProjectComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideStore(),
        provideState(authFeature),
        provideAnimationsAsync(),
        provideNativeDateAdapter(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'p1' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(EditProjectComponent);
    const httpMock = TestBed.inject(HttpTestingController);

    const getReq = httpMock.expectOne('/api/projects/p1');
    getReq.flush(
      {
        id: 'p1',
        name: 'Original',
        status: 'Planning',
        startDate: '2026-08-01',
        endDate: null,
        owner: { id: 'owner1', fullName: 'Owner' },
        createdAt: '2026-08-01T00:00:00Z',
        updatedAt: '2026-08-01T00:00:00Z',
      },
      { headers: { ETag: '"1"' } }
    );
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('a 409 on save shows the conflict prompt and does not silently retry', async () => {
    const { fixture, httpMock } = await createComponent();

    fixture.componentInstance['form'].patchValue({ name: 'Changed' });
    fixture.componentInstance['submit']();

    const putReq = httpMock.expectOne('/api/projects/p1');
    expect(putReq.request.method).toBe('PUT');
    putReq.flush({ title: 'Conflict', status: 409 }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect(fixture.componentInstance['conflict']()).toBe(true);
    httpMock.verify();
  });

  it('canDeactivate allows navigation without prompting when the form is untouched', async () => {
    const { fixture } = await createComponent();

    expect(fixture.componentInstance.canDeactivate()).toBe(true);
  });
});
