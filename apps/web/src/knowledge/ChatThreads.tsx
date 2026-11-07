import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiJson } from "../../../../shared/frontend/auth-client";

export type Thread = { id: string; title: string; updatedAt: string };

export const chatThreadsPath = "/api/knowledge/chat/threads";
const threadPageSize = 30;

type ChatThreadsValue = {
  threads: Thread[];
  hasMore: boolean;
  error: string;
  refresh(): Promise<void>;
  loadMore(): Promise<void>;
  remove(id: string): Promise<void>;
};

const ChatThreadsContext = createContext<ChatThreadsValue | null>(null);

/** Shared by the conversation and its history menu. */
export function ChatThreadsProvider({ children }: { children: ReactNode }) {
  const [threads, setThreads] = useState<Thread[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  const [error, setError] = useState("");

  const load = useCallback(async (number: number) => {
    const items = await apiJson<Thread[]>(`${chatThreadsPath}?page=${number}`);
    setThreads(old => number === 1 ? items : [...old, ...items.filter(item => old.every(thread => thread.id !== item.id))]);
    setPage(number);
    setHasMore(items.length === threadPageSize);
    setError("");
  }, []);

  useEffect(() => {
    void load(1).catch(() => setError("Chưa tải được lịch sử trò chuyện."));
  }, [load]);

  const value = useMemo<ChatThreadsValue>(() => ({
    threads,
    hasMore,
    error,
    refresh: () => load(1),
    loadMore: () => load(page + 1),
    async remove(id) {
      await apiJson(`${chatThreadsPath}/${id}`, { method: "DELETE" });
      await load(1);
    },
  }), [error, hasMore, load, page, threads]);

  return <ChatThreadsContext.Provider value={value}>{children}</ChatThreadsContext.Provider>;
}

export function useChatThreads() {
  const value = useContext(ChatThreadsContext);
  if (!value) throw new Error("useChatThreads must be used inside ChatThreadsProvider");
  return value;
}
