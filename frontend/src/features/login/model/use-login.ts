import { authApi } from "@/src/entities/auth";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useLogin() {
  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (account) => {
      useSessionStore.getState().setAccount(account);
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
