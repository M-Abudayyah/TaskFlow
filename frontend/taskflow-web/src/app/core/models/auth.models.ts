export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  roles: string[];
}

export interface CurrentUser {
  userId: string;
  email: string;
  roles: string[];
}
