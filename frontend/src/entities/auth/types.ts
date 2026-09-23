// Зеркалит контракты AuthService.Contracts (.NET PropertyNamingPolicy = camelCase).

export type RegisterRequest = {
  email: string;
  userName: string;
  password: string;
};

export type RegisterResponse = {
  accountId: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

// Cookie-схема: /auth/login. Тело ответа + сервер ставит Identity-cookie.
export type LoginResponse = {
  accountId: string;
  email: string;
  userName: string;
};

// JWT-схема: /auth/jwt/login.
export type JwtLoginResponse = {
  accessToken: string;
  expiresAt: string;
};

export type ProfileBody = {
  age: number | null;
  bio: string | null;
  location: string | null;
};

// GET /auth/profile — работает и под cookie, и под Bearer.
export type ProfileResponse = {
  id: string;
  email: string | null;
  username: string | null;
  displayName: string | null;
  createdAt: string;
  updatedAt: string;
  profile: ProfileBody;
};
