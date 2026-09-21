import axios from "axios";

export const ErrorType = {
  VALIDATION: "VALIDATION",
  NOT_FOUND: "NOT_FOUND",
  FAILURE: "FAILURE",
  CONFLICT: "CONFLICT",
  AUTHENTICATION: "AUTHENTICATION",
  AUTHORIZATION: "AUTHORIZATION",
} as const;

export type ErrorType = (typeof ErrorType)[keyof typeof ErrorType];

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
  isCritical?: boolean;
};

export class EnvelopeError extends Error {
  public readonly apiError: ApiError;
  public readonly type: ErrorType;

  constructor(apiError: ApiError) {
    super(apiError.messages[0]?.message ?? "Неизвестная ошибка");

    this.name = "EnvelopeError";
    this.apiError = apiError;
    this.type = apiError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get messages(): ErrorMessage[] {
    return this.apiError.messages;
  }

  get firstError(): ErrorMessage | undefined {
    return this.apiError.messages[0];
  }

  get allMessages(): string[] {
    return this.apiError.messages.map((error) => error.message);
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}

export function getErrorMessage(error: unknown, fallback: string): string {
  if (isEnvelopeError(error)) {
    return error.firstError?.message || fallback;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}

export function isForbiddenError(error: unknown): boolean {
  if (isEnvelopeError(error)) {
    return error.type === ErrorType.AUTHORIZATION;
  }

  return axios.isAxiosError(error) && error.response?.status === 403;
}
