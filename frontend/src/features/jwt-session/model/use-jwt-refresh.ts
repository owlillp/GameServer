import { authApi, authQueryKeys } from "@/src/entities/auth";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// Ручное обновление access-токена по HttpOnly refresh-cookie.
export function useJwtRefresh() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: authApi.jwtRefresh,
    onSuccess: (response) => {
      useSessionStore.getState().setJwt(response.accessToken);
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.all });
      toast.success("Access-токен обновлён");
    },
    onError: () => {
      toast.error("Не удалось обновить токен");
    },
  });

  return {
    refresh: mutation.mutate,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
