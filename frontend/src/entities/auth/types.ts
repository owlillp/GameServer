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

export type LoginResponse = {
  accountId: string;
  email: string;
  userName: string;
};
