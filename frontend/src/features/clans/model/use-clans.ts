import {
  clanDetailsQueryOptions,
  clansApi,
  clansListQueryOptions,
  clansQueryKeys,
  type ClansRequest,
} from "@/src/entities/clans";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useClansList(request: ClansRequest) {
  const { data, isPending, isFetching, error } = useQuery(
    clansListQueryOptions(request),
  );

  return {
    clans: data?.items ?? [],
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 1,
    isPending,
    isFetching,
    error,
  };
}

export function useClanDetails(clanId: string) {
  const { data, isPending, isFetching, error } = useQuery(
    clanDetailsQueryOptions(clanId),
  );

  return {
    clan: data ?? null,
    isPending,
    isFetching,
    error,
  };
}

function useInvalidateClans() {
  const queryClient = useQueryClient();

  return () => queryClient.invalidateQueries({ queryKey: clansQueryKeys.all });
}

export function useCreateClan() {
  const invalidate = useInvalidateClans();

  const mutation = useMutation({
    mutationFn: clansApi.createClan,
    onSuccess: () => {
      void invalidate();
      toast.success("Клан создан");
    },
  });

  return {
    createClan: mutation.mutateAsync,
    isPending: mutation.isPending,
    error: mutation.error,
  };
}

export function useJoinClan() {
  const invalidate = useInvalidateClans();

  const mutation = useMutation({
    mutationFn: (clanId: string) => clansApi.joinClan(clanId),
    onSuccess: () => {
      void invalidate();
      toast.success("Вы вступили в клан");
    },
  });

  return {
    joinClan: mutation.mutateAsync,
    isPending: mutation.isPending,
    error: mutation.error,
  };
}

export function useLeaveClan() {
  const invalidate = useInvalidateClans();

  const mutation = useMutation({
    mutationFn: (clanId: string) => clansApi.leaveClan(clanId),
    onSuccess: () => {
      void invalidate();
      toast.success("Вы покинули клан");
    },
  });

  return {
    leaveClan: mutation.mutateAsync,
    isPending: mutation.isPending,
    error: mutation.error,
  };
}

export function useDeleteClan() {
  const invalidate = useInvalidateClans();

  const mutation = useMutation({
    mutationFn: (clanId: string) => clansApi.deleteClan(clanId),
    onSuccess: () => {
      void invalidate();
      toast.success("Клан удалён");
    },
  });

  return {
    deleteClan: mutation.mutateAsync,
    isPending: mutation.isPending,
    error: mutation.error,
  };
}

export function useKickMember() {
  const invalidate = useInvalidateClans();

  const mutation = useMutation({
    mutationFn: ({ clanId, userId }: { clanId: string; userId: string }) =>
      clansApi.kickMember(clanId, userId),
    onSuccess: () => {
      void invalidate();
      toast.success("Участник исключён из клана");
    },
  });

  return {
    kickMember: mutation.mutateAsync,
    isPending: mutation.isPending,
    error: mutation.error,
  };
}
