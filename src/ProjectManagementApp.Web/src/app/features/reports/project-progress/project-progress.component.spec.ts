import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ProjectProgressComponent } from './project-progress.component';

// T041 — the flagship report renders exactly what the API returns, including the empty-rows case
// (a caller with no visible projects still gets a real, non-error page).
describe('ProjectProgressComponent', () => {
  async function createComponent(queryParams: Record<string, string>) {
    await TestBed.configureTestingModule({
      imports: [ProjectProgressComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAnimationsAsync(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ProjectProgressComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne(r => r.url === '/api/reports/project-progress');
    return { fixture, httpMock, req };
  }

  it('renders the empty state when the caller has no visible projects', async () => {
    const { fixture, httpMock, req } = await createComponent({ from: '2026-07-01', to: '2026-07-31' });

    req.flush({
      reportType: 'ProjectProgress',
      generatedAt: new Date().toISOString(),
      scope: 'ProjectManager',
      window: { from: '2026-07-01', to: '2026-07-31' },
      timeZone: 'UTC',
      rows: [],
      totals: { projects: 0, avgCompletionPercent: 0 },
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No visible projects for this window.');
    httpMock.verify();
  });

  it('renders a row with its completion percent', async () => {
    const { fixture, httpMock, req } = await createComponent({ from: '2026-07-01', to: '2026-07-31' });

    req.flush({
      reportType: 'ProjectProgress',
      generatedAt: new Date().toISOString(),
      scope: 'ProjectManager',
      window: { from: '2026-07-01', to: '2026-07-31' },
      timeZone: 'UTC',
      rows: [
        {
          projectId: '11111111-1111-1111-1111-111111111111',
          projectName: 'Alpha',
          status: 'Active',
          totalTasks: 12,
          openTasks: 9,
          closedTasks: 3,
          overdueTasks: 1,
          completionPercent: 25,
          projectedCompletion: null,
        },
      ],
      totals: { projects: 1, avgCompletionPercent: 25 },
    });
    fixture.detectChanges();

    const cell = fixture.debugElement.query(By.css('td'));
    expect(fixture.nativeElement.textContent).toContain('Alpha');
    expect(fixture.nativeElement.textContent).toContain('25');
    void cell;
    httpMock.verify();
  });
});
