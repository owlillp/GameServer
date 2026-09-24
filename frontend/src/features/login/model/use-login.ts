import { authApi, authQueryKeys } from "@/src/entities/auth";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

// Cookie-схема: /auth/login + Identity-cookie от сервера.
export function useLogin() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (account) => {
      useSessionStore.getState().setAccount(account);
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.all });
      toast.success(`С возвращением, ${account.userName}!`);
    },
  });

  return {
    login: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
