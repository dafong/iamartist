import { create } from "zustand";

export interface GlobalState {
  busy: boolean;
  keyCount: number;
  setBusy: (value: boolean) => void;
  incrementKeyCount: () => void;
}

export const useGlobalStore = create<GlobalState>()((set) => ({
  busy: false,
  keyCount: 0,
  setBusy: (value: boolean) => set({ busy: value }),
  incrementKeyCount: () => set((state) => ({ keyCount: state.keyCount + 1 })),
}));

export const useBusy = () => useGlobalStore((state) => state.busy);
export const useSetBusy = () => useGlobalStore((state) => state.setBusy);
export const useKeyCount = () => useGlobalStore((state) => state.keyCount);
export const useIncrementKeyCount = () => useGlobalStore((state) => state.incrementKeyCount);
