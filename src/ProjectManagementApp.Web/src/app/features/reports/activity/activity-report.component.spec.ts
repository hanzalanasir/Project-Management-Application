import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ActivityReportComponent } from './activity-report.component';

// T077 — the 🎯 422-blocks-export test. An over-threshold window must surface the "narrow your
// range" prompt and NEVER attempt a client render — no table, and critically no export controls,
// since there is no previewed data for either export button to render from.
describe('ActivityReportComponent', () => {
  async function createComponent() {
    await TestBed.configureTestingModule({
      imports: [ActivityReportComponent],
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

    const fixture = TestBed.createComponent(ActivityReportComponent);
    fixture.detectChanges();

    const httpMock = TestBed.inject(HttpTestingController);
    const req = httpMock.expectOne(r => r.url === '/api/reports/activity');
    return { fixture, httpMock, req };
  }

  it('a 422 response shows the narrow-range prompt, never a table or export controls', async () => {
    const { fixture, httpMock, req } = await createComponent();

    req.flush(
      { title: 'Report window too large', status: 422, detail: 'Narrow the date range.' },
      { status: 422, statusText: 'Unprocessable Entity' }
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Narrow the date range');
    expect(fixture.debugElement.query(By.css('table'))).toBeNull();
    expect(fixture.debugElement.query(By.css('.export-controls'))).toBeNull();
    expect(fixture.componentInstance['report']()).toBeNull();

    httpMock.verify();
  });

  it('a normal 200 response shows export controls alongside the table', async () => {
    const { fixture, httpMock, req } = await createComponent();

    req.flush({
      reportType: 'Activity',
      generatedAt: new Date().toISOString(),
      scope: 'ProjectManager',
      window: { from: '2026-07-01', to: '2026-07-31' },
      timeZone: 'UTC',
      items: [
        { id: '1', timestamp: new Date().toISOString(), actorId: null, actorName: 'System', action: 'TaskCreated', entityType: 'Task', entityId: 't1', changeSummary: 'Created' },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    });
    fixture.detectChanges();

    expect(fixture.debugElement.query(By.css('.export-controls'))).not.toBeNull();

    httpMock.verify();
  });
});
