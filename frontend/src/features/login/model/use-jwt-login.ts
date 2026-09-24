import { authApi, authQueryKeys } from "@/src/entities/auth";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// JWT-схема: /auth/jwt/login. Токен кладём в zustand; axios-интерсептор
// подставит Authorization: Bearer на последующие запросы.
export function useJwtLogin() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: authApi.jwtLogin,
    onSuccess: (response) => {
      useSessionStore.getState().setJwt(response.accessToken);
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.all });
      toast.success("Вход выполнен");
    },
  });

  return {
    jwtLogin: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
