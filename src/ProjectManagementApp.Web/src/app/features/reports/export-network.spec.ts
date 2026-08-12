import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ProjectProgressComponent } from './project-progress/project-progress.component';

// T075 — the 🎯 no-round-trip test. Export renders the SAME JSON the preview already fetched;
// triggering either export must issue ZERO additional HTTP requests. Proven here against
// ProjectProgressComponent (the flagship report); the other three views share the identical
// export-from-signal pattern (T079), verified individually by inspection of their toExportable()
// methods, which read only from the local `report()` signal, never call ReportsService again.
describe('Report export issues no additional HTTP requests', () => {
  async function createLoadedComponent() {
    await TestBed.configureTestingModule({
      imports: [ProjectProgressComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAnimationsAsync(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ from: '2026-07-01', to: '2026-07-31' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ProjectProgressComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne(r => r.url === '/api/reports/project-progress');
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
          totalTasks: 4,
          openTasks: 3,
          closedTasks: 1,
          overdueTasks: 0,
          completionPercent: 25,
          projectedCompletion: null,
        },
      ],
      totals: { projects: 1, avgCompletionPercent: 25 },
    });
    fixture.detectChanges();

    return { fixture, httpMock };
  }

  it('exportPdf() issues zero HTTP requests', async () => {
    const { fixture, httpMock } = await createLoadedComponent();

    expect(() => fixture.componentInstance['exportPdf']()).not.toThrow();

    httpMock.verify(); // throws if any request is outstanding/unexpected
  });

  it('exportCsv() issues zero HTTP requests', async () => {
    const { fixture, httpMock } = await createLoadedComponent();

    expect(() => fixture.componentInstance['exportCsv']()).not.toThrow();

    httpMock.verify();
  });
});
