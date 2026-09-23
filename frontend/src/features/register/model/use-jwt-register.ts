import { authApi, authQueryKeys } from "@/src/entities/auth";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// JWT-регистрация не выдаёт токен — пользователь затем входит через /auth/jwt/login.
export function useJwtRegister() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: authApi.jwtRegister,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.all });
      toast.success("Аккаунт создан. Войдите в систему.");
    },
  });

  return {
    jwtRegister: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
