import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideStore } from '@ngrx/store';
import { provideState } from '@ngrx/store';
import { authFeature } from '../store/auth/auth.feature';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        provideStore(),
        provideState(authFeature),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('several concurrent 401s produce exactly one refresh call', () => {
    httpClient.get('/api/one').subscribe({ error: () => {} });
    httpClient.get('/api/two').subscribe({ error: () => {} });
    httpClient.get('/api/three').subscribe({ error: () => {} });

    const reqOne = httpMock.expectOne('/api/one');
    const reqTwo = httpMock.expectOne('/api/two');
    const reqThree = httpMock.expectOne('/api/three');

    reqOne.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    reqTwo.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    reqThree.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    const refreshRequests = httpMock.match('/api/auth/refresh');
    expect(refreshRequests.length).toBe(1);

    refreshRequests[0].flush({
      accessToken: 'new-token',
      expiresAt: new Date().toISOString(),
      user: { id: '1', fullName: 'Test User', email: 'test@example.com', role: 'TeamMember' },
    });

    httpMock.expectOne('/api/one').flush({});
    httpMock.expectOne('/api/two').flush({});
    httpMock.expectOne('/api/three').flush({});
  });
});
