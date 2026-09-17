import { playersApi, playersQueryOptions } from "@/src/entities/players/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useCreatePlayer() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: playersApi.createPlayer,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [playersQueryOptions.baseKey],
      }),
    onSuccess: () => {
      toast.success("Игрок успешно создан");
    },
  });

  return {
    createPlayer: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
