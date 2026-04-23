import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';
import { CurrentUser, LoginRequest, LoginResponse } from '../models/auth.models';
import { apiConfig } from '../config/api.config';

const TOKEN_KEY = 'taskflow_token';
const USER_KEY = 'taskflow_user';
type CurrentSession = CurrentUser & { token: string };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly loginUrl = `${apiConfig.baseUrl}/api/auth/login`;
  readonly currentUser = signal<CurrentUser | null>(this.loadStoredUser());

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.http.post<LoginResponse>(this.loginUrl, request).pipe(
      map((response) => this.toSession(response)),
      tap((session) => this.persistSession(session)),
      map((session) => ({ userId: session.userId, email: session.email, roles: session.roles }))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  hasRole(role: string): boolean {
    const user = this.currentUser();
    return !!user && user.roles.includes(role);
  }

  getCurrentUser(): CurrentUser | null {
    return this.currentUser();
  }

  private persistSession(session: CurrentSession): void {
    localStorage.setItem(TOKEN_KEY, session.token);
    localStorage.setItem(
      USER_KEY,
      JSON.stringify({ userId: session.userId, email: session.email, roles: session.roles })
    );
    this.currentUser.set({ userId: session.userId, email: session.email, roles: session.roles });
  }

  private toSession(response: LoginResponse): CurrentSession {
    const userId = this.getUserIdFromToken(response.token);
    return {
      userId,
      email: response.email,
      roles: response.roles,
      token: response.token
    };
  }

  private loadStoredUser(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      localStorage.removeItem(USER_KEY);
      return null;
    }
  }

  private getUserIdFromToken(token: string): string {
    try {
      const payloadPart = token.split('.')[1];
      const payloadJson = atob(payloadPart.replace(/-/g, '+').replace(/_/g, '/'));
      const payload = JSON.parse(payloadJson) as Record<string, string>;
      return payload['nameid'] ?? payload['sub'] ?? '';
    } catch {
      return '';
    }
  }
}
