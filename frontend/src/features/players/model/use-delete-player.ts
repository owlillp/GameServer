import { playersApi, playersQueryOptions } from "@/src/entities/players/api";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeletePlayer() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: playersApi.deletePlayer,
    onSettled: () =>
      queryClient.invalidateQueries({
        queryKey: [playersQueryOptions.baseKey],
      }),
    onSuccess: () => {
      toast.success("Игрок удалён");
    },
  });

  return {
    deletePlayer: mutation.mutate,
    isPending: mutation.isPending,
  };
}
