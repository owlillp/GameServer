import { playersQueryOptions } from "@/src/entities/players/api";
import { Player } from "@/src/entities/players/types";
import { useQuery } from "@tanstack/react-query";

type UsePlayersResult = {
  players: Player[];
  totalCount: number;
  isPending: boolean;
  isFetching: boolean;
  error: string | null;
  refetch: () => void;
};

export function usePlayersList(page = 1, pageSize = 20): UsePlayersResult {
  const { data, isPending, isFetching, error, refetch } = useQuery(
    playersQueryOptions.getPlayersOptions({ page, pageSize }),
  );

  return {
    players: data?.records ?? [],
    totalCount: data?.totalCount ?? 0,
    isPending,
    isFetching,
    error: error?.message ?? null,
    refetch,
  };
}
