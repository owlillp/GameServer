import { authApi } from "@/src/entities/auth";
import { startLogin } from "@/src/shared/auth/oidc";
import { useMutation } from "@tanstack/react-query";

// Логин: /auth/login ставит Identity-cookie, затем уходим на /connect/authorize.
export function useLogin() {
  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: async () => {
      await startLogin("/profile");
    },
  });

  return {
    login: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
