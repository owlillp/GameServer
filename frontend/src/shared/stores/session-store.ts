import { create } from "zustand";
import { persist } from "zustand/middleware";

export type SessionAccount = {
  accountId: string;
  email: string;
  userName: string;
};

type SessionData = {
  account: SessionAccount | null;
};

type SessionActions = {
  setAccount: (account: SessionAccount) => void;
  clear: () => void;
};

export type SessionStore = SessionData & SessionActions;

export const useSessionStore = create<SessionStore>()(
  persist(
    (set) => ({
      account: null,
      setAccount: (account) => set({ account }),
      clear: () => set({ account: null }),
    }),
    { name: "gameserver-session" },
  ),
);

export const sessionSelectors = {
  account: (state: SessionStore) => state.account,
  isAuthenticated: (state: SessionStore) => state.account !== null,
  userName: (state: SessionStore) => state.account?.userName ?? null,
  setAccount: (state: SessionStore) => state.setAccount,
  clear: (state: SessionStore) => state.clear,
} as const;
